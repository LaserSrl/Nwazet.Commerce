using Nwazet.Commerce.Services;
using Nwazet.Commerce.ViewModels;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.UI.Notify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Commerce")]
    public abstract class ShoppingCartBase : IShoppingCart {

        protected readonly IContentManager _contentManager;
        protected readonly IShoppingCartStorage _cartStorage;
        protected readonly IPriceService _priceService;
        protected readonly IEnumerable<IProductAttributesDriver> _attributesDrivers;
        protected readonly IEnumerable<ITaxProvider> _taxProviders;
        protected readonly INotifier _notifier;
        protected readonly ITaxProviderService _taxProviderService;
        protected readonly IProductPriceService _productPriceService;
        protected readonly IEnumerable<ICartPriceAlterationProcessor> _cartPriceAlterationProcessors;
        protected readonly IWorkContextAccessor _workContextAccessor;

        protected IEnumerable<ShoppingCartQuantityProduct> _products;

        public Localizer T { get; set; }

        public ShoppingCartBase(
            IContentManager contentManager,
            IShoppingCartStorage cartStorage,
            IPriceService priceService,
            IEnumerable<IProductAttributesDriver> attributesDrivers,
            IEnumerable<ITaxProvider> taxProviders,
            INotifier notifier,
            ITaxProviderService taxProviderService,
            IProductPriceService productPriceService,
            IEnumerable<ICartPriceAlterationProcessor> cartPriceAlterationProcessors,
            IWorkContextAccessor workContextAccessor) {

            _contentManager = contentManager;
            _cartStorage = cartStorage;
            _priceService = priceService;
            _attributesDrivers = attributesDrivers;
            _taxProviders = taxProviders;
            _notifier = notifier;
            _taxProviderService = taxProviderService;
            _productPriceService = productPriceService;
            _cartPriceAlterationProcessors = cartPriceAlterationProcessors;
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
        }

        public virtual string Country {
            get { return _cartStorage.Country; }
            set { _cartStorage.Country = value; }
        }
        public virtual IEnumerable<ShoppingCartItem> Items { get; }
        public virtual ShippingOption ShippingOption {
            get { return _cartStorage.ShippingOption; }
            set { _cartStorage.ShippingOption = value; }
        }
        public virtual string ZipCode {
            get { return _cartStorage.ZipCode; }
            set { _cartStorage.ZipCode = value; }
        }

        // These are things that modify the cart's final total,
        // but not its subtotal. They may increase or decrease
        // the total. Practical example of these: coupons. Each
        // one of these has its own provider and rules.
        public virtual List<CartPriceAlteration> PriceAlterations {
            get { return _cartStorage
                    .PriceAlterations
                    .Where(cpa => _cartPriceAlterationProcessors.Any(p => p.CanProcess(cpa)))
                    .OrderByDescending(cpa => cpa.Weight)
                    .ToList(); }
            set { _cartStorage.PriceAlterations = value
                    // only keep the alterations that can be processed
                    .Where(cpa => _cartPriceAlterationProcessors.Any(p => p.CanProcess(cpa)))
                    .ToList(); }
        }
        public IEnumerable<CartPriceAlterationAmount> PriceAlterationAmounts {
            get {
                // Each alteration may affect the computation for the next.
                // PriceAlterations is already ordered by descending Weight.
                var alterationContext = new CartPriceAlterationContext {
                    ShoppingCart = this,
                    WorkContext = _workContextAccessor.GetContext()
                };
                var amounts = new List<CartPriceAlterationAmount>();
                foreach (var alteration in PriceAlterations) {
                    alterationContext.SetAlteration(alteration);
                    // get processors that are able to process the alteration
                    var processors = _cartPriceAlterationProcessors
                        .Where(p => p.CanProcess(alteration));
                    // process them one by one 
                    foreach (var processor in processors) {
                        amounts.Add(new CartPriceAlterationAmount() {
                            Label = processor.AlterationLabel(alteration, this),
                            Amount = processor.AlterationAmount(alterationContext),
                            AlterationType = alteration.AlterationType,
                            Key = alteration.Key,
                            Weight = alteration.Weight,
                            RemovalAction = alteration.RemovalAction
                        });
                    }
                }
                return amounts;
            }
        }

        public abstract bool TryAdd(int productId, int quantity = 1, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues = null);
        public abstract void Add(int productId, int quantity = 1, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues = null);
        public virtual void AddRange(IEnumerable<ShoppingCartItem> items) {
            foreach (var item in items) {
                Add(item.ProductId, item.Quantity, item.AttributeIdsToValues);
            }
        }
        public abstract void Clear();
        public abstract void ClearAll();
        public abstract ShoppingCartItem FindCartItem(int productId, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues);
        public virtual IEnumerable<ShoppingCartQuantityProduct> GetProducts() {
            if (_products != null) return _products;

            var ids = Items.Select(x => x.ProductId);

            var productParts =
                _contentManager.GetMany<ProductPart>(ids, VersionOptions.Published,
                new QueryHints().ExpandParts<TitlePart, ProductPart, AutoroutePart>()).ToArray();

            var productPartIds = productParts.Select(p => p.Id);

            var shoppingCartQuantities =
                (from item in Items
                 where productPartIds.Contains(item.ProductId) && item.Quantity > 0
                 select new ShoppingCartQuantityProduct(item.Quantity,
                        productParts.First(p => p.Id == item.ProductId),
                        item.AttributeIdsToValues))
                    .ToList();

            return _products = shoppingCartQuantities
                .Select(q => _priceService.GetDiscountedPrice(q, shoppingCartQuantities))
                .Where(q => q.Quantity > 0)
                .ToList();
        }
        public virtual double ItemCount() {
            return Items.Sum(x => x.Quantity);
        }
        public abstract void Remove(int productId, IDictionary<int, ProductAttributeValueExtended> attributeIdsToValues = null);
        public virtual decimal Subtotal() {
            return Math.Round(
                GetProducts()
                .Sum(pq => 
                    Math.Round(
                        _productPriceService.GetPrice(pq.Product, pq.Price, Country, ZipCode) 
                        * pq.Quantity + pq.LinePriceAdjustment, 2)), 2);
        }
        public virtual TaxAmount Taxes(decimal subTotal = 0) {
            //if (Country == null && ZipCode == null) return null;
            var taxes = _taxProviders
                .SelectMany(p => p.GetTaxes())
                .OrderByDescending(t => t.Priority);
            var shippingPrice = ShippingOption == null ? 0 : ShippingOption.Price;
            if (subTotal.Equals(0)) {
                subTotal = Subtotal();
            }
            var taxContext = _taxProviderService.CreateContext(GetProducts(), subTotal, shippingPrice, Country, ZipCode);
            return (
                from tax in taxes
                let name = tax.Name
                let amount = _taxProviderService.TotalTaxes(tax, taxContext)
                where amount > 0
                select new TaxAmount { Name = name, Amount = amount }
                ).FirstOrDefault() ?? new TaxAmount { Amount = 0, Name = null };
        }
        public virtual decimal Total(decimal subTotal = 0, TaxAmount taxes = null) {
            if (taxes == null) {
                taxes = Taxes();
            }
            if (subTotal.Equals(0)) {
                subTotal = Subtotal();
            }

            var taxAmount = taxes?.Amount ?? 0m;
            taxAmount = taxAmount <= 0m ? 0m : taxAmount;

            return subTotal 
                + taxAmount
                + PriceAlterationAmounts.Sum(aa => aa.Amount)
                + (ShippingOption?.Price ?? 0m);
        }
        public abstract void UpdateItems();
    }
}
