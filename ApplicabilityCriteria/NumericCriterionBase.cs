using Nwazet.Commerce.Descriptors.ApplicabilityCriterion;
using Nwazet.Commerce.Services;
using Orchard.Environment.Extensions;
using Orchard.Projections.FilterEditors.Forms;
using System;
using System.Globalization;

namespace Nwazet.Commerce.ApplicabilityCriteria {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public abstract class NumericCriterionBase : IApplicabilityCriterionProvider {
        public abstract void Describe(DescribeCriterionContext describe);

        protected Func<decimal, bool> Test(CriterionContext context) {
            // We used NumericFilterForm for the form. That class does not provide us
            // with a method to do the comparison ourselves, so we have to do it here
            // explicitly, replicating some of the stuff that's there.
            var op = (NumericOperator)Enum.Parse(
                typeof(NumericOperator),
                Convert.ToString(context.State.Operator));
            decimal min, max;
            if (op == NumericOperator.Between || op == NumericOperator.NotBetween) {
                min = decimal.Parse(
                    Convert.ToString(context.State.Min),
                    CultureInfo.CurrentCulture);
                max = decimal.Parse(
                    Convert.ToString(context.State.Max),
                    CultureInfo.CurrentCulture);
            } else {
                min = max = decimal.Parse(
                    Convert.ToString(context.State.Value),
                    CultureInfo.CurrentCulture);
            }
            // get the comparison func
            return Test(op, min, max);
        }

        protected Func<decimal, bool> Test(NumericOperator op, decimal min, decimal max) {
            switch (op) {
                case NumericOperator.LessThan:
                    return x => x < max;
                case NumericOperator.LessThanEquals:
                    return x => x <= max;
                case NumericOperator.Equals:
                    if (min == max) {
                        return x => x == min;
                    }
                    return x => x >= min && x <= max;
                case NumericOperator.NotEquals:
                    if (min == max) {
                        return x => x != min;
                    }
                    return x => x < min || x > max;
                case NumericOperator.GreaterThan:
                    return x => x > min;
                case NumericOperator.GreaterThanEquals:
                    return x => x >= min;
                case NumericOperator.Between:
                    return x => x >= min && x <= max;
                case NumericOperator.NotBetween:
                    return x => x < min || x > max;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
