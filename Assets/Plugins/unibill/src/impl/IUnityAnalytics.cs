using System;

namespace Unibill.Impl {
    public interface IUnityAnalytics {
        void Transaction(string productId, decimal price,
            string currency, string receipt,
            string signature);
    }
}
