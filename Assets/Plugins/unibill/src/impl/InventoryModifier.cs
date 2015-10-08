using System;

/// <summary>
/// Provides write access to various properties of our
/// Inventory Database's PurchasableItems, so that we can
/// populate them when retrieved from the underlying billing system.
/// Eg localized price.
/// </summary>
partial class PurchasableItem {
    public class Writer {
        public static void setLocalizedPrice (PurchasableItem item, decimal price) {
            item.localizedPrice = price;
            item.localizedPriceString = price.ToString ();
        }

        public static void setLocalizedPrice (PurchasableItem item, string price) {
            item.localizedPriceString = price;
        }
        
        public static void setLocalizedTitle (PurchasableItem item, string title) {
            item.localizedTitle = title;
        }
        
        public static void setLocalizedDescription (PurchasableItem item, string description) {
            item.localizedDescription = description;
        }
		
		public static void setPriceInLocalCurrency(PurchasableItem item, decimal amount) {
			item.priceInLocalCurrency = amount;
		}
		
		public static void setISOCurrencySymbol(PurchasableItem item, string code) {
			item.isoCurrencySymbol = code;
		}

        public static void setAvailable(PurchasableItem item, bool available) {
            item.AvailableToPurchase = available;
        }

        public static void setReceipt(PurchasableItem item, string receipt) {
            item.receipt = receipt;
        }
    }
}
