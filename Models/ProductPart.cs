using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.Collections.Generic;
using Nwazet.Commerce.Services;

namespace Nwazet.Commerce.Models {
    [OrchardFeature("Nwazet.Commerce")]
    public class ProductPart : ContentPart<ProductPartVersionRecord>, IProduct {
        [Required]
        public string Sku
        {
            get { return Retrieve(r => r.Sku); }
            set { Store(r => r.Sku, value); }
        }

        [Required]
        public decimal Price {
            get { return Retrieve(r => r.Price); }
            set { Store(r => r.Price, value); }
        }

        public decimal DiscountPrice {
            get {return Retrieve(r => r.DiscountPrice, -1);}
            set { Store(r => r.DiscountPrice, value); }
        }

        public decimal? ShippingCost
        {
            get { return Retrieve(r => r.ShippingCost); }
            set { Store(r => r.ShippingCost, value); }
        }

        public double Weight
        {
            get { return Retrieve(r => r.Weight); }
            set { Store(r => r.Weight, value); }
        }

        public string Size
        {
            get { return Retrieve(r => r.Size); }
            set { Store(r => r.Size, value); }
        }

        public bool OverrideTieredPricing
        {
            get { return Retrieve(r => r.OverrideTieredPricing); }
            set { Store(r => r.OverrideTieredPricing, value); }
        }

        public IEnumerable<PriceTier> PriceTiers
        {
            get
            {
                var rawTiers = Retrieve<string>("PriceTiers");
                return PriceTier.DeserializePriceTiers(rawTiers);
            }
            set
            {
                var serializedTiers = PriceTier.SerializePriceTiers(value);
                Store("PriceTiers", serializedTiers ?? "");
            }
        }

        public bool AuthenticationRequired
        {
            get { return Retrieve(r => r.AuthenticationRequired); }
            set { Store(r => r.AuthenticationRequired, value); }
        }

        public bool IsDigital {
            get { return Retrieve(r => r.IsDigital); }
            set { Store(r => r.IsDigital, value); }
        }

        /// <summary>
        /// Shortcut property to get the total inventory for the product
        /// </summary>
        public int Inventory {
            get {
                return ProductInventoryService?.GetInventory(this) ?? 0;
            }
        }

        public bool ConsiderInventory {
            get {
                return this.Has<InventoryPart>();
            }
        }

        public string OutOfStockMessage {
            get {
                return this.As<InventoryPart>()?.OutOfStockMessage ?? string.Empty;
            }
        }

        public bool AllowBackOrder {
            get {
                return this.As<InventoryPart>()?.AllowBackOrder ?? false;
            }
        }

        public int MinimumOrderQuantity {
            get {
                var minimumOrderQuantity = this.As<InventoryPart>()?.MinimumOrderQuantity;
                return minimumOrderQuantity > 1 ? minimumOrderQuantity.Value : 1;
            }
        }

        /// <summary>
        /// Back reference to service used for common operations.
        /// This is set when the ProductPart is Activated.
        /// </summary>
        public IProductService ProductService { get; set; }
        /// <summary>
        /// Back reference to service used for inventory operations.
        /// This is set when the ProductPart is Activated.
        /// </summary>
        public IProductInventoryService ProductInventoryService { get; set; }
        /// <summary>
        /// Back reference to service used for price operations.
        /// This is set when the ProductPart is Activated.
        /// </summary>
        public IProductPriceService ProductPriceService { get; set; }
    }
}
