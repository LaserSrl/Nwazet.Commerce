using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nwazet.Commerce.Models {
    public class PriceTiersContext {
        public List<PriceTiersContextRow> Tiers { get; set; }

        public void FromCSV(string csvTable) {
            Tiers = new List<PriceTiersContextRow>();
            var tiers = csvTable.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var tier in tiers) {
                var row = new PriceTiersContextRow();
                row.FromCSV(tier);
                if (row.Valid) {
                    Tiers.Add(row);
                }
            }
        }
    }

    public class PriceTiersContextRow {
        public decimal LowBound { get; set; }
        public string Formula { get; set; }
        public bool Valid { get; set; }

        public void FromCSV(string csvLine) {
            var cols = csvLine.Split(',');
            if (cols.Length == 2) {
                decimal bound;
                if (!decimal.TryParse(cols[0], NumberStyles.Any, CultureInfo.InvariantCulture, out bound) || string.IsNullOrWhiteSpace(cols[1])) {
                    Valid = false;
                } else {
                    LowBound = bound;
                    Formula = cols[1];
                    Valid = true;
                }
            } else {
                Valid = false;
            }
        }
    }
}
