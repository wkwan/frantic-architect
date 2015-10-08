//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;
using Unibill.Impl;

public class AndroidManifestMerger : IXmlNamespaceResolver {
    private XNamespace xmlns = "http://schemas.android.com/apk/res/android";

	private const string ELEMENT_WRITE_EXTERNAL = 			   "manifest/uses-permission[@android:name='android.permission.WRITE_EXTERNAL_STORAGE']";
	private const string ELEMENT_GOOGLEPLAY_BILLING = 		   "manifest/uses-permission[@android:name='com.android.vending.BILLING']";
	private const string ELEMENT_INTERNET = 				   "manifest/uses-permission[@android:name='android.permission.INTERNET']";
	private const string ELEMENT_GOOGLEPLAY_BILLING_SERVICE =  "manifest/application/service[@android:name='com.outlinegames.unibill.BillingService']";
    private const string ELEMENT_GOOGLEPLAY_BILLING_RECEIVER = "manifest/application/receiver[@android:name='com.outlinegames.unibill.BillingReceiver']";
	private const string ELEMENT_GOOGLEPLAY_CUSTOM_ACTIVITY =  "manifest/application/activity[@android:name='com.outlinegames.unibill.PurchaseActivity']";
    private const string ELEMENT_AMAZON_SERVICE =              "//receiver[@android:name='com.amazon.device.iap.ResponseReceiver']";
	private const string ELEMENT_OLD_AMAZON_SERVICE = 			   "//receiver[@android:name='com.amazon.inapp.purchasing.ResponseReceiver']";
	private const string ELEMENT_SAMSUNG_BILLING_PERMISSION =  "manifest/uses-permission[@android:name='com.sec.android.iap.permission.BILLING']";
	private const string ELEMENT_SAMSUNG_INBOX_ACTIVITY      = "manifest/application/activity[@android:name='com.sec.android.iap.lib.activity.InboxActivity']";
	private const string ELEMENT_SAMSUNG_PAYMENT_ACTIVITY    = "manifest/application/activity[@android:name='com.sec.android.iap.lib.activity.PaymentActivity']";
	private const string ELEMENT_SAMSUNG_ITEM_ACTIVITY    = "manifest/application/activity[@android:name='com.sec.android.iap.lib.activity.ItemActivity']";

    private string[] COMMON_ELEMENTS = {
        ELEMENT_INTERNET,
    };

    private string[] AMAZON_ELEMENTS = {
        ELEMENT_AMAZON_SERVICE,
    };

	private string[] GOOGLE_PLAY_ELEMENTS = {
		ELEMENT_GOOGLEPLAY_BILLING,
		ELEMENT_GOOGLEPLAY_CUSTOM_ACTIVITY
	};

	private string[] SAMSUNG_APPS_ELEMENTS = {
		ELEMENT_SAMSUNG_BILLING_PERMISSION,
		ELEMENT_SAMSUNG_INBOX_ACTIVITY,
		ELEMENT_SAMSUNG_PAYMENT_ACTIVITY,
		ELEMENT_SAMSUNG_ITEM_ACTIVITY
	};

    /// <summary>
    /// The external storage permission is one way - we cannot remove it, since
    /// it might be there legitimately.
    /// </summary>
    public XDocument merge (XDocument manifest, BillingPlatform platform, bool sandbox) {
		switch (platform) {
        case BillingPlatform.GooglePlay:
            addElements (manifest, GOOGLE_PLAY_ELEMENTS);
            addElements (manifest, COMMON_ELEMENTS);
			// Old billing service v2 elements.
            removeElements (manifest, ELEMENT_GOOGLEPLAY_BILLING_RECEIVER);
            removeElements (manifest, ELEMENT_GOOGLEPLAY_BILLING_SERVICE);
            removeElements (manifest, AMAZON_ELEMENTS);
            removeElements (manifest, ELEMENT_OLD_AMAZON_SERVICE);
			removeElements (manifest, SAMSUNG_APPS_ELEMENTS);
			break;
		case BillingPlatform.AmazonAppstore:
			addElements (manifest, AMAZON_ELEMENTS);
			addElements (manifest, COMMON_ELEMENTS);
			if (sandbox) {
				addElements (manifest, ELEMENT_WRITE_EXTERNAL);
			}
			removeElements (manifest, GOOGLE_PLAY_ELEMENTS);
			removeElements (manifest, SAMSUNG_APPS_ELEMENTS);
			break;
		case BillingPlatform.SamsungApps:
			addElements (manifest, SAMSUNG_APPS_ELEMENTS);
			removeElements (manifest, GOOGLE_PLAY_ELEMENTS);
			removeElements (manifest, AMAZON_ELEMENTS);
            removeElements (manifest, ELEMENT_OLD_AMAZON_SERVICE);
			break;
		}
        return manifest;
    }

    private void addElements (XDocument manifest, params string[] elements) {
        foreach (var element in elements) {
            if (null == manifest.XPathSelectElement(element, this)) {
                manifest.XPathSelectElement(getParentElement(element), this).Add(getElement(element));
            }
        }
    }

    private void removeElements (XDocument manifest, params string[] elements) {
        foreach (var element in elements) {
            var e = manifest.XPathSelectElement(element, this);
            if (null != e) {
                e.Remove();
            }
        }
    }

