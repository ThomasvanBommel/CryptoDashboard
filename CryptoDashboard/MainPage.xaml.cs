using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;
using Windows.UI.Popups;
using System.Diagnostics;
using Newtonsoft.Json;
using Windows.UI;
using Windows.UI.Text;
using Windows.Globalization.NumberFormatting;
using Microsoft.UI.Xaml.Controls;

namespace CryptoDashboard {
    public sealed partial class MainPage : Page {
        //public event EventHandler CanExecuteChanged;
        string APIKey;
        string currencyType = "CAD";
        int dashboardPage = 1;
        Dictionary<string, PurchasedCoin> myCurrencies = new Dictionary<string, PurchasedCoin>();

        public MainPage() {
            this.InitializeComponent();
        }

        // Clicked the hamburger icon!
        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        // Unlock the rest of the application when the user submits a valid API key
        private void UnlockContent_Click(object sender, RoutedEventArgs e) {
            // Disable button
            UnlockButton.IsEnabled = false;

            // Get key from UI
            string key = APIKeyInput.Text;

            // Check if key is empty
            if (key != "") {
                // Send request
                RequestNewDashboard(key);
            } else {
                // User submitted empty key
                new MessageDialog("Empty submission. Try again.").ShowAsync();

                // Enable button
                UnlockButton.IsEnabled = true;
            }
        }

        // Request a new / updated dashboard
        private async void RequestNewDashboard(string key, int page = 1, int per_page = 20) {
            // Create an HTTP client object
            HttpClient client = new HttpClient();

            // Nomic endpoint
            Uri uri = new Uri(
                "https://api.nomics.com/v1/currencies/ticker?status=active&sort=rank&key=" + key +
                "&convert=" + currencyType +
                "&page=" + page +
                "&per-page=" + per_page
            );

            // Object that will receive data asynchronously
            HttpResponseMessage response;
            string json;
            List<Currency> list = new List<Currency>();

            try {
                // Get response
                response = await client.GetAsync(uri);

                // Ensure success
                response.EnsureSuccessStatusCode();

                // Read data
                json = await response.Content.ReadAsStringAsync();

                // Set list
                list = deserialize(json);

                // Unlock the application
                UnlockApplication();
            } catch (Exception e) {
                // Inform user they've input an invalid key
                new MessageDialog("Error:\n" + e.Message).ShowAsync();
                return;
            }

            // Deserialize json and update the dashboard
            UpdateDashboard(list);

            // Set API key for use later
            APIKey = key;

            // Updated dashboard page number
            dashboardPage = page;

            // Enable unlock button
            UnlockButton.IsEnabled = true;

            // Enable the refresh button
            RefreshBtn.IsEnabled = true;
        }

        // Deserialize JSON to Currency[] object
        private List<Currency> deserialize(string json) {
            return JsonConvert.DeserializeObject<List<Currency>>(json);
        }

        // Unlock the application, hiding the lock screen and showing the dashboard
        private void UnlockApplication() {
            // Hide the lock screen
            UnlockPage.Visibility = Visibility.Collapsed;
            LockedMenu.Visibility = Visibility.Collapsed;

            // Make the dashboard visible
            Dashboard.Visibility = Visibility.Visible;
            DashboardMenu.Visibility = Visibility.Visible;

            // Show "logout" button
            LockBtn.Visibility = Visibility.Visible;
            RefreshBtn.Visibility = Visibility.Visible;

            // Show currency info (topbar)
            CashInfo.Visibility = Visibility.Visible;
        }

        // Lock the application, hiding the dashboard and showing the lock screen
        private void LockApplication() {
            // Hide dashboard
            Dashboard.Visibility = Visibility.Collapsed;
            DashboardMenu.Visibility = Visibility.Collapsed;

            // Show lock screen
            APIKeyInput.Text = "";
            UnlockPage.Visibility = Visibility.Visible;
            LockedMenu.Visibility = Visibility.Visible;

            // Hide "logout" button
            LockBtn.Visibility = Visibility.Collapsed;
            RefreshBtn.Visibility = Visibility.Collapsed;

            // Hide currency info (topbar)
            CashInfo.Visibility = Visibility.Collapsed;
        }

