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

        #region Detail Information
        // Some of these will have to carry a value, e.g. a rate, or an amount.
        // Some instead will only carry some text information describing their
        // effect on the cart, e.g. "Enable free shipping".
        public decimal Value { get; set; }
        public string Description { get; set; }
        #endregion
        #region Classification
        public OrderInformationType InformationType { get; set; }
        /// <summary>
        /// use this to decide how to display the Value decimal, and ho to combine Values
        /// from different objects of this class.
        /// </summary>
        public OrderValueType ValueType { get; set; }
        public string ProcessorClass { get; set; }
        #endregion
        public XElement ToXML() {
            return new XElement(ElementName)
                .With(this)
                .ToAttr(old => old.Label)
                .ToAttr(old => old.Value)
                .ToAttr(old => old.Description)
                .ToAttr(old => old.InformationType)
                .ToAttr(old => old.ValueType)
                .ToAttr(old => old.ProcessorClass);
        }

        public static OrderInformationDetail FromXML(XElement el) {
            return el
                .With(new OrderInformationDetail())
                .FromAttr(e => e.Label)
                .FromAttr(e => e.Value)
                .FromAttr(e => e.Description)
                .FromAttr(e => e.InformationType)
                .FromAttr(e => e.ValueType)
                .FromAttr(e => e.ProcessorClass)
                .Context;
        }

    }

    public enum OrderInformationType {
        RawLinePrice,
        VAT,
        OriginalLineData,
        OriginalOrderData,
        TextInfo, // use this to display more details at backoffice, especially for order-level (rather than line-level) stuff
        FrontEndInfo // use this for stuff you may wish to display to the user
    }

    public enum OrderValueType {
        Number, // a quantity perhaps?
        Percent, // a rate: e.g. for VAT. Note that value in this case should be as a fraction of 1 (i.e. for 10% the value should be 0.10)
        Currency // an amount
    }
}
