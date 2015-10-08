// UnibillStorekit.h
#import <StoreKit/StoreKit.h>

// Callback to Unity identifying the subject, JSON message body and optional app receipt.
// Note that App Receipts are sent separately to the JSON body for performance reasons.
typedef void (*UnibillSendMessageCallback)(const char* subject, const char* payload, const char* receipt);

@interface ReceiptRefresher : NSObject <SKRequestDelegate>

@property (nonatomic, strong) void (^callback)(BOOL);

@end

@interface EBPurchase : NSObject <SKProductsRequestDelegate, SKPaymentTransactionObserver> {
    UnibillSendMessageCallback messageCallback;
    NSArray* validProducts;
    NSSet* productIds;
    SKProductsRequest *request;
    NSMutableDictionary *pendingTransactions;
}


-(bool) requestProducts:(NSSet*)productId;
-(bool) purchaseProduct:(NSString*)requestedProduct;
-(void) finishTransaction:(NSString*)transactionIdentifier;
-(void)onTransactionSucceeded:(SKPaymentTransaction*)transaction;
-(void) restorePurchases;
-(NSString*) getAppReceipt;
-(void) addTransactionObserver;
@property (nonatomic, strong) ReceiptRefresher* receiptRefresher;
@property (nonatomic, strong) SKReceiptRefreshRequest* refreshRequest;

@end
