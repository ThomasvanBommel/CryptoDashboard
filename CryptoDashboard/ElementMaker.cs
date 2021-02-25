using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CryptoDashboard {

    // Element maker for generated elements
    public class ElementMaker {

        // Get buffer or svg image and return it
        public static Image MakeImage(string url) {
            ImageSource source = null;

            // check if url is empty
            if (url != "") {
                // check if url is svg
                if (url.Substring(url.Length - 3) == "svg") {
                    source = new SvgImageSource(new Uri(url));
                } else {
                    // normal image
                    source = new BitmapImage(new Uri(url));
                }
            }

            // Logo
            Image img = new Image();
            if (source != null) img.Source = source;
            img.Margin = new Thickness(0, 0, 10, 0);
            img.Width = 64;
            img.Height = 64;

            return img;
        }
    }
}
