using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels.Combinations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Services.Combinations {
    [OrchardFeature("Nwazet.ProductCombinations")]
    public class ProductPartDetailProvider : 
        BaseCombinationDetailProvider,
        // Combinations/Variants may share a "single" SKU
        ISKUUniquenessHelper {

        private readonly ICurrencyProvider _currencyProvider;
        private readonly IProductPriceService _productPriceService;
        private readonly IPriceService _priceService;

        public ProductPartDetailProvider (
            IContentDefinitionManager contentDefinitionManager,
            ICurrencyProvider currencyProvider,
            IProductPriceService productPriceService,
            IPriceService priceService) 
            : base(
                contentDefinitionManager) {

            _currencyProvider = currencyProvider;
            _productPriceService = productPriceService;
            _priceService = priceService;
        }

        public override ContentTypeDefinitionBuilder AlterCombinationDefinition(
            ContentTypeDefinitionBuilder previous,
            string containerTypeName) {

            var containerDefinition = _contentDefinitionManager.GetTypeDefinition(containerTypeName);

            if (containerDefinition != null 
                && containerDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProductPart")) {
                // alter the definition
                return previous.WithPart("ProductPart");
            }
            return previous;
        }

        public override void Synchronize(
            CombinationContainerPart container, CombinationPart combination) {

            var sourceProductPart = container.As<ProductPart>();
            var targetProductPart = combination.As<ProductPart>();
            if (sourceProductPart != null && targetProductPart != null) {
                // Simply copy all properties from the container to the 
                // combination.
                targetProductPart.Sku = sourceProductPart.Sku;
                targetProductPart.Price = sourceProductPart.Price;
                targetProductPart.DiscountPrice = sourceProductPart.DiscountPrice;
                targetProductPart.ShippingCost = sourceProductPart.ShippingCost;
                targetProductPart.Weight = sourceProductPart.Weight;
                targetProductPart.Size = sourceProductPart.Size;
                targetProductPart.OverrideTieredPricing = sourceProductPart.OverrideTieredPricing;
                targetProductPart.PriceTiers = sourceProductPart.PriceTiers;
                targetProductPart.AuthenticationRequired = sourceProductPart.AuthenticationRequired;
                targetProductPart.IsDigital = sourceProductPart.IsDigital;
            }
        }

        public override IEnumerable<CombinationDetailShape> GetCombinationDetailShapes(
            CombinationPart part, 
            dynamic shapeHelper) {

            var productPart = part.As<ProductPart>();
            if (productPart != null) {
                var details = new List<CombinationDetailShape>();
                // In the shapes for prices, we pass some stuff same as is being done in 
                // ProductPartDriver, to make it easier to do the alternates here.
                var discountedPriceQuantity = _priceService
                    .GetDiscountedPrice(new ShoppingCartQuantityProduct(1, productPart));

                details.Add(new CombinationDetailShape {
                    RoleKey = "product-discounted-price",
                    Shape = shapeHelper.Combinations_ProductDiscountedPrice(
                        ContentItem: productPart.ContentItem,
                        ProductPart: productPart,
                        CombinationPart: part,
                        CurrencyProvider: _currencyProvider,
                        Price: _productPriceService.GetPrice(productPart),
                        DiscountedPrice: _productPriceService
                            .GetPrice(productPart, discountedPriceQuantity.Price),
                        DiscountComment: discountedPriceQuantity.Comment)
                });
                details.Add(new CombinationDetailShape {
                    RoleKey = "product-price",
                    Shape = shapeHelper.Combinations_ProductPrice(
                        ContentItem: productPart.ContentItem,
                        ProductPart: productPart,
                        CombinationPart: part,
                        CurrencyProvider: _currencyProvider,
                        Price: _productPriceService.GetPrice(productPart),
                        DiscountedPrice: _productPriceService
                            .GetPrice(productPart, discountedPriceQuantity.Price),
                        DiscountComment: discountedPriceQuantity.Comment)
                });

                return details;
            }

            return Enumerable.Empty<CombinationDetailShape>();
        }

        #region ISKUUniquenessHelper
        public IEnumerable<int> GetIdsOfValidSKUDuplicates(ProductPart part) {
            var container = part.As<CombinationContainerPart>();
            if (container != null) {
                return GetIdsOfValidSKUDuplicates(container);
            }
            var combination = part.As<CombinationPart>();
            if (combination != null) {
                return GetIdsOfValidSKUDuplicates(combination);
            }
            return Enumerable.Empty<int>();
        }

        private List<int> GetIdsOfValidSKUDuplicates(CombinationContainerPart part) {
            return part.CombinationParts.Select(cp => cp.Id).ToList();
        }

        private List<int> GetIdsOfValidSKUDuplicates(CombinationPart part) {
            var container = part.CombinationContainerPart;
            var siblings = GetIdsOfValidSKUDuplicates(container);
            siblings.Add(container.Id);
            return siblings;
        }
        #endregion
    }
}
