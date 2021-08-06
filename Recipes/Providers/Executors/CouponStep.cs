using Orchard.Environment.Extensions;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.Recipes.Providers.Executors {
    [OrchardFeature("Nwazet.Couponing")]
    public class CouponStep : RecipeExecutionStep {

        public CouponStep(
            RecipeExecutionLogger logger) : base(logger) { }

        public override string Name => "Coupons";

        public override void Execute(RecipeExecutionContext context) {
            
        }
    }
}
