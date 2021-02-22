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

namespace CryptoDashboard {
    public sealed partial class MainPage : Page {
        string APIKey;

        public MainPage() {
            this.InitializeComponent();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        // Unlock the rest of the application when the user submits a valid API key
        private void UnlockContent(object sender, RoutedEventArgs e) {
            // Get key from UI
            string key = APIKeyInput.Text;

            // Check if key is empty
            if (key != "") {
                // Send request
                request(key);
            } else {
                // User submitted empty key
                new MessageDialog("Empty submission. Try again.").ShowAsync();
            }
        }

        // Request a response from the Nomic API
        private async void request(string key) {
            // Create an HTTP client object
            HttpClient client = new HttpClient();

            // Add a user-agent header to the GET request. 
            var headers = client.DefaultRequestHeaders;

            // Nomic endpoint
            Uri uri = new Uri("https://api.nomics.com/v1/currencies/ticker?id=BTC&per-page=1&key=" + key);

            // Object that will receive data asynchronously
            HttpResponseMessage response;
            string json;

            try {
                // Get response
                response = await client.GetAsync(uri);

                // Ensure success
                response.EnsureSuccessStatusCode();

                // Read data
                json = await response.Content.ReadAsStringAsync();

                // Set API key for use later
                APIKey = key;

                // Unlock the application
                UnlockApplication();
            } catch {
                // Inform user they've input an invalid key
                new MessageDialog("Invalid API key").ShowAsync();
            }
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

        private void DashboardChanged(object sender, SelectionChangedEventArgs e) {

        }

        // User clicked the "lock application" button
        private void LockBtn_Click(object sender, RoutedEventArgs e) {
            // Lock the application
            LockApplication();
        }
    }
}
