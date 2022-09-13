using System;
using System.Collections.Generic;
using System.Linq;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Reporting;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Reports {
    public class MostProfitableProductsReport : ICommerceReport {
        private readonly IContentManager _contentManager;

        public MostProfitableProductsReport(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string Name {
            get { return T("Most profitable products").Text; }
        }

        public string Description {
            get { return T("Products that made the most money.").Text; }
        }

        public string DescriptionColumnHeader {
            get { return T("Product").Text; }
        }

        public string ValueColumnHeader {
            get { return T("Revenue").Text; }
        }

        public string ValueFormat {
            get { return "c"; }
        }

        public ChartType ChartType {
            get { return ChartType.Doughnut; }
        }

        public ReportData GetData(DateTime startDate, DateTime endDate, TimePeriod granularity) {
            var orders = _contentManager
                .Query<CommonPart, CommonPartRecord>("Order")
                .Where(r =>
                    r.CreatedUtc >= startDate.ToUniversalTime()
                    && r.CreatedUtc <= endDate.ToUniversalTime())
                .Join<OrderPartRecord>()
                .Where(order => order.Status != OrderPart.Cancelled)
                .List()
                .ToList();
            var totalRevenues = new Dictionary<int, decimal>();
            var products = new Dictionary<int, string>();
            var productsVersion = new Dictionary<int, int>();
            foreach (var order in orders) {
                var checkoutItems = order.As<OrderPart>().Items;

                // TO DO: within the dictionary there must be the uniquekey and not the product id
                //var titleProduct = checkoutItems
                //    .Select(i => _contentManager.Get<TitlePart>(i.ProductId, 
                //                (i.ProductVersion != 0 ? VersionOptions.Number(i.ProductVersion) :
                //                VersionOptions.Number(_contentManager.GetAllVersions(i.ProductId).Max(cv => cv.VersionRecord.Number)))))
                //    .ToDictionary(t => t.Id,t=>t.Title);
                // Using titles from order items, avoiding queries on TitlePart and ensuring compatibility with Product Combinations (which have no TitlePart).
                var titleProduct = checkoutItems.ToDictionary(ci => ci.ProductId, ci => ci.Title);

                foreach (var checkoutItem in checkoutItems) {
                    var productId = checkoutItem.ProductId;
                    var totalPrice = (checkoutItem.Quantity*checkoutItem.Price) + checkoutItem.LinePriceAdjustment;
                    if (totalRevenues.ContainsKey(productId)) {
                        totalRevenues[productId] += totalPrice;
                        // check if the version of the product I am adding is correct
                        // is greater than or equal to the version already saved and replace the title
                        var versionSaved = productsVersion.FirstOrDefault(t => t.Key == productId).Value;
                        if (checkoutItem.ProductVersion != 0 && versionSaved != 0 && checkoutItem.ProductVersion > versionSaved) {
                            products[productId] = titleProduct.FirstOrDefault(t => t.Key == productId).Value;
                            productsVersion[productId] = checkoutItem.ProductVersion;
                        }
                    }
                    else {
                        totalRevenues.Add(productId, totalPrice);
                        // add the title to the product list
                        products.Add(productId, titleProduct.FirstOrDefault(t => t.Key == productId).Value);
                        // save the version in another list 
                        productsVersion.Add(productId, checkoutItem.ProductVersion);
                    }
                }
            }
            return new ReportData {
                DataPoints = totalRevenues
                    .Select(q => new ReportDataPoint {
                        Description = products.ContainsKey(q.Key) ? products[q.Key] : q.Key.ToString(),
                        Value = q.Value
                    })
                    .OrderByDescending(q => q.Value)
            };
        }
    }
}
