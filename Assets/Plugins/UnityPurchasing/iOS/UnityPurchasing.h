#import <StoreKit/StoreKit.h>

// Callback to Unity identifying the subject, JSON message body and optional app receipt.
// Note that App Receipts are sent separately to the JSON body for performance reasons.
typedef void (*UnityPurchasingCallback)(const char* subject, const char* payload, const char* receipt, const char* transactionId);

@interface ProductDefinition : NSObject
    
@property (nonatomic, strong) NSString *id;
@property (nonatomic, strong) NSString *storeSpecificId;
@property (nonatomic, strong) NSString *type;
@end

@interface ReceiptRefresher : NSObject <SKRequestDelegate>

@property (nonatomic, strong) void (^callback)(BOOL);

@end

@interface UnityPurchasing : NSObject <SKProductsRequestDelegate, SKPaymentTransactionObserver> {
    UnityPurchasingCallback messageCallback;
    NSArray* validProducts;
    NSSet* productIds;
    SKProductsRequest *request;
    NSMutableDictionary *pendingTransactions;
}

+ (NSArray*) deserializeProductDefs:(NSString*)json;
+ (ProductDefinition*) deserializeProductDef:(NSString*)json;
+ (NSString*) serializeProductMetadata:(NSArray*)products;

-(void) restorePurchases;
-(NSString*) getAppReceipt;
-(void) addTransactionObserver;
@property (nonatomic, strong) ReceiptRefresher* receiptRefresher;
@property (nonatomic, strong) SKReceiptRefreshRequest* refreshRequest;

@end
