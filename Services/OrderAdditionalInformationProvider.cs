using Nwazet.Commerce.Models;
using Orchard.Environment.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.Orders")]
    public class OrderAdditionalInformationProvider : BaseOrderAdditionalInformationProvider {

        public override IEnumerable<XElement> PrepareAdditionalInformation(OrderContext context) {
            // for each product in the order,
            // add their original prices (before VAT and everything else)
            var cart = context.ShoppingCart;
            var productLines = cart?.GetProducts();
            if (productLines != null) {
                foreach (var productLine in productLines) {
                    var productPart = productLine.Product;

                    yield return new OrderLineInformation() {
                        ProductId = productLine.Product.Id,
                        Details = new OrderInformationDetail[] {
                            new OrderInformationDetail {
                                Label = "OriginalPrice",
                                Value = productPart.Price,
                                InformationType = OrderInformationType.OriginalData,
                                ProcessorClass = this.GetType().FullName
                            },
                            new OrderInformationDetail {
                                Label = "DiscountPrice",
                                Value = productPart.DiscountPrice,
                                InformationType = OrderInformationType.OriginalData,
                                ProcessorClass = this.GetType().FullName
                            }
                        }
                    }.ToXML();
                }
            }
        }
    }
}
