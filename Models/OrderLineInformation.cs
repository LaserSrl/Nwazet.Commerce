using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nwazet.Commerce.Models {
    public class OrderLineInformation {
        public const string ElementName = "OrderLineInformation";

        public OrderLineInformation() {
            Details = Enumerable.Empty<OrderInformationDetail>();
        }

        public int ProductId { get; set; }

        public IEnumerable<OrderInformationDetail> Details { get; set; }

        public XElement Source { get; set; }
        
        public XElement ToXML() {
            return new XElement(ElementName)
                .Attr("ProductId", ProductId)
                .AddEl(new XElement("Source", Source))
                .AddEl(Details
                    .Select(d => d.ToXML())
                    .ToArray());
        }

        public static OrderLineInformation FromXML(XElement el) {
            var ola = el
                .With(new OrderLineInformation())
                .FromAttr(e => e.ProductId)
                .Context;
            ola.Source = el.Element("Source");
            ola.Details = el.Elements(OrderInformationDetail.ElementName)
                .Select(subel => OrderInformationDetail.FromXML(subel));
            return ola;
        }
    }
}
