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

namespace Nwazet.Commerce.Services {
    [OrchardFeature("Nwazet.AdvancedVAT")]
    public class VatOrderAdditionalInformationProvider : BaseOrderAdditionalInformationProvider {

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

        public override void StoreAdditionalInformation(OrderPart orderPart) {
            // We have to save, for each product in the order, what is the applied rate at the time the
            // order is created. 

            // We will need the destination TerritoryInternalRecord to figure out VAT
            var destination = FindDestination(orderPart.ShippingAddress, orderPart.BillingAddress);

            var data = _contentManager
                .GetMany<ProductPart>(orderPart.Items.Select(ci => ci.ProductId),
                    VersionOptions.Published, QueryHints.Empty)
                .Select(pp => {
                    return Tuple.Create<int, RateAndPrice>(
                        pp.ContentItem.Id,
                        new RateAndPrice {
                            Rate = _vatConfigurationService.GetRate(pp, destination),
                            PriceBeforeTax = pp.Price
                        });
                }).ToDictionary(tup => tup.Item1, tup => tup.Item2);

            // Now store
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
                Information = SerializeInformationDictionary(data)
            });
        }

        public override void Exporting(OrderPart part, ExportContentContext context) {
            base.Exporting(part, context);
        }

        public override void Importing(OrderPart part, ImportContentContext context) {
            base.Importing(part, context);
        }

        public override IEnumerable<OrderEditorAdditionalProductInfoViewModel> GetAdditionalOrderProductsInformation(OrderPart orderPart) {
            // Given the OrderPart, we should fetch from our repository the additional info
            var info = _vatOrderRepository
                .Fetch(ovr => ovr.OrderPartRecord == orderPart.Record)
                .FirstOrDefault();

            if (info == null) {
                yield break;
            }

            yield return new OrderEditorAdditionalProductInfoViewModel {
                Title = T("VAT Rate").Text,
                HeaderClass = "vat",
                Information = DeserializeInformation(info.Information)
                    .ToDictionary(did => did.Key, did => $"{(did.Value.Rate * 100m).ToString()} %"),
                InformationClass = "vat"
            };

            var currency = Currency.Currencies[orderPart.CurrencyCode];
            var cultureInUse = CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture);
            yield return new OrderEditorAdditionalProductInfoViewModel {
                Title = T("Price before Tax").Text,
                HeaderClass = "price",
                Information = DeserializeInformation(info.Information)
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
                    data.ContainsKey(checkoutItem.ProductId)
                        ? TaxDue(data[checkoutItem.ProductId]) * checkoutItem.Quantity
                        : 0m
                );
            var taxable = orderPart.Items
                .Sum(checkoutItem =>
                    data.ContainsKey(checkoutItem.ProductId)
                    ? data[checkoutItem.ProductId].PriceBeforeTax * checkoutItem.Quantity
                    : 0m);

            var currency = Currency.Currencies[orderPart.CurrencyCode];
            var cultureInUse = CultureInfo.GetCultureInfo(_workContextAccessor.GetContext().CurrentCulture);

            yield return _shapeFactory.VatAdditionalOrderProductsShape(
                TaxDue: currency.PriceAsString(vatDue, cultureInUse),
                TaxableAmount: currency.PriceAsString(taxable, cultureInUse)
                );
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

        private static string SerializeInformationDictionary(IDictionary<int, RateAndPrice> infoDictionary) {
            return JsonConvert.SerializeObject(infoDictionary, Formatting.None);
        }

        private static Dictionary<int, RateAndPrice> DeserializeInformation(string info) {
            return JsonConvert.DeserializeObject<Dictionary<int, RateAndPrice>>(info);
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
