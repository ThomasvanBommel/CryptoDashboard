using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoDashboard {
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 

    public class Data {
        public string volume { get; set; }
        public string price_change { get; set; }
        public string price_change_pct { get; set; }
        public string volume_change { get; set; }
        public string volume_change_pct { get; set; }
        public string market_cap_change { get; set; }
        public string market_cap_change_pct { get; set; }
    }

    public class Currency {
        public string id { get; set; }
        public string currency { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string logo_url { get; set; }
        public string status { get; set; }
        public double price { get; set; }
        public DateTime price_date { get; set; }
        public DateTime price_timestamp { get; set; }
        public string circulating_supply { get; set; }
        public string max_supply { get; set; }
        public string market_cap { get; set; }
        public string num_exchanges { get; set; }
        public string num_pairs { get; set; }
        public string num_pairs_unmapped { get; set; }
        public DateTime first_candle { get; set; }
        public DateTime first_trade { get; set; }
        public DateTime first_order_book { get; set; }
        public string rank { get; set; }
        public string rank_delta { get; set; }
        public string high { get; set; }
        public DateTime high_timestamp { get; set; }
        [JsonProperty("1h")]
        public Data _1h { get; set; }
        [JsonProperty("1d")]
        public Data _1d { get; set; }
        [JsonProperty("7d")]
        public Data _7d { get; set; }
        [JsonProperty("30d")]
        public Data _30d { get; set; }
        [JsonProperty("365d")]
        public Data _365d { get; set; }
        [JsonProperty("ytd")]
        public Data ytd { get; set; }
    }
}