    private string getParentElement (string el) {
        switch (el) {
        case ELEMENT_WRITE_EXTERNAL:
        case ELEMENT_INTERNET:
        case ELEMENT_GOOGLEPLAY_BILLING:
		case ELEMENT_SAMSUNG_BILLING_PERMISSION:
            return "manifest";
        default:
            return "manifest/application";
        }
    }

    private XElement getElement (string el) {
        switch (el) {
        case ELEMENT_WRITE_EXTERNAL:
            return new XElement("uses-permission", new XAttribute(xmlns + "name", "android.permission.WRITE_EXTERNAL_STORAGE"));
        case ELEMENT_INTERNET:
            return new XElement("uses-permission", new XAttribute(xmlns + "name", "android.permission.INTERNET"));
        case ELEMENT_GOOGLEPLAY_BILLING:
            return new XElement("uses-permission", new XAttribute(xmlns + "name", "com.android.vending.BILLING"));
        case ELEMENT_GOOGLEPLAY_BILLING_RECEIVER:
            XElement element = new XElement("receiver");
            element.Add(new XAttribute(xmlns + "name", "com.outlinegames.unibill.BillingReceiver"));
            
            XElement intentFilter = new XElement("intent-filter");
            element.Add(intentFilter);
            intentFilter.Add(new XElement("action", new XAttribute(xmlns + "name", "com.android.vending.billing.IN_APP_NOTIFY")));
            intentFilter.Add(new XElement("action", new XAttribute(xmlns + "name", "com.android.vending.billing.RESPONSE_CODE")));
            intentFilter.Add(new XElement("action", new XAttribute(xmlns + "name", "com.android.vending.billing.PURCHASE_STATE_CHANGED")));
            return element;
        case ELEMENT_GOOGLEPLAY_BILLING_SERVICE:
            return new XElement("service", new XAttribute(xmlns + "name", "com.outlinegames.unibill.BillingService"));
        case ELEMENT_AMAZON_SERVICE: {
            XElement receiver = new XElement("receiver");
            receiver.Add(new XAttribute(xmlns + "name", "com.amazon.device.iap.ResponseReceiver"));
            
            intentFilter = new XElement("intent-filter");
            XElement action = new XElement("action");
            action.Add(new XAttribute(xmlns + "name", "com.amazon.inapp.purchasing.NOTIFY"));
            action.Add(new XAttribute(xmlns + "permission", "com.amazon.inapp.purchasing.Permission.NOTIFY"));
            
            intentFilter.Add(action);
            receiver.Add(intentFilter);
            return receiver;
        }
        case ELEMENT_GOOGLEPLAY_CUSTOM_ACTIVITY: {
            var activity = new XElement("activity");
            activity.Add(new XAttribute(xmlns + "name", "com.outlinegames.unibill.PurchaseActivity"));
            activity.Add(new XAttribute(xmlns + "label", "@string/app_name"));
            activity.Add(new XAttribute(xmlns + "configChanges", "fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen"));
			activity.Add(new XAttribute(xmlns + "theme", "@android:style/Theme.Translucent.NoTitleBar.Fullscreen"));
            return activity;
        }
		case ELEMENT_SAMSUNG_BILLING_PERMISSION:
			return new XElement("uses-permission", new XAttribute(xmlns + "name", "com.sec.android.iap.permission.BILLING"));
		case ELEMENT_SAMSUNG_INBOX_ACTIVITY:
			var inboxActivity = new XElement("activity");
			inboxActivity.Add(new XAttribute(xmlns + "name", "com.sec.android.iap.lib.activity.InboxActivity"));
			inboxActivity.Add(new XAttribute(xmlns + "theme", "@android:style/Theme.Translucent.NoTitleBar.Fullscreen"));
			inboxActivity.Add(new XAttribute(xmlns + "configChanges", "orientation|screenSize"));
			return inboxActivity;
		case ELEMENT_SAMSUNG_PAYMENT_ACTIVITY:
			var paymentActivity = new XElement("activity");
			paymentActivity.Add(new XAttribute(xmlns + "name", "com.sec.android.iap.lib.activity.PaymentActivity"));
			paymentActivity.Add(new XAttribute(xmlns + "theme", "@android:style/Theme.Translucent.NoTitleBar.Fullscreen"));
			paymentActivity.Add(new XAttribute(xmlns + "configChanges", "orientation|screenSize"));
			return paymentActivity;
		case ELEMENT_SAMSUNG_ITEM_ACTIVITY:
			var itemActivity = new XElement("activity");
			itemActivity.Add(new XAttribute(xmlns + "name", "com.sec.android.iap.lib.activity.ItemActivity"));
			itemActivity.Add(new XAttribute(xmlns + "theme", "@android:style/Theme.Translucent.NoTitleBar.Fullscreen"));
			itemActivity.Add(new XAttribute(xmlns + "configChanges", "orientation|screenSize"));
			return itemActivity;
        }

        throw new ArgumentException(el);
    }

    #region IXmlNamespaceResolver implementation

    public System.Collections.Generic.IDictionary<string, string> GetNamespacesInScope (XmlNamespaceScope scope) {
        throw new NotImplementedException ();
    }

    public string LookupNamespace (string prefix) {
        if (prefix == "android") {
            return "http://schemas.android.com/apk/res/android";
        }
        throw new NotImplementedException ();
    }

    public string LookupPrefix (string ns) {
        throw new NotImplementedException ();
    }

    #endregion
}
