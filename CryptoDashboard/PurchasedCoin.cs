using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoDashboard {

    // Coin purchased from my exchange; (I know their should be public getter/setters and private attributes.... time is of the essence!)
    public class PurchasedCoin {
        public Currency currency;
        public double amount;

        // Create a new purchased coin object
        public PurchasedCoin(Currency currency, double amount) {
            this.currency = currency;
            this.amount = amount;
        }
    }
}
