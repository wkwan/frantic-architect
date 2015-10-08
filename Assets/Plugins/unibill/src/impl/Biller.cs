//-----------------------------------------------------------------
//  Copyright 2013 Alex McAusland and Ballater Creations
//  All rights reserved
//  www.outlinegames.com
//-----------------------------------------------------------------
using System;
using System.Collections.Generic;
using Unibill.Impl;
using Uniject;
using Uniject.Impl;
using UnityEngine;


namespace Unibill.Impl {
    public enum BillerState {
        NOT_INITIALISED,
        INITIALISING,
        INITIALISED,
        INITIALISED_WITH_ERROR,
        INITIALISED_WITH_CRITICAL_ERROR,
    }
}

namespace Unibill {

    /// <summary>
    /// A purchase that succeeded, including the purchased item
    /// along with its purchase receipt.
    /// </summary>
    public class PurchaseEvent {
        public PurchasableItem PurchasedItem { get; private set; }

        /// <summary>
        /// Whether this purchase has just been made (true),
        /// or was restored from a prior install (false).
        /// </summary>
        public bool IsNewPurchase { get; private set; }
        public string Receipt { get; private set; }
        public String TransactionId { get; private set; }

        internal PurchaseEvent(PurchasableItem purchasedItem, bool isNewPurchase, string receipt, string transactionId) {
            this.PurchasedItem = purchasedItem;
            this.IsNewPurchase = isNewPurchase;
            this.Receipt = receipt;
            this.TransactionId = transactionId;
        }
    }


    /// <summary>
    /// The various reasons a purchase can fail.
    /// </summary>
    public enum PurchaseFailureReason {
        /// <summary>
        /// Typically indicates attempting a purchase
        /// before the biller has finished initialising.
        /// </summary>
        BILLER_NOT_READY,

        /// <summary>
        /// Billing may be disabled in security settings.
        /// </summary>
        BILLING_UNAVAILABLE,

        /// <summary>
        /// An attempt has been made to purchase a non
        /// consumable that is already owned.
        /// Non consumables cannot be repurchased.
        /// </summary>
        CANNOT_REPURCHASE_NON_CONSUMABLE,

        /// <summary>
        /// The item was reported unavailable by the billing system.
        /// </summary>
        ITEM_UNAVAILABLE,

        /// <summary>
        /// Signature validation of the purchase's receipt failed.
        /// </summary>
        SIGNATURE_INVALID,

        /// <summary>
        /// The user opted to cancel rather than proceed with the purchase.
        /// This is not specified on platforms that do not distinguish
        /// cancellation from other failure (Amazon, Microsoft).
        /// </summary>
        USER_CANCELLED,

        /// <summary>
        /// There was a problem with the payment.
        /// This is unique to Apple platforms.
        /// </summary>
        PAYMENT_DECLINED,

        /// <summary>
        /// A catch all for remaining purchase problems.
        /// </summary>
        UNKNOWN
    }

    /// <summary>
    /// A purchase that failed including the item under purchase,
    /// the reason for the failure and any additional information.
    /// </summary>
    public class PurchaseFailedEvent {
        public PurchasableItem PurchasedItem { get; private set; }
        public PurchaseFailureReason Reason { get; private set; }
        public string Message { get; private set; }

        internal PurchaseFailedEvent(PurchasableItem purchasedItem, PurchaseFailureReason reason, string message) {
            this.PurchasedItem = purchasedItem;
            this.Reason = reason;
            this.Message = message;
        }
    }

    /// <summary>
    /// Singleton that composes the various components of Unibill.
    /// All billing events are routed through the Biller for recording.
    /// Purchase events are logged in the transaction database.
    /// </summary>
    public class Biller : IBillingServiceCallback {
        public UnibillConfiguration InventoryDatabase { get; private set; }
        private TransactionDatabase transactionDatabase;
        private ILogger logger;
        private HelpCentre help;
        private ProductIdRemapper remapper;
		private CurrencyManager currencyManager;
        private bool restoreInProgress;
        public IBillingService billingSubsystem { get; private set; }

