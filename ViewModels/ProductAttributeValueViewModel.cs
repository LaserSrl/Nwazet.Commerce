using Nwazet.Commerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nwazet.Commerce.ViewModels {
    public class ProductAttributeValueViewModel {
        public ProductAttributeValueViewModel() {
            Deleted = false;
        }
        public ProductAttributeValueRecord AttributeValueRecord { get; set; }
        public bool Deleted { get; set; }
    }
}
