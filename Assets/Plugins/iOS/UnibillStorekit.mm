#import "UnibillStorekit.h"
#if UNIBILLOSX
#import "Base64.h"
#endif

// Encapsulates an SKReceiptRefreshRequest.

@implementation ReceiptRefresher

-(id) initWithCallback:(void (^)(BOOL))callbackBlock {
    self.callback = callbackBlock;
    return [super init];
}

-(void) requestDidFinish:(SKRequest *)request {
    self.callback(true);
}

-(void) request:(SKRequest *)request didFailWithError:(NSError *)error {
    self.callback(false);
}

@end

@implementation EBPurchase

// The max time we wait in between retrying failed SKProductRequests.
static const int MAX_REQUEST_PRODUCT_RETRY_DELAY = 60;

// Track our accumulated delay.
int delayInSeconds = 2;

-(NSString*) getAppReceipt {
    
    NSBundle* bundle = [NSBundle mainBundle];
    if ([bundle respondsToSelector:@selector(appStoreReceiptURL)]) {
        NSURL *receiptURL = [bundle appStoreReceiptURL];
        if ([[NSFileManager defaultManager] fileExistsAtPath:[receiptURL path]]) {
            NSData *receipt = [NSData dataWithContentsOfURL:receiptURL];
            
#if UNIBILLOSX
            // The base64EncodedStringWithOptions method was only added in OSX 10.9.
            NSString* result = [receipt base64EncodedString];
#else
            NSString* result = [receipt base64EncodedStringWithOptions:0];
#endif
            
            return result;
        }
    }
    
    NSLog(@"Unibill: No App Receipt found!");
    return @"";
}

-(void) UnibillSendMessage:(NSString*) subject payload:(NSString*) payload {
    messageCallback(subject.UTF8String, payload.UTF8String, @"".UTF8String);
}

-(void) UnibillSendMessage:(NSString*) subject payload:(NSString*) payload receipt:(NSString*) receipt {
    messageCallback(subject.UTF8String, payload.UTF8String, receipt.UTF8String);
}

-(void) setCallback:(UnibillSendMessageCallback)callback {
    messageCallback = callback;
}

#if !UNIBILLOSX
-(BOOL) isiOS6OrEarlier {
    float version = [[[UIDevice currentDevice] systemVersion] floatValue];
    return version < 7;
}
#endif

// Retrieve a receipt for the transaction, which will either
// be the old style transaction receipt on <= iOS 6,
// or the App Receipt in OSX and iOS 7+.
-(NSString*) selectReceipt:(SKPaymentTransaction*) transaction {
#if UNIBILLOSX
    return [self getAppReceipt];
#else
    if ([self isiOS6OrEarlier]) {
        if (nil == transaction) {
            return @"";
        }
        NSString* receipt;
        receipt = [[NSString alloc] initWithData:transaction.transactionReceipt encoding: NSUTF8StringEncoding];
        
        return receipt;
    } else {
        return [self getAppReceipt];
    }
#endif
}

-(void) refreshReceipt {
    #if !UNIBILLOSX
    if ([self isiOS6OrEarlier]) {
        NSLog(@"Unibill: refreshReceipt not supported on iOS < 7!");
        return;
    }
    #endif

    self.receiptRefresher = [[ReceiptRefresher alloc] initWithCallback:^(BOOL success) {
        NSLog(@"Unibill: refreshReceipt status %d", success);
        if (success) {
            [self UnibillSendMessage:@"onAppReceiptRefreshed" payload:[self getAppReceipt]];
        } else {
            [self UnibillSendMessage:@"onAppReceiptRefreshFailed" payload:nil];
        }
    }];
    self.refreshRequest = [[SKReceiptRefreshRequest alloc] init];
    self.refreshRequest.delegate = self.receiptRefresher;
    [self.refreshRequest start];
}

