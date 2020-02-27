using System.Collections.Generic;
using Nwazet.Commerce.Models.Reporting;
using Nwazet.Commerce.Services;
using Orchard.Core.Common.ViewModels;

namespace Nwazet.Commerce.ViewModels {
    public class ReportDataViewModel {
        public IEnumerable<ReportDataPoint> DataPoints { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DescriptionColumnHeader { get; set; }
        public string ValueColumnHeader { get; set; }
        public string ValueFormat { get; set; }
        public ChartType ChartType { get; set; }
        public string Preset { get; set; }
        //public DateTimeEditor StartDateEditor { get; set; }
        public string StartDate { get; set; }
        public DateTimeEditor StartDateEditor {
            get {
                return new DateTimeEditor {
                    Date = StartDate,
                    Time = null,
                    ShowDate = true,
                    ShowTime = false
                };
            }
            set {
                StartDate = value.Date;
            }
        }
        //public DateTimeEditor EndDateEditor { get; set; }
        public string EndDate { get; set; }
        public DateTimeEditor EndDateEditor {
            get {
                return new DateTimeEditor {
                    Date = EndDate,
                    Time = null,
                    ShowDate = true,
                    ShowTime = false
                };
            }
            set {
                EndDate = value.Date;
            }
        }
        public TimePeriod Granularity { get; set; }
        public IEnumerable<string> Series { get; set; } 
        public ICurrencyProvider CurrencyProvider { get; set; }
    }
}
