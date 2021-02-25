using System;
using Windows.Globalization.NumberFormatting;

namespace CryptoDashboard {

    // Format class
    public class Formatter {
        static DecimalFormatter formatter = new DecimalFormatter();

        // Format double and round to 2 decimal places
        public static String FormatGroupedDecimalAndRound(double value) {
            formatter.IsGrouped = true;

            return formatter.Format(Math.Round(value * 100) / 100.0);
        }
    }
}