// Handle a new or restored purchase transaction by informing Unity.
- (void)onTransactionSucceeded:(SKPaymentTransaction*)transaction {
    NSString* transactionId = transaction.transactionIdentifier;
    
    // This should never happen according to Apple's docs, but it does!
    if (nil == transactionId) {
        // Make something up, allowing us to identifiy the transaction when finishing it.
        transactionId = [[NSUUID UUID] UUIDString];
        NSLog(@"Unibill: missing transaction Identifier!");
    }
    
    // Item was successfully purchased or restored.
    NSMutableDictionary* dic;
    dic = [[NSMutableDictionary alloc] init];
    [dic setObject:transaction.payment.productIdentifier forKey:@"productId"];
    [dic setObject:transactionId forKey:@"transactionIdentifier"];
    
    NSData* data;
    data = [NSJSONSerialization dataWithJSONObject:dic options:0 error:nil];
    NSString* result;
    result = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    
    if (nil == [pendingTransactions objectForKey:transactionId]) {
        [pendingTransactions setObject:transaction forKey:transactionId];
    }
    
    [self UnibillSendMessage:@"onProductPurchaseSuccess" payload:result receipt:[self selectReceipt:transaction]];
}

// Called back by managed code when the tranaction has been logged.
-(void) finishTransaction:(NSString *)transactionIdentifier {
    NSLog(@"Unibill: finishTransaction");
    SKPaymentTransaction* transaction = [pendingTransactions objectForKey:transactionIdentifier];
    if (nil != transaction) {
        NSLog(@"Unibill: finishing transaction %@", transactionIdentifier);
        [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
        [pendingTransactions removeObjectForKey:transactionIdentifier];
    } else {
        NSLog(@"Unibill: Transaction %@ not found!", transactionIdentifier);
    }
}

// Request information about our products from Apple.
-(bool) requestProducts:(NSSet*)paramIds
{
    productIds = [[NSSet alloc] initWithSet:paramIds];
    if (productIds != nil) {
        
        NSLog(@"Unibill: requestProducts:%@", productIds);
        if ([SKPaymentQueue canMakePayments]) {
            // Start an immediate poll.
            [self initiateProductPoll:0];
            
            return YES;
            
        } else {
            return NO;
        }
        
    } else {
        return NO;
    }
}

// Execute a product metadata retrieval request via GCD.
-(void) initiateProductPoll:(int) delayInSeconds
{
    dispatch_time_t popTime = dispatch_time(DISPATCH_TIME_NOW, delayInSeconds * NSEC_PER_SEC);
    dispatch_after(popTime, dispatch_get_main_queue(), ^(void) {
        NSLog(@"Requesting product data...");
        request = [[SKProductsRequest alloc] initWithProductIdentifiers:productIds];
        request.delegate = self;
        [request start];
    });
}

// Called by managed code when a user requests a purchase.
-(bool) purchaseProduct:(NSString*)requestedProductId
{
    // Look up our corresponding product.
    SKProduct* requestedProduct = nil;
    for (SKProduct* product in validProducts) {
        if ([product.productIdentifier isEqualToString:requestedProductId]) {
            requestedProduct = product;
            break;
        }
    }
    
    if (requestedProduct != nil) {
        
        NSLog(@"Unibill purchaseProduct: %@", requestedProduct.productIdentifier);
        
        if ([SKPaymentQueue canMakePayments]) {
            SKPayment *paymentRequest = [SKPayment paymentWithProduct:requestedProduct];
            [[SKPaymentQueue defaultQueue] addPayment:paymentRequest];
            
            return YES;
            
        } else {
            NSLog(@"Unibill purchaseProduct: IAP Disabled");
            
            return NO;
        }
        
    } else {
        [self onPurchaseFailed:requestedProductId reason:@"ITEM_UNAVAILABLE"];
        return YES;
    }
}

// Initiate a request to Apple to restore previously made purchases.
-(void) restorePurchases
{
    NSLog(@"Unibill restorePurchase");
    [[SKPaymentQueue defaultQueue] restoreCompletedTransactions];
}

// A transaction observer should be added at startup (by managed code)
// and maintained for the life of the app, since transactions can
// be delivered at any time.
-(void) addTransactionObserver {
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
}

#pragma mark -
#pragma mark SKProductsRequestDelegate Methods

// Store Kit returns a response from an SKProductsRequest.
- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response {
    
    // Parse the received product info.
    //self.validProduct = nil;
    NSUInteger count = [response.products count];
    if (count>0) {
        NSLog(@"Unibill: productsRequest:didReceiveResponse:%@", response.products);
        // Record our products.
        validProducts = [[NSArray alloc] initWithArray:response.products];
        NSMutableDictionary* dic = [[NSMutableDictionary alloc] init];
        
        NSMutableDictionary* products = [[NSMutableDictionary alloc] init];
        [dic setObject:products forKey:@"products"];
        [dic setObject:[self selectReceipt:nil]  forKey:@"appReceipt"];
        
        for (SKProduct* product in validProducts) {
            NSMutableDictionary* entry = [[NSMutableDictionary alloc] init];
            
            NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
            [numberFormatter setFormatterBehavior:NSNumberFormatterBehavior10_4];
            [numberFormatter setNumberStyle:NSNumberFormatterCurrencyStyle];
            [numberFormatter setLocale:product.priceLocale];
            NSString *formattedString = [numberFormatter stringFromNumber:product.price];
            
            if (NULL != product.price) {
                [entry setObject:product.price forKey:@"priceDecimal"];
            }
            
            if (NULL != product.priceLocale) {
                NSString *currencyCode = [product.priceLocale objectForKey:NSLocaleCurrencyCode];
                [entry setObject:currencyCode forKey:@"isoCurrencyCode"];
            }
            
            if (NULL == product.productIdentifier) {
                NSLog(@"Unibill: Product is missing an identifier!");
                continue;
            }
            
            if (NULL == formattedString) {
                NSLog(@"Unibill: Unable to format a localized price");
                [entry setObject:@"" forKey:@"price"];
            } else {
                [entry setObject:formattedString forKey:@"price"];
            }
            if (NULL == product.localizedTitle) {
                NSLog(@"Unibill: no localized title for: %@. Have your products been disapproved in itunes connect?", product.productIdentifier);
                [entry setObject:@"" forKey:@"localizedTitle"];
            } else {
                [entry setObject:product.localizedTitle forKey:@"localizedTitle"];
            }
            
            if (NULL == product.localizedDescription) {
                NSLog(@"Unibill: no localized description for: %@. Have your products been disapproved in itunes connect?", product.productIdentifier);
                [entry setObject:@"" forKey:@"localizedDescription"];
            } else {
                [entry setObject:product.localizedDescription forKey:@"localizedDescription"];
            }
            
            [products setObject:entry forKey:product.productIdentifier];
        }
        
        NSData *data = [NSJSONSerialization dataWithJSONObject:dic options:0 error:nil];
        NSString* result = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
        
        [self UnibillSendMessage:@"onProductListReceived" payload:result];
    } else {
        if (0 == [response.invalidProductIdentifiers count]) {
            // It seems we got no response at all.
        } else {
            // Call back to Unity - fail
            [self UnibillSendMessage:@"onProductListReceived" payload:@""];
        }
    }
    
}


#pragma mark -
#pragma mark SKPaymentTransactionObserver Methods
// A product metadata retrieval request failed.
// We handle it by retrying at an exponentially increasing interval.
- (void)request:(SKRequest *)request didFailWithError:(NSError *)error {
    delayInSeconds = MIN(MAX_REQUEST_PRODUCT_RETRY_DELAY, 2 * delayInSeconds);
    NSLog(@"Unibill: SKProductRequest::didFailWithError: %ld, %@. Unibill will retry in %i seconds", (long)error.code, error.description, delayInSeconds);
    
    [self initiateProductPoll:delayInSeconds];
}

- (void)requestDidFinish:(SKRequest *)req {
    request = nil;
}

- (void)onPurchaseFailed:(NSString*) productId reason:(NSString*)reason {
    NSMutableDictionary* dic = [[NSMutableDictionary alloc] init];
    [dic setObject:productId forKey:@"productId"];
    [dic setObject:reason forKey:@"reason"];

    NSData* data = [NSJSONSerialization dataWithJSONObject:dic options:0 error:nil];
    NSString* result = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];

    [self UnibillSendMessage:@"onProductPurchaseFailed" payload:result];
}

