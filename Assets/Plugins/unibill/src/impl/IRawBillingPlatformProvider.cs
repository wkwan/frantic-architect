using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unibill.Impl;
using Uniject;

namespace Unibill.Impl {
    public interface IRawBillingPlatformProvider {
        IRawGooglePlayInterface getGooglePlay();

        IRawAmazonAppStoreBillingInterface getAmazon();

        IStoreKitPlugin getStorekit();
		IRawSamsungAppsBillingService getSamsung();
        ILevelLoadListener getLevelLoadListener();
        IHTTPClient getHTTPClient(IUtil util);
    }
}
