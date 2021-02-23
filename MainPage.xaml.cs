using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.UI.Popups;
using System.Diagnostics;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using Windows.UI.Text;

namespace CryptoDashboard {
    public sealed partial class MainPage : Page {
        string APIKey;
        int dashboardPage = 1;

        public MainPage() {
            this.InitializeComponent();
        }

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
                request(key);
            } else {
                // User submitted empty key
                new MessageDialog("Empty submission. Try again.").ShowAsync();

                // Enable button
                UnlockButton.IsEnabled = true;
            }
        }

        // Request a response from the Nomic API
        private async void request(string key, int page=1, int per_page=20) {
            // Create an HTTP client object
            HttpClient client = new HttpClient();

            // Nomic endpoint
            Uri uri = new Uri(
                "https://api.nomics.com/v1/currencies/ticker?convert=CAD&status=active&sort=rank&key=" + key +
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
            }

            // Deserialize json and update the dashboard
            UpdateDashboard(list);

            // Set API key for use later
            APIKey = key;

            // Updated dashboard page number
            dashboardPage = page;

            // Enable unlock button
            UnlockButton.IsEnabled = true;
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
        }

        // Deserialize JSON to Currency[] object
        private List<Currency> deserialize(string json) {
            return JsonConvert.DeserializeObject<List<Currency>>(json);
        }

        private void UpdateDashboard(List<Currency> currencies) {
            // Clear the dashboard
            //Currencies.Children.Clear();
            DashboardGrid.Children.Clear();
            DashboardGrid.RowDefinitions.Clear();

            // Keep track of which row we're on
            int count = 0;

            // For each currency
            foreach (Currency currency in currencies) {
                // Add row to the grid
                DashboardGrid.RowDefinitions.Add(new RowDefinition());

                // Create main element
                RelativePanel main = CreateElement(currency);
                Grid.SetColumn(main, 0);
                Grid.SetRow(main, count);

                // Create 1d element
                RelativePanel _1 = CreateElement1d(currency._1d.price_change);
                Grid.SetColumn(_1, 1);
                Grid.SetRow(_1, count);

                // Create 30d element
                RelativePanel _30 = CreateElement1d(currency._30d.price_change);
                Grid.SetColumn(_30, 2);
                Grid.SetRow(_30, count);

                // Create 365d element
                RelativePanel _365 = CreateElement1d(currency._365d.price_change);
                Grid.SetColumn(_365, 3);
                Grid.SetRow(_365, count);

                // Create YTD element
                RelativePanel _ytd = CreateElement1d(currency.ytd.price_change);
                Grid.SetColumn(_ytd, 4);
                Grid.SetRow(_ytd, count);

                // Increment count
                count++;

                // Add content to the grid
                DashboardGrid.Children.Add(main);
                DashboardGrid.Children.Add(_1);
                DashboardGrid.Children.Add(_30);
                DashboardGrid.Children.Add(_365);
                DashboardGrid.Children.Add(_ytd);
            }
        }

        private RelativePanel CreateElement(Currency currency) {
            RelativePanel panel = new RelativePanel();
            panel.Margin = new Thickness(10);

            // Check image source type
            ImageSource source = null;
            if (currency.logo_url != "") {
                if (currency.logo_url.Substring(currency.logo_url.Length - 3) == "svg") {
                    source = new SvgImageSource(new Uri(currency.logo_url));
                } else {
                    source = new BitmapImage(new Uri(currency.logo_url));
                }
            }

            // Logo
            Image logo = new Image();
            if(source != null) logo.Source = source;
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

        private void DashboardChanged(object sender, SelectionChangedEventArgs e) {

        }

        // User clicked the "lock application" button
        private void LockBtn_Click(object sender, RoutedEventArgs e) {
            // Lock the application
            LockApplication();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e) {
            request(APIKey, ++dashboardPage);
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e) {
            if (dashboardPage > 1) {
                request(APIKey, --dashboardPage);
            }
        }
    }
}
