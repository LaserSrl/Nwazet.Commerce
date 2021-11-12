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
    public class ProductPartDetailProvider : BaseCombinationDetailProvider {
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
    }
}
