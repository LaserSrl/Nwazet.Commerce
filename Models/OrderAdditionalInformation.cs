using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nwazet.Commerce.Models {
    /// <summary>
    /// Used to specify details for the whole order
    /// </summary>
    public class OrderAdditionalInformation {
        public const string ElementName = "OrderAdditionalInformation";

        public OrderAdditionalInformation() {
            Details = Enumerable.Empty<OrderInformationDetail>();
        }

        public IEnumerable<OrderInformationDetail> Details { get; set; }

        public XElement Source { get; set; }

        public XElement ToXML() {
            return new XElement(ElementName)
                .AddEl(new XElement("Source", Source))
                .AddEl(Details
                    .Select(d => d.ToXML())
                    .ToArray());
        }

        public static OrderAdditionalInformation FromXML(XElement el) {
            var ola = el
                .With(new OrderAdditionalInformation())
                .Context;
            ola.Source = el.Element("Source");
            ola.Details = el.Elements(OrderInformationDetail.ElementName)
                .Select(subel => OrderInformationDetail.FromXML(subel));
            return ola;
        }
    }
}
