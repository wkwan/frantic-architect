using System;
using System.Linq;
using System.Collections.Generic;
using Uniject;

namespace Unibill.Impl {
	

	public class CurrencyManager {

		private IStorage storage;
		private UnibillConfiguration config;

		public string[] Currencies { get; private set; }


		public CurrencyManager(UnibillConfiguration config, IStorage storage) {
			this.storage = storage;
            this.config = config;
            Currencies = config.currencies.Select<VirtualCurrency, string>(x => x.currencyId).ToArray();
		}

		public void OnPurchased(string id) {
			foreach (var currency in config.currencies) {
				if (currency.mappings.ContainsKey (id)) {
					CreditBalance(currency.currencyId, currency.mappings[id]);
				}
			}
		}

		public decimal GetCurrencyBalance(string id) {
			return storage.GetInt (getKey(id), 0);
		}

		public void CreditBalance(string id, decimal amount) {
			storage.SetInt (getKey (id), (int) (GetCurrencyBalance (id) + amount));
		}

		public void SetBalance(string id, decimal amount) {
			storage.SetInt (getKey (id), (int)amount);
		}

		public bool DebitBalance(string id, decimal amount) {
			var balance = GetCurrencyBalance (id);
			if ((balance - amount) >= 0) {
				storage.SetInt (getKey (id), (int) (balance - amount));
				return true;
			}

			return false;
		}

		private string getKey(string id) {
			return string.Format ("com.outlinegames.unibill.currencies.{0}.balance", id);
		}
	}
}

