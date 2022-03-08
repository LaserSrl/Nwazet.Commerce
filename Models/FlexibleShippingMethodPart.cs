using Nwazet.Commerce.ApplicabilityCriteria;
using Nwazet.Commerce.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.FlexibleShippingImplementations")]
    public class FlexibleShippingMethodPart
        : ContentPart<FlexibleShippingMethodRecord>, IShippingMethod {
        public string Name {
            get { return Retrieve(r => r.Name); }
            set { Store(r => r.Name, value); }
        }

        public string ShippingCompany {
            get { return Retrieve(r => r.ShippingCompany); }
            set { Store(r => r.ShippingCompany, value); }
        }
        public string IncludedShippingAreas {
            get { return Retrieve(r => r.IncludedShippingAreas); }
            set { Store(r => r.IncludedShippingAreas, value); }
        }
        public string ExcludedShippingAreas {
            get { return Retrieve(r => r.ExcludedShippingAreas); }
            set { Store(r => r.ExcludedShippingAreas, value); }
        }
        /// <summary>
        /// In cases where VAT or similar taxes are applied on shipping, this is the
        /// price before tax.
        /// </summary>
        public decimal DefaultPrice {
            get { return Retrieve(r => r.DefaultPrice); }
            set { Store(r => r.DefaultPrice, value); }
        }

        public string PriceTiersTable {
            get { return Retrieve(r => r.PriceTiersTable); }
            set { Store(r => r.PriceTiersTable, value); }
        }

        public string TierTarget {
            get { return Retrieve(r => r.TierTarget); }
            set { Store(r => r.TierTarget, value); }
        }

        public IList<ApplicabilityCriterionRecord> ApplicabilityCriteria {
            get { return Record.ApplicabilityCriteria; }
            set { Record.ApplicabilityCriteria = value; }
        }

        public IEnumerable<ShippingOption> ComputePrice(
            IEnumerable<ShoppingCartQuantityProduct> productQuantities,
            IEnumerable<IShippingMethod> shippingMethods,
            string country,
            string zipCode,
            IWorkContextAccessor workContextAccessor) {

            var workContext = workContextAccessor.GetContext();
            IFlexibleShippingManager flexibleShippingManager;
            if (workContext != null
                && workContext.TryResolve(out flexibleShippingManager)) {
                // we have a usable IFlexibleShippingManager here
                var ac = new ApplicabilityContext(productQuantities, shippingMethods, country, zipCode);
                if (flexibleShippingManager.TestCriteria(Id, ac)) {
                    var price = DefaultPrice;
                    // Verify if Price Tiers are enabled via site settings.
                    var tiersSettings = workContext.CurrentSite.As<FlexibleShippingSiteSettingPart>();
                    if (tiersSettings != null && tiersSettings.EnablePriceTiers) {
                        price = ComputeTiers(ac, price);
                    }
                    var baseVatConfig = this.As<ProductVatConfigurationPart>();
                    IVatConfigurationService vatConfigurationService;
                    if (baseVatConfig != null
                        && workContext.TryResolve(out vatConfigurationService)) {
                        // a ProductVatConfigurationPart has been attached to this part that is
                        // used to configure shipping. This tells us 2 things:
                        // 1. The AdvancedVAT feature is active.
                        // 2. We want to configure VAT for this shipping.
                        // Using this part for this here is kind of an hack. We really would like to have
                        // a  more coherent system in place.
                        // TODO: fix this hack without breaking anything.
                        var vatConfig = baseVatConfig.VatConfigurationPart
                            ?? vatConfigurationService.GetDefaultCategory();
                        var rate = vatConfigurationService.GetRate(vatConfig);
                        price = price * (1.0m + rate);
                    }
                    yield return GetOption(price);
                }
            }

            yield break;
        }

        private decimal ComputeTiers(ApplicabilityContext applicabilityContext, decimal defaultPrice) {
            if (string.IsNullOrWhiteSpace(TierTarget) || string.IsNullOrWhiteSpace(PriceTiersTable)) {
                return defaultPrice;
            }
            // Loads the tiers table in a PriceTiersContext object.
            var tiers = new PriceTiersContext();
            tiers.FromCSV(PriceTiersTable);

            switch (TierTarget.ToLowerInvariant()) {
                case "weight":
                    // I need to search for the correct tier, ordering my list of tiers.
                    // First, I need to compute the total weight of the cart.
                    decimal totalWeight = applicabilityContext.ProductQuantities
                        .Sum(pc => pc.Product.Weight * pc.Quantity);
                    var correctWeightTier = tiers.Tiers
                        .Where(t => t.Valid && t.LowBound <= totalWeight)
                        .OrderByDescending(t => t.LowBound)
                        .FirstOrDefault();
                    if (correctWeightTier != null) {
                        decimal result;
                        if (ComputeFormula(correctWeightTier.Formula, applicabilityContext, out result)) {
                            return result;
                        }
                    }
                    break;

                case "quantity":
                    int totalQuantity = applicabilityContext.ProductQuantities
                        .Sum(pc => pc.Quantity);
                    var correctQuantityTier = tiers.Tiers
                        .Where(t => t.Valid && t.LowBound <= (decimal)totalQuantity)
                        .OrderByDescending(t => t.LowBound)
                        .FirstOrDefault();
                    if (correctQuantityTier != null) {
                        decimal result;
                        if (ComputeFormula(correctQuantityTier.Formula, applicabilityContext, out result)) {
                            return result;
                        }
                    }
                    break;

                case "cartamount":
                    decimal totalAmount = applicabilityContext.ProductQuantities
                        .Sum(pc => pc.Price * pc.Quantity);
                    var correctAmountTier = tiers.Tiers
                        .Where(t => t.Valid && t.LowBound <= totalAmount)
                        .OrderByDescending(t => t.LowBound)
                        .FirstOrDefault();
                    if (correctAmountTier != null) {
                        decimal result;
                        if (ComputeFormula(correctAmountTier.Formula, applicabilityContext, out result)) {
                            return result;
                        }
                    }
                    break;

                default:
                    break;
            }

            return defaultPrice;
        }

        private bool ComputeFormula(string formula, ApplicabilityContext applicabilityContext, out decimal result) {
            if (decimal.TryParse(formula, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) {
                return true;
            }

            // Substitute parameters like they were tokens.
            // {w} is total weight.
            if (formula.Contains("{w}")) {
                var w = applicabilityContext.ProductQuantities
                    .Sum(pc => pc.Product.Weight * pc.Quantity);
                formula = formula.Replace("{w}", w.ToString());
            }
            // {q} is the product quantity.
            if (formula.Contains("{q}")) {
                var q = applicabilityContext.ProductQuantities
                    .Sum(pc => pc.Quantity);
                formula = formula.Replace("{q}", q.ToString());
            }
            // {p} is the cart amount.
            if (formula.Contains("{p}")) {
                var p = applicabilityContext.ProductQuantities
                    .Sum(pc => pc.Price * pc.Quantity);
                formula = formula.Replace("{p}", p.ToString());
            }
            
            try {
                // Use mxparser Expression to compute the formula.
                Expression expr = new Expression(formula);
                var computed = expr.calculate();
                result = (decimal)computed;
                return true;
            } catch {

            }            

            return false;
        }

        private ShippingOption GetOption(decimal price) {
            return new ShippingOption {
                Description = Name,
                Price = price,
                ShippingCompany = ShippingCompany,
                IncludedShippingAreas =
                    IncludedShippingAreas == null
                        ? new string[] { }
                        : IncludedShippingAreas.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                ExcludedShippingAreas =
                    ExcludedShippingAreas == null
                        ? new string[] { }
                        : ExcludedShippingAreas.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries),
                ShippingMethodId = this.Id,
                DefaultPrice = DefaultPrice
            };
        }

        private ShippingOption GetOption() {
            return GetOption(DefaultPrice);
        }
    }
}
