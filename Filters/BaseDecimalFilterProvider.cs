using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Localization;
using Orchard.Projections.Descriptors.Filter;
using Orchard.Projections.FilterEditors.Forms;
using Orchard.Projections.Services;

namespace Nwazet.Commerce.Filters {
    public abstract class BaseDecimalFilterProvider<TRecord>
        : IFilterProvider where TRecord : ContentPartRecord {
        /// <summary>
        /// Category for the filter:
        /// When a filter is described we use
        /// describe.For(CATEGORY, name, description)
        /// </summary>
        protected abstract string FilterCategory { get; }
        /// <summary>
        /// Name for the filter:
        /// When a filter is described we use
        /// describe.For(category, NAME, description)
        /// </summary>
        protected abstract LocalizedString FilterName { get; }
        /// <summary>
        /// Description for the filter:
        /// When a filter is described we use
        /// describe.For(category, name, DESCRIPTION)
        /// </summary>
        protected abstract LocalizedString FilterDescription { get; }

        protected abstract string FilterElementType { get; }
        protected abstract LocalizedString FilterElementName { get; }
        protected abstract LocalizedString FilterElementDescription { get; }
        protected abstract string FormName { get; }
        /// <summary>
        /// THis is the property name used in the actual query predicate.
        /// </summary>
        protected abstract string PropertyName { get; }
        protected abstract string DisplayProperty { get; }

        public BaseDecimalFilterProvider() {

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public virtual void Describe(DescribeFilterContext describe) {
            describe.For(FilterCategory, FilterName, FilterDescription)
                .Element(FilterElementType, FilterElementName, FilterElementDescription,
                    ApplyFilter,
                    DisplayFilter,
                    FormName
                );
        }

        public virtual void ApplyFilter(FilterContext context) {
            decimal dvalue, dmin, dmax;
            dvalue = FromStateValue(context.State.Value);
            dmin = FromStateValue(context.State.Min);
            dmax = FromStateValue(context.State.Max);
            var op = (NumericOperator)Enum.Parse(
                typeof(NumericOperator), Convert.ToString(context.State.Operator));
            var filterExpression = FilterHelper
                .GetFilterPredicateNumeric(op, PropertyName, dvalue, dmin, dmax);
            var query = context.Query;
            context.Query = query
                .Where(x => x.ContentPartRecord<TRecord>(), filterExpression);
            return;
        }

        private decimal FromStateValue(dynamic sValue) {
            var str = Convert.ToString(sValue);
            decimal value;
            if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) {
                return value;
            }
            return 0m;
        }

        public virtual LocalizedString DisplayFilter(FilterContext context) {
            return FilterHelper.DisplayFilterNumeric(T, context.State, DisplayProperty);
        }
    }
}
