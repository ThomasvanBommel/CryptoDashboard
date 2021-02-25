using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace CryptoDashboard {

    // Panel with selectable amount, calculated price, and button button
    public class NumberButtonPanel : StackPanel {
        public TextBlock calculated_price = new TextBlock();
        public NumberBox amount = new NumberBox();
        public Button button = new Button();

        Currency currency;

        // Create a new NumberButtonPanel
        public NumberButtonPanel(String button_text, Currency currency) {
            // Initialize
            this.currency = currency;

            // Alignment
            calculated_price.HorizontalAlignment = HorizontalAlignment.Center;
            amount.HorizontalAlignment = button.HorizontalAlignment = HorizontalAlignment.Stretch;

            // Value
            calculated_price.Text = "0";
            amount.Value = 0;
            button.Content = button_text;

            // Value change event
            amount.ValueChanged += ValueChanged;

            // Numberbox defaults
            amount.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact;
            amount.Minimum = 0;
            amount.SmallChange = 0.1;
            amount.LargeChange = 1;

            // Add to this
            Children.Add(calculated_price);
            Children.Add(amount);
            Children.Add(button);
        }

        // Event listener for when the value changes
        private void ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) {
            calculated_price.Text = Formatter.FormatGroupedDecimalAndRound(currency.price * amount.Value);
        }

        // Calculate price * amount
        public double getAmountPrice() {
            return currency.price * amount.Value;
        }
    }
}
