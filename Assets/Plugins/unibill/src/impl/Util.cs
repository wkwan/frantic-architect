using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Unibill.Impl {
    public class Util {
        public static string ReadAllText(string path) {
            #if !(UNITY_WP8 || UNITY_METRO)
            using (var r = new StreamReader(path)) {
                return r.ReadToEnd();
            }
            #else
            throw new NotImplementedException();
            #endif
        }

        public static void WriteAllText(string path, string text) {
            #if !(UNITY_WP8 || UNITY_METRO)
            using (var r = new StreamWriter(path)) {
                r.Write(text);
            }
            #else
            throw new NotImplementedException();
            #endif
        }

        public static List<ProductDescription> DeserialiseProductList(string json) {
            Dictionary<string, object> response = (Dictionary<string, object>)Unibill.Impl.MiniJSON.jsonDecode(json);
            return DeserialiseProductList (response);
        }

        public static List<ProductDescription> DeserialiseProductList(Dictionary<string, object> productHash) {
            var products = new List<ProductDescription> ();
            foreach (var identifier in productHash.Keys) {
                var details = (Dictionary<string, object>)productHash[identifier];
                var description = new ProductDescription (
                    identifier,
                    details.getString("price"),
                    details.getString("localizedTitle"),
                    details.getString("localizedDescription"),
                    details.getString("isoCurrencyCode"),
                    decimal.Parse (details.getString("priceDecimal")),
                    details.getString("receipt"),
                    details.getString("transactionId")
                );

                products.Add (description);
            }

            return products;
        }
    }
}
