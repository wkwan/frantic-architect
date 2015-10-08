using System;


namespace Unibill.Impl
{
	public interface IRawSamsungAppsBillingService
	{
		void initialise (SamsungAppsBillingService samsung);

		void getProductList (string json);

		void initiatePurchaseRequest (string productId);

		void restoreTransactions();
	}
}