        public event Action<bool> onBillerReady;
		public event Action<PurchaseEvent> onPurchaseComplete;
		public event Action<bool> onTransactionRestoreBegin;
        public event Action<bool> onTransactionsRestored;
        public event Action<PurchasableItem> onPurchaseRefunded;
        public event Action<PurchaseFailedEvent> onPurchaseFailed;
        public event Action<PurchasableItem> onPurchaseDeferred;

        public BillerState State { get; private set; }
        public List<UnibillError> Errors { get; private set; }
        public bool Ready {
            get { return State == BillerState.INITIALISED || State == BillerState.INITIALISED_WITH_ERROR; }
        }

        public string[] CurrencyIdentifiers {
            get {
                return currencyManager.Currencies;
            }
        }

		public Biller (UnibillConfiguration config, TransactionDatabase tDb, IBillingService billingSubsystem, ILogger logger, HelpCentre help, ProductIdRemapper remapper, CurrencyManager currencyManager) {
            this.InventoryDatabase = config;
            this.transactionDatabase = tDb;
            this.billingSubsystem = billingSubsystem;
            this.logger = logger;
            logger.prefix = "UnibillBiller";
            this.help = help;
            this.Errors = new List<UnibillError> ();
            this.remapper = remapper;
			this.currencyManager = currencyManager;
            this.State = BillerState.NOT_INITIALISED;
        }

        public void Initialise () {
            if (State == BillerState.INITIALISING) {
                logError (UnibillError.BILLER_NOT_READY);
                return;
            }

            if (InventoryDatabase.AllPurchasableItems.Count == 0) {
                logError(UnibillError.UNIBILL_NO_PRODUCTS_DEFINED);
                onSetupComplete(false);
                return;
            }

            State = BillerState.INITIALISING;
            billingSubsystem.initialise(this);
        }

        public int getPurchaseHistory (PurchasableItem item) {
            return transactionDatabase.getPurchaseHistory(item);
        }

        public int getPurchaseHistory (string purchasableId) {
            var item = InventoryDatabase.getItemById (purchasableId);
            if (null == item) {
                // A warning will already have been logged.
                return -1;
            }
            return getPurchaseHistory(item);
        }

		public decimal getCurrencyBalance(string identifier) {
            return currencyManager.GetCurrencyBalance(identifier);
		}

		public void creditCurrencyBalance(string identifier, decimal amount) {
            currencyManager.CreditBalance(identifier, amount);
        }

		public bool debitCurrencyBalance(string identifier, decimal amount) {
            return currencyManager.DebitBalance(identifier, amount);
        }

        public void purchase (PurchasableItem item, string developerPayload = "") {
            if (State == BillerState.INITIALISING) {
                logError (UnibillError.BILLER_NOT_READY);
                onPurchaseFailedEvent(item, PurchaseFailureReason.BILLER_NOT_READY);
                return;
            } else if (State == BillerState.INITIALISED_WITH_CRITICAL_ERROR) {
                logError (UnibillError.UNIBILL_INITIALISE_FAILED_WITH_CRITICAL_ERROR);
                onPurchaseFailedEvent (item, PurchaseFailureReason.BILLING_UNAVAILABLE);
                return;
            }

            if (null == item) {
                logger.LogError ("Trying to purchase null PurchasableItem");
                return;
            }

            if (!item.AvailableToPurchase) {
                logError(UnibillError.UNIBILL_MISSING_PRODUCT, item.Id);
                return;
            }

            if (item.PurchaseType == PurchaseType.NonConsumable && transactionDatabase.getPurchaseHistory (item) > 0) {
                logError(UnibillError.UNIBILL_ATTEMPTING_TO_PURCHASE_ALREADY_OWNED_NON_CONSUMABLE);
                onPurchaseFailedEvent (item, PurchaseFailureReason.CANNOT_REPURCHASE_NON_CONSUMABLE);
                return;
            }
            
            billingSubsystem.purchase(remapper.mapItemIdToPlatformSpecificId(item), developerPayload);
            logger.Log("purchase({0})", item.Id);
        }

