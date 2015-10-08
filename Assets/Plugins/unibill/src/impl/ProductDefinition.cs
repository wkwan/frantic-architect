using System;

namespace Unibill {
    public class ProductDefinition {
        public string PlatformSpecificId { get; private set; }
        public PurchaseType Type { get; private set; }

        public ProductDefinition (string platformSpecificId, PurchaseType type) {
            this.PlatformSpecificId = platformSpecificId;
            this.Type = type;
        }
    }
}

