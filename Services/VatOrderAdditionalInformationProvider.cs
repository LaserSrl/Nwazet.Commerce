using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.ViewModels;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.ContentManagement;
using Orchard.Data;
using Newtonsoft.Json;
using Orchard.Localization;
using System.Globalization;
using Orchard;
using Orchard.DisplayManagement;
using System.Xml.Linq;
using Nwazet.Commerce.Aspects;

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatOrderAdditionalInformationProvider : 
        BaseOrderAdditionalInformationProvider,
        IOrderFrontEndAdditionalInformationProvider {

        private readonly ITerritoriesRepositoryService _territoriesRepositoryService;
        private readonly IVatConfigurationService _vatConfigurationService;
        private readonly IContentManager _contentManager;
        private readonly IRepository<OrderVatRecord> _vatOrderRepository;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;

        public VatOrderAdditionalInformationProvider(
            ITerritoriesRepositoryService territoriesRepositoryService,
            IVatConfigurationService vatConfigurationService,
            IContentManager contentManager,
            IRepository<OrderVatRecord> vatOrderRepository,
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory) {

            _territoriesRepositoryService = territoriesRepositoryService;
            _vatConfigurationService = vatConfigurationService;
            _contentManager = contentManager;
            _vatOrderRepository = vatOrderRepository;
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<XElement> PrepareAdditionalInformation(OrderContext context) {
            // new method to inject additional information in the order
            
            // We will need the destination TerritoryInternalRecord to figure out VAT
            var destination = FindDestination(context.ShippingAddress, context.BillingAddress);

            var cart = context.ShoppingCart;
            var productLines = cart?.GetProducts();
            foreach (var productLine in productLines) {
                var productPart = productLine.Product;
                var vatConfigurationPart = productPart
                    ?.As<ProductVatConfigurationPart>()
                    ?.VatConfigurationPart
                    ?? _vatConfigurationService.GetDefaultCategory();
                var rate = _vatConfigurationService.GetRate(productPart, destination);

                var source = new XElement("VAT")
                    .Attr("TaxProductCategory", vatConfigurationPart.TaxProductCategory)
                    .Attr("Rate", rate);
                if (destination != null) {
                    source.AddEl(new XElement("DestinationTerritoryInternalRecord")
                        .With(destination)
                        .ToAttr(tir => tir.Id)
                        .ToAttr(tir => tir.Name)
                        .ToAttr(tir => tir.NameHash));
                }

                yield return new OrderLineInformation() {
                    ProductId = productLine.Product.Id,
                    Source = source,
                    Details = new OrderInformationDetail[] {
                        new OrderInformationDetail {
                            Label = "VAT",
                            Value = rate,
                            ValueType = OrderValueType.Percent,
                            InformationType = OrderInformationType.VAT,
                            ProcessorClass = this.GetType().FullName
                        }
                    }
                }.ToXML();
            }
        }

        public override void StoreAdditionalInformation(OrderPart orderPart) {
            // We have to save, for each product in the order, what is the applied rate at the time the
            // order is created. 

            // We will need the destination TerritoryInternalRecord to figure out VAT
            var destination = FindDestination(orderPart)
                .Where(d => d != null);

            // The VAT data we'll store as a Dictionary. The keys will uniquely relate to each CheckoutItem
            var vatData = new Dictionary<string, RateAndPrice>();
            // The Items in the OrderPart are af class CheckoutItem. They have the Id of the corresponding
            // ProductPart, but not the part itself or its ContentItem. We need the ContentItem to get
            // the VAT information for it.
            var productParts = _contentManager
                .GetMany<ProductPart>(
                    // more than one CheckoutItems may be for the same ProductPart, with different attributes
                    orderPart.Items.Select(ci => ci.ProductId).Distinct(),
                    VersionOptions.Published, QueryHints.Empty);
            foreach (var lineItem in orderPart.Items) {
                // ProductPart for this CheckoutItem
                var productPart = productParts.FirstOrDefault(pp => pp.Id == lineItem.ProductId);
                if (productPart != null) { // sanity check
                    // destination is ordered from most to least specific territory
                    var rate = _vatConfigurationService.GetRate(productPart, destination);
                    // productPart.Price is what has been input in the editor for the ContentItem. It may
                    // not be the taxable amount in case there are discounts of any form.
                    // We can recover the taxable amount by using checkoutItem.Price and the computed rate.
                    // CheckoutItem.Price should have already been "altered" by attributes that may change 
                    // the price of each single item.
                    var priceBeforeTax = lineItem.Price;
                    if (rate != 0m) {
                        // If the rate is 0, we don't really care about the taxable amount, and it's fine to
                        // consider it the same as the final price.
                        // If the rate is not 0, we discriminate depending on the setting for the default 
                        // destination.
                        if (_vatConfigurationService.GetDefaultDestination() != null) {
                            // in this configuration, the price on the checkoutItem is already inclusive
                            // of VAT. We may then find the taxable amount by subtracting the VAT.
                            priceBeforeTax = lineItem.Price / (1m + rate);
                        }
                        // If the default destination was null, then the checkoutItem.Price does not include
                        // VAT: it is already the taxable amount as it is.
                    }
                    // create a key to uniquely identify this CheckoutItem in the order
                    var itemKey = lineItem.AsUniqueKey();
                    var itemVAT = new RateAndPrice {
                        Rate = rate,
                        PriceBeforeTax = priceBeforeTax
                    };
                    vatData.Add(itemKey, itemVAT);
                }
            }

            // add Vat info related to shipping, if it's even there
            if (orderPart.ShippingOption != null) {
                var shippingVatPart = _contentManager.Get<ProductVatConfigurationPart>(orderPart.ShippingOption.ShippingMethodId);
                if (shippingVatPart != null) {
                    // shipping has a VAT configured
                    var vatConfig = shippingVatPart.VatConfigurationPart
                            ?? _vatConfigurationService.GetDefaultCategory();
                    var shippingKey = orderPart.ShippingOption.ShippingMethodId.ToString();
                    var shippingVat = new RateAndPrice {
                        Rate = _vatConfigurationService.GetRate(vatConfig),
                        PriceBeforeTax = orderPart.ShippingOption.DefaultPrice
                    };

                    vatData.Add(shippingKey, shippingVat);
                }
            }

            // store information related to the destination. This way, even if the addresses for the order
            // are changed later, we can still mark the destination used for computing the VAT information.
            var specificityCounter = 0; // use this to mark specificity when storing territories
            foreach (var tir in destination) {
                // we are hacking what we currently store: 
                // the ids for destinations are positive and may clash with products and such
                // However, we are allowed to use negative keys for our data. By using (-Id) as
                // key, we identify entries in the dictionary that represent territories.
                // We use the value object to store an information that will allow us to sort
                // the territories back in the correct specificity order (from most to least specific)
                var territoryKey = (-tir.Id).ToString();
                var territoryInfo = new RateAndPrice { Rate = specificityCounter++ };

                vatData.Add(territoryKey, territoryInfo);
            }

            // Now store
            StoreInfo(orderPart, SerializeInformationDictionary(vatData));
        }

        public override void Exporting(OrderPart part, ExportContentContext context) {
            var info = _vatOrderRepository
                .Fetch(ovr => ovr.OrderPartRecord == part.Record)
                .FirstOrDefault();
            if (info == null) {
                return;
            }

            context.Element(part.PartDefinition.Name)
                .AddEl(new XElement("VatOrderAdditionalInformation").With(info)
                    .ToAttr(i => i.Information)
                );
            
        }

        public override void Importing(OrderPart part, ImportContentContext context) {
            var element = context.Data.Element(part.PartDefinition.Name);
            if (element == null) {
                return;
            }

            var infoEl = element.Element("VatOrderAdditionalInformation");
            if (infoEl == null) {
                return;
            }

            var info = infoEl.Attribute("Information").Value;
            if (!string.IsNullOrWhiteSpace(info)) {
                StoreInfo(part, info);
            }
        }

        public override IEnumerable<OrderEditorAdditionalProductInfoViewModel> GetAdditionalOrderProductsInformation(OrderPart orderPart) {
            // Given the OrderPart, we should fetch from our repository the additional info
            var info = _vatOrderRepository
                .Fetch(ovr => ovr.OrderPartRecord == orderPart.Record)
                .FirstOrDefault();

            if (info == null) {
                yield break;
            }

            var data = DeserializeInformation(info.Information);
            // In an order, there may be more than one item for the same Product ContentItem when
            // attributes are involved. That means the key for the Information Dictionary has to be
            // more complex.
            yield return new OrderEditorAdditionalProductInfoViewModel {
                Title = T("VAT Rate").Text,
                HeaderClass = "vat",
                Information = data
                    .ToDictionary(did => did.Key, did => $"{(did.Value.Rate * 100m).ToString()} %"),
                InformationClass = "vat"
            };

            var currency = Currency.Currencies[orderPart.CurrencyCode];
            var cultureInUse = CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture);
            yield return new OrderEditorAdditionalProductInfoViewModel {
                Title = T("Taxable").Text,
                HeaderClass = "price",
                Information = data
                    .ToDictionary(
                        did => did.Key, 
                        did => currency.PriceAsString(did.Value.PriceBeforeTax, cultureInUse)),
                InformationClass = "price"
            };
        }

        public override IEnumerable<dynamic> GetAdditionalOrderProductsShapes(OrderPart orderPart) {
            // we return a shape that will tell the total VAT due for the order.
            var info = _vatOrderRepository
                .Fetch(ovr => ovr.OrderPartRecord == orderPart.Record)
                .FirstOrDefault();

            if (info == null) {
                yield break;
            }

            var data = DeserializeInformation(info.Information);
            var vatDue = orderPart.Items
                .Sum(checkoutItem => 
                    data.ContainsKey(checkoutItem.AsUniqueKey())
                        ? TaxDue(data[checkoutItem.AsUniqueKey()]) * checkoutItem.Quantity
                        : 0m
                );
            var taxable = orderPart.Items
                .Sum(checkoutItem =>
                    data.ContainsKey(checkoutItem.AsUniqueKey())
                    ? data[checkoutItem.AsUniqueKey()].PriceBeforeTax * checkoutItem.Quantity
                    : 0m);

            //add shipping
            var shippingTax = 0.0m;
            var shippingTaxable = 0.0m;
            if (orderPart.ShippingOption != null) {
                if (data.ContainsKey(orderPart.ShippingOption.ShippingMethodId.ToString())) {
                    var rateAndPrice = data[orderPart.ShippingOption.ShippingMethodId.ToString()];
                    shippingTax += TaxDue(rateAndPrice);
                    shippingTaxable += rateAndPrice.PriceBeforeTax;
                }
            }

            var currency = Currency.Currencies[orderPart.CurrencyCode];
            var cultureInUse = CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture);

            yield return _shapeFactory.VatAdditionalOrderProductsShape(
                TaxDue: currency.PriceAsString(vatDue, cultureInUse),
                TaxableAmount: currency.PriceAsString(taxable, cultureInUse),
                // provide all details to rebuild the strings.
                Currency: currency,
                CultureInUse: cultureInUse,
                TaxValue: vatDue,
                TaxableValue: taxable,
                ShippingTaxValue: shippingTax,
                ShippingTaxableValue: shippingTaxable
                );
        }

        public override IEnumerable<dynamic> GetAdditionalOrderAddressesShapes(OrderPart orderPart) {
            // We will need the destination TerritoryInternalRecord to figure out VAT
            var destination = FindDestination(orderPart);

            if (destination == null || !destination.Any()) {
                yield break;
            }
            var name = string.Join(" - ", destination.Select(d => d.Name));
            yield return _shapeFactory.VatAdditionalOrderAddressesShapes(
                Destination: name
                );
        }

        #region Compute destination territory
        private IEnumerable<TerritoryInternalRecord> FindDestination(OrderPart order) {
            // See whether we stored the destination in the additional records for the 
            // order
            var results = DestinationFromRecord(order);
            if (results != null && results.Any()) {
                return results;
            }
            // use the new IAspect to figure this information out and 
            // only eventually fallback to the "old" logic based on the information
            // from OrderPart
            results = DestinationFromAspect(order);
            if (results != null && results.Any()) {
                return results;
            }
            // Fallback: try to parse the information in the address in case we were
            // unable to find territories before this.
            return new TerritoryInternalRecord[] {
                FindDestination(order.ShippingAddress, order.BillingAddress)
            };
        }

        private IEnumerable<TerritoryInternalRecord> DestinationFromRecord(OrderPart order) {
            // See whether we stored the destination in the additional records for the 
            // order
            var info = _vatOrderRepository
                .Fetch(ovr => ovr.OrderPartRecord == order.Record)
                .FirstOrDefault();
            if (info != null) {
                var data = DeserializeInformation(info.Information);
                // only get the subset of data where the key can be parsed to an int
                int tmp;
                var destinationData = new Dictionary<int, RateAndPrice>();
                foreach (var kvp in data) {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    // we use negative keys to store the ids of territories, so we don't
                    // mistake them for products.
                    if (int.TryParse(key, out tmp) && tmp < 0) {
                        destinationData.Add(tmp, value);
                    }
                }
                if (destinationData.Any()) {
                    // We use the Rate we stored for those keys to sort them. 
                    // Lower Rate means higher specificity.
                    return destinationData
                        // sort ascending by rate
                        .OrderBy(kvp => kvp.Value.Rate)
                        .Select(kvp => _territoriesRepositoryService
                            .GetTerritoryInternal(-kvp.Key));
                }
            }
            return null;
        }

        private IEnumerable<TerritoryInternalRecord> DestinationFromAspect(OrderPart order) {
            var addressAspect = order.As<ITerritoryAddressAspect>();
            if (addressAspect != null) {
                var foundTerritory = false;
                var tir = _territoriesRepositoryService
                    .GetTerritoryInternal(addressAspect.CityId);
                if (tir != null) {
                    foundTerritory = true;
                    yield return tir;
                }
                tir = _territoriesRepositoryService
                    .GetTerritoryInternal(addressAspect.ProvinceId);
                if (tir != null) {
                    foundTerritory = true;
                    yield return tir;
                }
                tir = _territoriesRepositoryService
                    .GetTerritoryInternal(addressAspect.CountryId);
                if (tir != null) {
                    foundTerritory = true;
                    yield return tir;
                }
                // If we could not find the specific information, fallback to trying to
                // return everything
                if (!foundTerritory) {
                    foreach (var id in addressAspect.TerritoriesIds) {
                        tir = _territoriesRepositoryService.GetTerritoryInternal(id);
                        if (tir != null) {
                            foundTerritory = true;
                            yield return tir;
                        }
                    }
                }
            }
        }

        private TerritoryInternalRecord FindDestination(Address shippingAddress, Address billingAddress) {
            // prioritize the shipping address if it's there
            var destination = FindDestination(shippingAddress);
            // try with the billing address if it's there
            if (destination == null) {
                destination = FindDestination(billingAddress);
            }

            return destination;
        }

        private TerritoryInternalRecord FindDestination(Address address) {
            if (address == null) {
                return null;
            }
            if (string.IsNullOrWhiteSpace(address.Country)
                    && string.IsNullOrWhiteSpace(address.PostalCode)) {
                return null;
            }
            // postal code is more specific
            var destination = !string.IsNullOrWhiteSpace(address.PostalCode)
                ? _territoriesRepositoryService.GetTerritoryInternal(address.PostalCode)
                : null;
            if (destination == null) {
                destination = !string.IsNullOrWhiteSpace(address.Country)
                    ? _territoriesRepositoryService.GetTerritoryInternal(address.Country)
                    : null;
            }
            return destination;
        }
        #endregion

        private static string SerializeInformationDictionary(IDictionary<string, RateAndPrice> infoDictionary) {
            return JsonConvert.SerializeObject(infoDictionary, Formatting.None);
        }
        
        private static Dictionary<string, RateAndPrice> DeserializeInformation(string info) {
            return JsonConvert.DeserializeObject<Dictionary<string, RateAndPrice>>(info);
        }

        private void StoreInfo(OrderPart orderPart, string info) {
            var existing = _vatOrderRepository
                .Fetch(ovr => ovr.OrderPartRecord == orderPart.Record);
            if (existing != null && existing.Any()) {
                // the fact that something already exists is an error that should never happen
                // we handle it by deleting those old records
                foreach (var entity in existing) {
                    _vatOrderRepository.Delete(entity);
                }
            }

            _vatOrderRepository.Create(new OrderVatRecord {
                OrderPartRecord = orderPart.Record,
                Information = info
            });
        }

        private static decimal TaxDue(RateAndPrice rap) {
            return rap.PriceBeforeTax * rap.Rate;
        }

        struct RateAndPrice {
            public decimal Rate { get; set; }
            public decimal PriceBeforeTax { get; set; }
        }
    }
}