        public void purchase (string purchasableId, string developerPayload = "") {
            PurchasableItem item = InventoryDatabase.getItemById (purchasableId);
            if (null == item) {
                logger.LogWarning("Unable to purchase unknown item with id: {0}", purchasableId);
            }
            purchase(item, developerPayload);
        }

        public void restoreTransactions () {
            logger.Log("restoreTransactions()");
            if (!Ready) {
                logError(UnibillError.BILLER_NOT_READY);
                return;
            }
            restoreInProgress = true;
            if (null != onTransactionRestoreBegin) {
                onTransactionRestoreBegin (true);
            }
            billingSubsystem.restoreTransactions ();
        }

        public void onPurchaseSucceeded (string id, string receipt, string transactionId) {
            onPurchaseSucceeded (id, !restoreInProgress, receipt, transactionId);
        }

        private void onPurchaseSucceeded (string id, bool isNewPurchase, string receipt, string transactionId) {
            if (!verifyPlatformId (id)) {
                // We still need to close the transaction.
                billingSubsystem.finishTransaction(null, transactionId);
                return;
            }
            if (null != receipt) {
                this.onPurchaseReceiptRetrieved (id, receipt);
            }

            PurchasableItem item = remapper.getPurchasableItemFromPlatformSpecificId (id);
            if (item.PurchaseType == PurchaseType.NonConsumable) {
                if (transactionDatabase.getPurchaseHistory (item) > 0) {
                    logger.Log ("Ignoring multi purchase of non consumable " + item.Id);
                    billingSubsystem.finishTransaction (item, transactionId);
                    return;
                }
            }

            if (transactionDatabase.recordPurchase (item, transactionId)) {
                currencyManager.OnPurchased (item.Id);
                if (null != onPurchaseComplete) {
                    logger.Log("onPurchaseSucceeded {0} {1})", item.Id, transactionId);
                    onPurchaseComplete (new PurchaseEvent (item, isNewPurchase, receipt, transactionId));
                }
            }

            // We don't put this in a try finally since we want any 
            // exceptions to prevent the transaction closing.
            billingSubsystem.finishTransaction (item, transactionId);
        }
        
        public void onSetupComplete (bool available) {
            logger.Log("onSetupComplete({0})", available);
            this.State = available ? (Errors.Count > 0 ? BillerState.INITIALISED_WITH_ERROR : BillerState.INITIALISED) : BillerState.INITIALISED_WITH_CRITICAL_ERROR;
            if (onBillerReady != null) {
                onBillerReady(Ready);
            }
        }

        public void onPurchaseDeferredEvent (string id)
        {
            if (!verifyPlatformId (id)) {
                return;
            }

            PurchasableItem item = remapper.getPurchasableItemFromPlatformSpecificId(id);
            logger.Log("onPurchaseDeferredEvent({0})", item.Id);
            if (onPurchaseDeferred != null) {
                onPurchaseDeferred(item);
            }
        }

        public void onPurchaseRefundedEvent (string id) {
            if (!verifyPlatformId (id)) {
                return;
            }
            PurchasableItem item = remapper.getPurchasableItemFromPlatformSpecificId(id);
            logger.Log("onPurchaseRefundedEvent({0})", item.Id);
            transactionDatabase.onRefunded(item);
            if (onPurchaseRefunded != null) {
                onPurchaseRefunded(item);
            }
        }

