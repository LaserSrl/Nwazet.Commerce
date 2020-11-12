using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Nwazet.Commerce.Models {
    public class OrderInformationDetail {
        public const string ElementName = "OrderInformationDetail";

        public OrderInformationDetail() { }

        public string Label { get; set; }
        
        public decimal Value { get; set; }
        
        public OrderInformationType InformationType { get; set; }
        public string ProcessorClass { get; set; }

        public XElement ToXML() {
            return new XElement(ElementName)
                .With(this)
                .ToAttr(old => old.Label)
                .ToAttr(old => old.Value)
                .ToAttr(old => old.InformationType)
                .ToAttr(old => old.ProcessorClass);
        }

        public static OrderInformationDetail FromXML(XElement el) {
            return el
                .With(new OrderInformationDetail())
                .FromAttr(e => e.Label)
                .FromAttr(e => e.Value)
                .FromAttr(e => e.InformationType)
                .FromAttr(e => e.ProcessorClass)
                .Context;
        }

    }

    public enum OrderInformationType {
        RawLinePrice,
        VAT,
        OriginalData
    }
}