        // Update dashboard currencies
        private void UpdateDashboard(List<Currency> currencies) {
            // Clear the dashboard
            DashboardGrid.Children.Clear();
            DashboardGrid.RowDefinitions.Clear();

            // Keep track of which row we're on
            int count = 0;

            // For each currency
            foreach (Currency currency in currencies) {
                // Add row to the grid
                DashboardGrid.RowDefinitions.Add(new RowDefinition());

                // Col 0-4 offset
                Thickness offset = new Thickness(0, 20, 0, 0);

                // Create main element
                RelativePanel main = CreateElement(currency);
                main.Margin = new Thickness(10, 20, 0, 0);
                Grid.SetColumn(main, 0);
                Grid.SetRow(main, count);

                // Create 1d element
                RelativePanel _1 = CreateElement1d(currency._1d.price_change);
                _1.Margin = offset;
                Grid.SetColumn(_1, 1);
                Grid.SetRow(_1, count);

                // Create 30d element
                RelativePanel _30 = CreateElement1d(currency._30d.price_change);
                _30.Margin = offset;
                Grid.SetColumn(_30, 2);
                Grid.SetRow(_30, count);

                // Create 365d element
                RelativePanel _365 = CreateElement1d(currency._365d.price_change);
                _365.Margin = offset;
                Grid.SetColumn(_365, 3);
                Grid.SetRow(_365, count);

                // Create YTD element
                RelativePanel _ytd = CreateElement1d(currency.ytd.price_change);
                _ytd.Margin = offset;
                Grid.SetColumn(_ytd, 4);
                Grid.SetRow(_ytd, count);

                // BuyAmountButtonPanelThingy
                NumberButtonPanel buyPanel = BuyAmountButton(currency);
                Grid.SetColumn(buyPanel, 5);
                Grid.SetRow(buyPanel, count);

                // Increment count
                count++;

                // Add content to the grid
                DashboardGrid.Children.Add(main);
                DashboardGrid.Children.Add(_1);
                DashboardGrid.Children.Add(_30);
                DashboardGrid.Children.Add(_365);
                DashboardGrid.Children.Add(_ytd);
                DashboardGrid.Children.Add(buyPanel);
            }
        }

        // Create a new buy amount button for a certain currency
        private NumberButtonPanel BuyAmountButton(Currency currency) {
            // NumberButtonPanel
            NumberButtonPanel buyPanel = new NumberButtonPanel("Buy " + currency.symbol, currency);
            buyPanel.Margin = new Thickness(10);

            // User clicks buy button
            buyPanel.button.Click += delegate (object sender, RoutedEventArgs args) {
                if (buyPanel.amount.Value <= 0) return;

                // Check if user has enough money
                if (ChangeCash(-buyPanel.getAmountPrice())) {

                    // Check if myCurrencies contains this coin at this price
                    if (myCurrencies.ContainsKey(currency.symbol)) {

                        // If so add to it
                        myCurrencies[currency.symbol + currency.price].amount += buyPanel.amount.Value;
                    } else {

                        // If not add a new entry
                        myCurrencies[currency.symbol + currency.price] = new PurchasedCoin(currency, buyPanel.amount.Value);
                    }

                    // Update currency page
                    UpdateMyCurrencies();
                }
            };

            return buyPanel;
        }

        // Create main element for currency
        private RelativePanel CreateElement(Currency currency) {
            RelativePanel panel = new RelativePanel();
            panel.Margin = new Thickness(10);

            // Logo
            Image logo = ElementMaker.MakeImage(currency.logo_url);
            logo.Width = 64;
            logo.Height = 64;
            logo.Margin = new Thickness(0, 0, 10, 0);

            // Name / title
            TextBlock name = new TextBlock();
            name.Text = currency.name + " (" + currency.symbol + ")";
            RelativePanel.SetRightOf(name, logo);

            // Rank
            TextBlock rank = new TextBlock();
            rank.Text = "Rank #" + currency.rank;
            RelativePanel.SetBelow(rank, name);
            RelativePanel.SetRightOf(rank, logo);

            // Rank
            TextBlock price = new TextBlock();
            price.Text = "$" + currency.price;
            RelativePanel.SetBelow(price, rank);
            RelativePanel.SetRightOf(price, logo);

            // Add elements to the panel
            panel.Children.Add(logo);
            panel.Children.Add(name);
            panel.Children.Add(rank);
            panel.Children.Add(price);

            // Return element
            return panel;
        }

        // Create colored element from currency change
        private RelativePanel CreateElement1d(string change) {
            RelativePanel panel = new RelativePanel();
            panel.Margin = new Thickness(10);
            panel.HorizontalAlignment = HorizontalAlignment.Center;

            // Change / movement
            TextBlock txt = new TextBlock();
            txt.Padding = new Thickness(0, 15, 0, 0);
            txt.Text = change;
            txt.FontWeight = FontWeights.Bold;
            txt.FontSize = 18;

            // Change text color +green / -red
            if (double.Parse(change) > 0) {
                txt.Foreground = new SolidColorBrush(Colors.Green);
            } else {
                txt.Foreground = new SolidColorBrush(Colors.Red);
            }

            panel.Children.Add(txt);

            return panel;
        }

