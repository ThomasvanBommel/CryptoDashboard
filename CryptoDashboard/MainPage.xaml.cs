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
using System.Threading.Tasks;

namespace CryptoDashboard {
    public sealed partial class MainPage : Page {
        string APIKey;
        string currencyType = "CAD";
        int dashboardPage = 1;
        Dictionary<string, PurchasedCoin> myCurrencies = new Dictionary<string, PurchasedCoin>();
        List<TextBlock> myCurrencyElements = new List<TextBlock>();

        bool onMyCurrencies = false;

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
                // Set API key
                APIKey = key;

                // Send request
                RequestNewDashboard();
            } else {
                // User submitted empty key
                new MessageDialog("Empty submission. Try again.").ShowAsync();

                // Enable button
                UnlockButton.IsEnabled = true;
            }
        }

        // Request from the nomic API
        private async Task<string> request(string target, Dictionary<string, string> args) {
            // Create an HTTP client object
            HttpClient client = new HttpClient();

            string url = "https://api.nomics.com/v1" + target + "?key=" + APIKey + "&";

            // Turn args into url params
            foreach (KeyValuePair<string, string> arg in args) 
                url += arg.Key + "=" + arg.Value + "&";

            // Nomic endpoint
            Uri uri = new Uri(url);

            // Object that will receive data asynchronously
            HttpResponseMessage message;
            string response = "";

            try {
                // Get response
                message = await client.GetAsync(uri);

                // Ensure success
                message.EnsureSuccessStatusCode();

                // Read data
                response = await message.Content.ReadAsStringAsync();
            } catch (Exception e) {

                // Inform user they've input an invalid key
                new MessageDialog("Error:\n" + e.Message).ShowAsync();
            }

            return response;
        }

        // Request a new / updated dashboard
        private async void RequestNewDashboard(int page = 1, int per_page = 20) {
            Dictionary<string, string> args = new Dictionary<string, string>() {
                { "convert", currencyType },
                { "page", page.ToString() },
                { "per-page", per_page.ToString() }
            };

            // Get json response
            string json = await request("/currencies/ticker", args);

            // Set list
            List<Currency> list = deserialize(json);

            // Unlock the application
            UnlockApplication();

            // Deserialize json and update the dashboard
            UpdateDashboard(list);

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
                    if (myCurrencies.ContainsKey(currency.symbol + currency.price)) {

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

                onMyCurrencies = false;

                // Dashboard selected
                if (Browse.IsSelected) {
                    MyCurrencyPage.Visibility = Visibility.Collapsed;
                    Dashboard.Visibility = Visibility.Visible;

                // myCurrencies selected
                } else if (MyCurrencies.IsSelected) {
                    Dashboard.Visibility = Visibility.Collapsed;
                    MyCurrencyPage.Visibility = Visibility.Visible;

                    // Update current price
                    UpdateMyCurrencyPrices();

                    onMyCurrencies = true;
                }
            }
        }

        // Update the mycurrencies current price
        private async void UpdateMyCurrencyPrices() {
            if (myCurrencies.Count == 0) return;

            // Hash set to hold unique values
            HashSet<string> symbols = new HashSet<string>();

            // Get all symbols from owned currencies (distinct / unique)
            foreach(PurchasedCoin coin in myCurrencies.Values) {
                symbols.Add(coin.currency.symbol);
            }

            // Request prices
            string json = await request("/currencies/ticker", new Dictionary<string, string> {
                { "ids", string.Join(",", symbols) },
                { "convert", currencyType }
            });

            // Deserialize data
            List<Currency> list = deserialize(json);

            Debug.WriteLine(json);

            // Update my currency current prices (bit of a hack :P)
            foreach (TextBlock current_price in myCurrencyElements) {
                Debug.WriteLine(current_price.AccessKey);

                string[] key = current_price.AccessKey.Split(":");

                if (key.Length == 2) {
                    // Find currency in the list of currencies equal to the elements access key
                    Currency currency = list.Find(x => x.id == key[0]);

                    // If found set text to the price returned
                    if (currency != null) {
                        current_price.Text = Formatter.FormatGroupedDecimalAndRound(currency.price);

                        try {
                            // Parse old price
                            double old_price = double.Parse(key[1]);

                            // Change color of text element +=green -=red
                            if (currency.price > old_price) {
                                current_price.Foreground = new SolidColorBrush(Colors.Green);
                            } else if (currency.price < old_price) {
                                current_price.Foreground = new SolidColorBrush(Colors.Red);
                            } else {
                                current_price.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        } catch { }
                    }
                }
            }

            RefreshBtn.IsEnabled = true;
        }

        // User clicked the "lock application" button
        private void LockBtn_Click(object sender, RoutedEventArgs e) {
            // Lock the application
            LockApplication();
        }

        // Next dashboard page...
        private void NextPage_Click(object sender, RoutedEventArgs e) {
            RequestNewDashboard(++dashboardPage);

            // Scroll to top
            DashboardScroll.ChangeView(0, 0, 1);
        }

        // Previous dashboard page...
        private void PrevPage_Click(object sender, RoutedEventArgs e) {
            if (dashboardPage > 1) {
                RequestNewDashboard(--dashboardPage);

                // Scroll to top
                DashboardScroll.ChangeView(0, 0, 1);
            }
        }

        // Updated what kind of currency to use and refresh the dashboard
        private void ChangeCurrencyType_Click(object sender, RoutedEventArgs e) {
            currencyType = CurrencyType.Text;

            // update dashboard
            RequestNewDashboard();
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
            myCurrencyElements.Clear();

            // Foreach coin in our collection, make an elements and append it to myCurrencies
            foreach (PurchasedCoin coin in myCurrencies.Values) {
                MyCurrencyPageStackPanel.Children.Add(MyCurrencyElement(coin));
            }
        }

        // User clicked the refresh button!
        private void RefreshBtn_Click(object sender, RoutedEventArgs e) {
            RefreshBtn.IsEnabled = false;

            if (onMyCurrencies) {
                UpdateMyCurrencies();
                UpdateMyCurrencyPrices();
            } else {
                RequestNewDashboard();
            }
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

            // Current prices element / panel / thingy
            StackPanel dataPanel = new StackPanel();
            dataPanel.Margin = new Thickness(160, 10, 0, 0);
            RelativePanel.SetRightOf(dataPanel, symbol);

            // Currencies current price
            TextBlock current_price_txt = new TextBlock();
            current_price_txt.Text = "Current Price:";
            dataPanel.Children.Add(current_price_txt);

            // Currencies current price
            TextBlock current_price = new TextBlock();
            current_price.Text = "Unknown";
            current_price.AccessKey = currency.symbol + ":" + currency.price;
            dataPanel.Children.Add(current_price);
            myCurrencyElements.Add(current_price);

            // Sell NumberButtonPanelThingy
            NumberButtonPanel sellPanel = new NumberButtonPanel("Sell " + currency.symbol, currency);
            sellPanel.amount.Maximum = coin.amount;
            RelativePanel.SetAlignRightWithPanel(sellPanel, true);

            // Update amount when changes (relative to the current price) ((NOT a very nice solution))
            sellPanel.amount.ValueChanged += delegate (NumberBox sender, NumberBoxValueChangedEventArgs args) {
                try {
                    double price = double.Parse(current_price.Text);
                    sellPanel.calculated_price.Text = Formatter.FormatGroupedDecimalAndRound(sellPanel.amount.Value * price);
                } catch { }
            };

            // Sell button based on current price elements value
            sellPanel.button.Click += delegate (object sender, RoutedEventArgs args) {
                try {
                    double price = double.Parse(current_price.Text);

                    // Update cash money!
                    ChangeCash(sellPanel.amount.Value * price);
                } catch (Exception e) {
                    throw e;
                    return;
                }

                // Remove amount from mycurrencies
                coin.amount -= sellPanel.amount.Value;

                // Remove if amount == 0
                if (coin.amount <= 0)
                    myCurrencies.Remove(coin.currency.symbol + coin.currency.price);

                // Update UI
                UpdateMyCurrencies();
                UpdateMyCurrencyPrices();
            };

            // Add stuff to the panel
            panel.Children.Add(logo);
            panel.Children.Add(symbol);
            panel.Children.Add(amount);
            panel.Children.Add(purchased_price);
            panel.Children.Add(sellPanel);
            panel.Children.Add(dataPanel);

            return panel;
        }
    }
}
