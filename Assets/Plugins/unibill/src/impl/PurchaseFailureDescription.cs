using System;
using Unibill;


namespace Unibill.Impl {
    /// <summary>
    /// Represents a failed purchase as described 
    /// by a billing service.
    /// </summary>
    public class PurchaseFailureDescription {
        /// <summary>
        /// The platform specific product ID.
        /// </summary>
        public string ProductId { get; private set; }

        public PurchaseFailureReason Reason { get; private set; }

        public String Message { get; private set; }

        public PurchaseFailureDescription(string jsonHash) {
            var dic = jsonHash.hashtableFromJson();

            this.ProductId = dic.getString("productId");
            var reason = dic.getString ("reason");
            if (Enum.IsDefined (typeof(PurchaseFailureReason), reason)) {
                this.Reason = (PurchaseFailureReason)Enum.Parse (typeof(PurchaseFailureReason), reason);
            } else {
                this.Reason = PurchaseFailureReason.UNKNOWN;
            }
            this.Message = dic.getString ("message");
        }

        public PurchaseFailureDescription(string productId, PurchaseFailureReason reason, string message) {
            this.ProductId = productId;
            this.Reason = reason;
            this.Message = message;
        }
    }
}