        public void onPurchaseFailedEvent (PurchaseFailureDescription description) {
            if (!verifyPlatformId (description.ProductId)) {
                return;
            }

            PurchasableItem item = remapper.getPurchasableItemFromPlatformSpecificId(description.ProductId);
            logger.Log("onPurchaseFailedEvent({0})", item.Id);
            if (null != onPurchaseFailed) {
                onPurchaseFailedEvent (item, description.Reason);
            }
        }
        public void onTransactionsRestoredSuccess () {
            logger.Log("onTransactionsRestoredSuccess()");
            restoreInProgress = false;
            if (onTransactionsRestored != null) {
                onTransactionsRestored(true);
            }
        }

        public void ClearPurchases() {
            foreach (var item in InventoryDatabase.AllPurchasableItems) {
                transactionDatabase.clearPurchases (item);
            }
        }

        public void onTransactionsRestoredFail(string error) {
            logger.Log("onTransactionsRestoredFail({0})", error);
            restoreInProgress = false;
            onTransactionsRestored(false);
        }

        public bool isOwned(PurchasableItem item) {
            return getPurchaseHistory(item) > 0;
        }

        public void setAppReceipt(string receipt) {
            foreach (var item in InventoryDatabase.AllPurchasableItems) {
                if (getPurchaseHistory (item) > 0) {
                    item.receipt = receipt;
                }
            }
        }

        public void onSetupComplete (List<ProductDescription> products) {
            foreach (var product in products) {
                if (remapper.canMapProductSpecificId(product.PlatformSpecificID)) {
                    var item = remapper.getPurchasableItemFromPlatformSpecificId(product.PlatformSpecificID);

                    item.AvailableToPurchase = true;
                    item.localizedPriceString = product.Price;
                    item.localizedTitle = product.Title;
                    item.localizedDescription = product.Description;
                    item.isoCurrencySymbol = product.ISOCurrencyCode;
                    item.priceInLocalCurrency = product.PriceDecimal;
                    item.receipt = product.Receipt;

                    if (!string.IsNullOrEmpty (product.Receipt)) {
                        // Consider these purchases as restored rather than new.
                        // Interrupted purchases will (falsely) be marked 
                        // as restored.
                        onPurchaseSucceeded (product.PlatformSpecificID, false, product.Receipt, product.TransactionID);
                    }
                } else {
                    logger.LogError("Warning: Unknown product identifier: {0}", product.PlatformSpecificID);
                }
            }

            var available = false;
            foreach (var item in InventoryDatabase.AllPurchasableItems) {
                if (!item.AvailableToPurchase) {
                    logError (UnibillError.UNIBILL_MISSING_PRODUCT, item.Id, item.LocalId);
                }
                else {
                    available = true;
                }
            }

            onSetupComplete(available);
        }


        /// <summary>
        /// Get access to (a subset of) the Apple billing service.
        /// 
        /// This will return null on non storekit platforms.
        /// </summary>
        /// <returns>The apple extensions.</returns>
        public IAppleExtensions getAppleExtensions() {
            return billingSubsystem as IAppleExtensions;
        }

        public void logError (UnibillError error) {
            logError(error, new object[0]);
        }

        public void logError (UnibillError error, params object[] args) {
            Errors.Add(error);
            logger.LogError(help.getMessage(error), args);
        }

        public void onPurchaseReceiptRetrieved(string platformSpecificItemId, string receipt) {
            if (remapper.canMapProductSpecificId (platformSpecificItemId)) {
                var item = remapper.getPurchasableItemFromPlatformSpecificId (platformSpecificItemId);
                item.receipt = receipt;
            }
        }

        private void onPurchaseFailedEvent(PurchasableItem item, PurchaseFailureReason reason) {
            onPurchaseFailed(new PurchaseFailedEvent(item, reason, null));
        }

        private bool verifyPlatformId (string platformId) {
            if (!remapper.canMapProductSpecificId (platformId)) {
                logError(UnibillError.UNIBILL_UNKNOWN_PRODUCTID, platformId);
                return false;
            }
            return true;
        }
    }
}