        // User has changed the dashboard option (sidebar)
        private void DashboardChanged(object sender, SelectionChangedEventArgs e) {

            // Ensure elements are valid / not null
            if (Dashboard != null && MyCurrencyPage != null) {
                
                // Dashboard selected
                if (Browse.IsSelected) {
                    MyCurrencyPage.Visibility = Visibility.Collapsed;
                    Dashboard.Visibility = Visibility.Visible;

                // myCurrencies selected
                } else if (MyCurrencies.IsSelected) {
                    Dashboard.Visibility = Visibility.Collapsed;
                    MyCurrencyPage.Visibility = Visibility.Visible;
                }
            }
        }

        // User clicked the "lock application" button
        private void LockBtn_Click(object sender, RoutedEventArgs e) {
            // Lock the application
            LockApplication();
        }

        // Next dashboard page...
        private void NextPage_Click(object sender, RoutedEventArgs e) {
            RequestNewDashboard(APIKey, ++dashboardPage);

            // Scroll to top
            DashboardScroll.ChangeView(0, 0, 1);
        }

        // Previous dashboard page...
        private void PrevPage_Click(object sender, RoutedEventArgs e) {
            if (dashboardPage > 1) {
                RequestNewDashboard(APIKey, --dashboardPage);

                // Scroll to top
                DashboardScroll.ChangeView(0, 0, 1);
            }
        }

        // Updated what kind of currency to use and refresh the dashboard
        private void ChangeCurrencyType_Click(object sender, RoutedEventArgs e) {
            currencyType = CurrencyType.Text;

            // update dashboard
            RequestNewDashboard(APIKey);
        }

        // Change / update the value displayed as the users currency
        public bool ChangeCash(double amount) {
            double value = double.Parse(Cash.Text) + amount;

            // Check if user has enough money
            if (value < 0) {
                new MessageDialog("Error:\nNot enough money").ShowAsync();
                return false;
            }

            // Format and set cash
            Cash.Text = Formatter.FormatGroupedDecimalAndRound(value);

            return true;
        }

        // Update my currency page with myCurrencies
        private void UpdateMyCurrencies() {
            MyCurrencyPageStackPanel.Children.Clear();

            // Foreach coin in our collection, make an elements and append it to myCurrencies
            foreach (PurchasedCoin coin in myCurrencies.Values) {
                MyCurrencyPageStackPanel.Children.Add(MyCurrencyElement(coin));
            }
        }

        // User clicked the refresh button!
        private void RefreshBtn_Click(object sender, RoutedEventArgs e) {
            RefreshBtn.IsEnabled = false;
            RequestNewDashboard(APIKey);
        }

        // Create a new myCurrency element
        public RelativePanel MyCurrencyElement(PurchasedCoin coin) {
            RelativePanel panel = new RelativePanel();
            panel.Margin = new Thickness(10);

            Currency currency = coin.currency;

            // Logo
            Image logo = ElementMaker.MakeImage(currency.logo_url);
            logo.Margin = new Thickness(0, 10, 0, 0);

            // Symbol
            TextBlock symbol = new TextBlock();
            symbol.Text = currency.symbol;
            symbol.Margin = new Thickness(10, 10, 0, 0);
            RelativePanel.SetRightOf(symbol, logo);

            // Amount
            TextBlock amount = new TextBlock();
            amount.Text = "Amount: " + coin.amount;
            amount.Margin = new Thickness(10, 0, 0, 0);
            RelativePanel.SetBelow(amount, symbol);
            RelativePanel.SetRightOf(amount, logo);

            // Purchase price
            TextBlock purchased_price = new TextBlock();
            purchased_price.Margin = new Thickness(10, 0, 0, 0);
            purchased_price.Text = "Purchase Price: " + Formatter.FormatGroupedDecimalAndRound(currency.price);
            RelativePanel.SetBelow(purchased_price, amount);
            RelativePanel.SetRightOf(purchased_price, logo);

            // Sell NumberButtonPanelThingy
            NumberButtonPanel sellPanel = new NumberButtonPanel("Sell " + currency.symbol, currency);
            sellPanel.amount.Maximum = coin.amount;
            RelativePanel.SetAlignRightWithPanel(sellPanel, true);

            sellPanel.button.Click += delegate (object sender, RoutedEventArgs args) {
                // Goto nomic and get the current price
                // Add price to cash
                // Do other stuffs...
            };

            // Add stuff to the panel
            panel.Children.Add(logo);
            panel.Children.Add(symbol);
            panel.Children.Add(amount);
            panel.Children.Add(purchased_price);
            panel.Children.Add(sellPanel);

            return panel;
        }
    }
}
