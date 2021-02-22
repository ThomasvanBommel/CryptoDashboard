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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CryptoDashboard
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void HamburgerMenu_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        // Unlock the rest of the application when the user submits a valid API key
        private void UnlockContent(object sender, RoutedEventArgs e) {
            // Get key from UI
            string key = APIKey.Text;

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
            } catch {
                new MessageDialog("Invalid API key").ShowAsync();
            }
        }
    }
}