- (NSString*)purchaseErrorCodeToReason:(NSInteger) errorCode {
    switch (errorCode) {
        case SKErrorPaymentCancelled:
            return @"USER_CANCELLED";
        case SKErrorPaymentInvalid:
            return @"PAYMENT_DECLINED";
        case SKErrorPaymentNotAllowed:
            return @"BILLING_UNAVAILABLE";
    }

    return @"UNKNOWN";
}

// The transaction status of the SKPaymentQueue is sent here.
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions {
    NSLog(@"Unibill: updatedTransactions");
    for(SKPaymentTransaction *transaction in transactions) {
        switch (transaction.transactionState) {
                
            case SKPaymentTransactionStatePurchasing:
                // Item is still in the process of being purchased
                break;
                
            case SKPaymentTransactionStatePurchased:
            case SKPaymentTransactionStateRestored: {
                [self onTransactionSucceeded:transaction];
                break;
            }
            case SKPaymentTransactionStateDeferred:
                NSLog(@"Unibill: purchaseDeferred");
                [self UnibillSendMessage:@"onProductPurchaseDeferred" payload:transaction.payment.productIdentifier];
                break;
            case SKPaymentTransactionStateFailed:
                // Purchase was either cancelled by user or an error occurred.
                NSString* errorCode = [NSString stringWithFormat:@"%ld",(long)transaction.error.code];
                NSLog(@"Unibill: purchaseFailed: %@", errorCode);
                
                NSString* reason = [self purchaseErrorCodeToReason:transaction.error.code];
                [self onPurchaseFailed:transaction.payment.productIdentifier reason:reason];

                // Finished transactions should be removed from the payment queue.
                [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
                break;
        }
    }
}

// Called when one or more transactions have been removed from the queue.
- (void)paymentQueue:(SKPaymentQueue *)queue removedTransactions:(NSArray *)transactions
{
    // Nothing to do here.
}

// Called when SKPaymentQueue has finished sending restored transactions.
- (void)paymentQueueRestoreCompletedTransactionsFinished:(SKPaymentQueue *)queue {
    
    NSLog(@"Unibill paymentQueueRestoreCompletedTransactionsFinished");
    [self UnibillSendMessage:@"onTransactionsRestoredSuccess" payload:@""];
}

// Called if an error occurred while restoring transactions.
- (void)paymentQueue:(SKPaymentQueue *)queue restoreCompletedTransactionsFailedWithError:(NSError *)error
{
    NSLog(@"restoreCompletedTransactionsFailedWithError");
    // Restore was cancelled or an error occurred, so notify user.
    
    [self UnibillSendMessage:@"onTransactionsRestoredFail" payload:error.localizedDescription];
}


#pragma mark - Internal Methods & Events

- (id)init {
    if ( self = [super init] ) {
        validProducts = nil;
        pendingTransactions = [[NSMutableDictionary alloc] init];
    }
    return self;
}

@end

EBPurchase* _instance = NULL;

EBPurchase* _getInstance() {
    if (NULL == _instance) {
        _instance = [[EBPurchase alloc] init];
    }
    return _instance;
}

// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C" {
    
    bool _unibillPaymentsAvailable () {
        return [SKPaymentQueue canMakePayments];
    }
    
    void _unibillInitialise(UnibillSendMessageCallback callback) {
        [_getInstance() setCallback:callback];
    }
    
    void _unibillFinishTransaction (const char* transactionIdentifier) {
        NSString* id = [NSString stringWithUTF8String:transactionIdentifier];
        [_getInstance() finishTransaction:id];
    }
    
    void _unibillRequestProductData (const char* json) {
        NSLog(@"Unibill: requestProductData");
        NSString* jsonString = [NSString stringWithUTF8String:json];
        NSData* data = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
        NSArray* productIds = [NSJSONSerialization JSONObjectWithData:data options:0 error:nil];
        NSSet* ids = [NSSet setWithArray:productIds];
        [_getInstance() requestProducts:ids];
        
        NSLog(@"Unibill: Traceout: requestProductData");
    }
    
    void _unibillPurchaseProduct (const char* productId) {
        NSLog(@"Unibill: _unibillPurchaseProduct");
        NSString* str = [NSString stringWithUTF8String:productId];
        [_getInstance() purchaseProduct:str];
        NSLog(@"Unibill: Traceout: _unibillPurchaseProduct");
    }
    
    void _unibillRestoreTransactions() {
        NSLog(@"Unibill: _unibillRestoreTransactions");
        [_getInstance() restorePurchases];
        NSLog(@"Unibill: Traceout: _unibillRestoreTransactions");
    }
    
    void _unibillAddTransactionObserver() {
        NSLog(@"Unibill: _unibillAddTransactionObserver");
        [_getInstance() addTransactionObserver];
    }
    
    void _unibillRefreshAppReceipt() {
        NSLog(@"Unibill: _unibillRefreshAppReceipt");
        [_getInstance() refreshReceipt];
    }
}

