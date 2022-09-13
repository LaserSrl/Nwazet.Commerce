using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Models.Reporting;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Localization;

namespace Nwazet.Commerce.Reports {
    public class SalesByBestSellingProductsReport : ICommerceReport {
        private const int HowManyProductsAreDisplayed = 5;

        private readonly IContentManager _contentManager;

        public SalesByBestSellingProductsReport(IContentManager contentManager) {
            _contentManager = contentManager;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string Name {
            get { return T("Sales By Best Sellers").Text; }
        }

        public string Description {
            get { return T("Sales broken down by best selling products.").Text; }
        }

        public string DescriptionColumnHeader {
            get { return T("Period").Text; }
        }

        public string ValueColumnHeader {
            get { return T("Total").Text; }
        }

        public string ValueFormat {
            get { return "c"; }
        }

        public ChartType ChartType {
            get { return ChartType.Line; }
        }

        public ReportData GetData(DateTime startDate, DateTime endDate, TimePeriod granularity) {
            startDate = granularity.BeginningDate(startDate);
            endDate = granularity.EndingDate(endDate);
            var orders = _contentManager
                .Query<CommonPart, CommonPartRecord>("Order")
                .Where(r =>
                    r.CreatedUtc >= startDate.ToUniversalTime()
                    && r.CreatedUtc <= endDate.ToUniversalTime())
                .OrderBy(r => r.CreatedUtc)
                .Join<OrderPartRecord>()
                .Where(order => order.Status != OrderPart.Cancelled)
                .List()
                .ToList();
            var numberOfPoints = granularity.PeriodCount(startDate, endDate);
            var results = new List<ReportDataPoint>(numberOfPoints);
            var intervalStart = startDate;
            var intervalEnd = startDate + granularity;
            var seriesIds = orders
                .SelectMany(order => order
                    .As<OrderPart>()
                    .Items
                    .Select(item => new {
                        Id = item.ProductId,
                        Amount = (item.Quantity * item.Price) + item.LinePriceAdjustment,
                        ProductVersion = item.ProductVersion
                    }))
                .GroupBy(item => item.Id)
                .Select(group => new {
                    Id = group.Key,
                    Amount = group.Sum(item => item.Amount),
                    ProductVersion = group.Max(item => item.ProductVersion)
                })
                .OrderByDescending(sale => sale.Amount)
                .Take(HowManyProductsAreDisplayed);

            var seriesProductIds = seriesIds
                .Select(sale => sale.Id);

            var seriesProduct = seriesIds
                .ToDictionary(sale => sale.Id, sale => sale.ProductVersion);

            // TO DO: within the dictionary there must be the uniquekey and not the product id
            //var titleProducts = seriesProduct
            //    .Select(i => _contentManager.Get<TitlePart>(i.Key,
            //                (i.Key != 0 ? VersionOptions.Number(i.Value) :
            //                VersionOptions.Number(_contentManager.GetAllVersions(i.Key).Max(cv => cv.VersionRecord.Number)))))
            //    .ToDictionary(t => t.Id, t => t.Title);
            // Using titles from order items, avoiding queries on TitlePart and ensuring compatibility with Product Combinations (which have no TitlePart).
            // TODO: ensure that product collection has actually unique ids with the Distinct() function.
            // This because potentially title may change between orders.
            var titleProducts = orders
                .SelectMany(order => order
                    .As<OrderPart>()
                    .Items
                    .Select(item => new {
                        Id = item.ProductId,
                        Title = item.Title
                    }))
                    .Distinct()
                .ToDictionary(p => p.Id, p => p.Title);

            while (intervalStart < endDate) {
                var ordersForInterval = orders.Where(
                    common => common.CreatedUtc >= intervalStart
                              && common.CreatedUtc < intervalEnd)
                    .Select(common => common.As<OrderPart>())
                    .ToList();
                               
                results.Add(new ReportDataPoint {
                    Description = granularity.ToString(intervalStart, CultureInfo.CurrentUICulture),
                    Value = ordersForInterval.Any()
                        ? ordersForInterval.Sum(order => order.AmountPaid)
                        : 0.0M,
                    Series = ordersForInterval
                        .SelectMany(order => order
                            .Items
                            .Where(item => seriesProductIds.Contains(item.ProductId)))
                        .GroupBy(item => item.ProductId)
                        .Where(group => titleProducts.ContainsKey(group.Key))
                        .ToDictionary(
                            group => group.Key,
                            group => new ReportDataPoint {
                                Description = titleProducts[group.Key],
                                Value = group.Sum(item => (item.Quantity * item.Price) + item.LinePriceAdjustment)
                            })
                });
                intervalStart = intervalEnd;
                intervalEnd = intervalStart + granularity;
            }
            return new ReportData {
                DataPoints = results,
                Series = seriesProductIds
                    .Where(id => titleProducts.ContainsKey(id))
                    .Select(id => titleProducts[id]).ToList(),
                OrderBySeriesId= seriesProductIds
            };
        }
    }
}
