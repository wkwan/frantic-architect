#import "UnityPurchasing.h"
#if MAC_APPSTORE
#import "Base64.h"
#endif

@implementation ProductDefinition

@synthesize id;
@synthesize storeSpecificId;
@synthesize type;

@end

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

void UnityPurchasingLog(NSString *format, ...) {
    va_list args;
    va_start(args, format);
    NSString *message = [[NSString alloc] initWithFormat:format arguments:args];
    va_end(args);

    NSLog(@"UnityIAP:%@", message);
}

@implementation UnityPurchasing

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
            
#if MAC_APPSTORE
            // The base64EncodedStringWithOptions method was only added in OSX 10.9.
            NSString* result = [receipt mgb64_base64EncodedString];
#else
            NSString* result = [receipt base64EncodedStringWithOptions:0];
#endif
            
            return result;
        }
    }
    
    UnityPurchasingLog(@"No App Receipt found!");
    return @"";
}

-(void) UnitySendMessage:(NSString*) subject payload:(NSString*) payload {
    messageCallback(subject.UTF8String, payload.UTF8String, @"".UTF8String, @"".UTF8String);
}

-(void) UnitySendMessage:(NSString*) subject payload:(NSString*) payload receipt:(NSString*) receipt {
    messageCallback(subject.UTF8String, payload.UTF8String, receipt.UTF8String, @"".UTF8String);
}

-(void) UnitySendMessage:(NSString*) subject payload:(NSString*) payload receipt:(NSString*) receipt transactionId:(NSString*) transactionId {
    messageCallback(subject.UTF8String, payload.UTF8String, receipt.UTF8String, transactionId.UTF8String);
}

-(void) setCallback:(UnityPurchasingCallback)callback {
    messageCallback = callback;
}

#if !MAC_APPSTORE
-(BOOL) isiOS6OrEarlier {
    float version = [[[UIDevice currentDevice] systemVersion] floatValue];
    return version < 7;
}
#endif

// Retrieve a receipt for the transaction, which will either
// be the old style transaction receipt on <= iOS 6,
// or the App Receipt in OSX and iOS 7+.
-(NSString*) selectReceipt:(SKPaymentTransaction*) transaction {
#if MAC_APPSTORE
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
    #if !MAC_APPSTORE
    if ([self isiOS6OrEarlier]) {
        UnityPurchasingLog(@"RefreshReceipt not supported on iOS < 7!");
        return;
    }
    #endif

    self.receiptRefresher = [[ReceiptRefresher alloc] initWithCallback:^(BOOL success) {
        UnityPurchasingLog(@"RefreshReceipt status %d", success);
        if (success) {
            [self UnitySendMessage:@"onAppReceiptRefreshed" payload:[self getAppReceipt]];
        } else {
            [self UnitySendMessage:@"onAppReceiptRefreshFailed" payload:nil];
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
        UnityPurchasingLog(@"Missing transaction Identifier!");
    }
    
    // Item was successfully purchased or restored.
    if (nil == [pendingTransactions objectForKey:transactionId]) {
        [pendingTransactions setObject:transaction forKey:transactionId];
    }
    
    [self UnitySendMessage:@"OnPurchaseSucceeded" payload:transaction.payment.productIdentifier receipt:[self selectReceipt:transaction] transactionId:transactionId];
}

// Called back by managed code when the tranaction has been logged.
-(void) finishTransaction:(NSString *)transactionIdentifier {
    SKPaymentTransaction* transaction = [pendingTransactions objectForKey:transactionIdentifier];
    if (nil != transaction) {
        UnityPurchasingLog(@"Finishing transaction %@", transactionIdentifier);
        [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
        [pendingTransactions removeObjectForKey:transactionIdentifier];
    } else {
        UnityPurchasingLog(@"Transaction %@ not found!", transactionIdentifier);
    }
}

// Request information about our products from Apple.
-(void) requestProducts:(NSSet*)paramIds
{
    productIds = paramIds;
    UnityPurchasingLog(@"RequestProducts:%@", productIds);
    if ([SKPaymentQueue canMakePayments]) {
        // Start an immediate poll.
        [self initiateProductPoll:0];
    } else {
        // Send an InitializationFailureReason.
        [self UnitySendMessage:@"OnSetupFailed" payload:@"PurchasingUnavailable" ];
    }
}

// Execute a product metadata retrieval request via GCD.
-(void) initiateProductPoll:(int) delayInSeconds
{
    dispatch_time_t popTime = dispatch_time(DISPATCH_TIME_NOW, delayInSeconds * NSEC_PER_SEC);
    dispatch_after(popTime, dispatch_get_main_queue(), ^(void) {
        UnityPurchasingLog(@"Requesting product data...");
        request = [[SKProductsRequest alloc] initWithProductIdentifiers:productIds];
        request.delegate = self;
        [request start];
    });
}

// Called by managed code when a user requests a purchase.
-(void) purchaseProduct:(ProductDefinition*)productDef
{
    // Look up our corresponding product.
    SKProduct* requestedProduct = nil;
    for (SKProduct* product in validProducts) {
        if ([product.productIdentifier isEqualToString:productDef.storeSpecificId]) {
            requestedProduct = product;
            break;
        }
    }
    
    if (requestedProduct != nil) {
        UnityPurchasingLog(@"PurchaseProduct: %@", requestedProduct.productIdentifier);
        
        if ([SKPaymentQueue canMakePayments]) {
            SKPayment *paymentRequest = [SKPayment paymentWithProduct:requestedProduct];
            [[SKPaymentQueue defaultQueue] addPayment:paymentRequest];
        } else {
            UnityPurchasingLog(@"PurchaseProduct: IAP Disabled");
            [self onPurchaseFailed:productDef.storeSpecificId reason:@"PurchasingUnavailable"];
        }
        
    } else {
        [self onPurchaseFailed:productDef.storeSpecificId reason:@"ItemUnavailable"];
    }
}

// Initiate a request to Apple to restore previously made purchases.
-(void) restorePurchases
{
    UnityPurchasingLog(@"RestorePurchase");
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
    
    UnityPurchasingLog(@"ProductsRequest:didReceiveResponse:%@", response.products);
    // Record our products.
    validProducts = [[NSArray alloc] initWithArray:response.products];

    NSString* productJSON = [UnityPurchasing serializeProductMetadata:response.products];
    
    // Send the app receipt as a separate parameter to avoid JSON parsing a large string.
    [self UnitySendMessage:@"OnProductsRetrieved" payload:productJSON receipt:[self selectReceipt:nil] ];
}


#pragma mark -
#pragma mark SKPaymentTransactionObserver Methods
// A product metadata retrieval request failed.
// We handle it by retrying at an exponentially increasing interval.
- (void)request:(SKRequest *)request didFailWithError:(NSError *)error {
    delayInSeconds = MIN(MAX_REQUEST_PRODUCT_RETRY_DELAY, 2 * delayInSeconds);
    UnityPurchasingLog(@"SKProductRequest::didFailWithError: %ld, %@. Unity Purchasing will retry in %i seconds", (long)error.code, error.description, delayInSeconds);
    
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

    [self UnitySendMessage:@"OnPurchaseFailed" payload:result];
}

- (NSString*)purchaseErrorCodeToReason:(NSInteger) errorCode {
    switch (errorCode) {
        case SKErrorPaymentCancelled:
            return @"UserCancelled";
        case SKErrorPaymentInvalid:
            return @"PaymentDeclined";
        case SKErrorPaymentNotAllowed:
            return @"PurchasingUnavailable";
    }

    return @"Unknown";
}

// The transaction status of the SKPaymentQueue is sent here.
- (void)paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray *)transactions {
    UnityPurchasingLog(@"UpdatedTransactions");
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
                UnityPurchasingLog(@"PurchaseDeferred");
                [self UnitySendMessage:@"onProductPurchaseDeferred" payload:transaction.payment.productIdentifier];
                break;
            case SKPaymentTransactionStateFailed: {
                // Purchase was either cancelled by user or an error occurred.
                NSString* errorCode = [NSString stringWithFormat:@"%ld",(long)transaction.error.code];
                UnityPurchasingLog(@"PurchaseFailed: %@", errorCode);
                
                NSString* reason = [self purchaseErrorCodeToReason:transaction.error.code];
                [self onPurchaseFailed:transaction.payment.productIdentifier reason:reason];

                // Finished transactions should be removed from the payment queue.
                [[SKPaymentQueue defaultQueue] finishTransaction: transaction];
            }
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
    
    UnityPurchasingLog(@"PaymentQueueRestoreCompletedTransactionsFinished");
    [self UnitySendMessage:@"onTransactionsRestoredSuccess" payload:@""];
}

// Called if an error occurred while restoring transactions.
- (void)paymentQueue:(SKPaymentQueue *)queue restoreCompletedTransactionsFailedWithError:(NSError *)error
{
    UnityPurchasingLog(@"restoreCompletedTransactionsFailedWithError");
    // Restore was cancelled or an error occurred, so notify user.
    
    [self UnitySendMessage:@"onTransactionsRestoredFail" payload:error.localizedDescription];
}

+(ProductDefinition*) decodeProductDefinition:(NSDictionary*) hash
{
    ProductDefinition* product = [[ProductDefinition alloc] init];
    product.id = [hash objectForKey:@"id"];
    product.storeSpecificId = [hash objectForKey:@"storeSpecificId"];
    product.type = [hash objectForKey:@"type"];
    return product;
}

+ (NSArray*) deserializeProductDefs:(NSString*)json
{
    NSData* data = [json dataUsingEncoding:NSUTF8StringEncoding];
    NSArray* hashes = [NSJSONSerialization JSONObjectWithData:data options:0 error:nil];

    NSMutableArray* result = [[NSMutableArray alloc] init];
    for (NSDictionary* hash in hashes) {
        [result addObject:[self decodeProductDefinition:hash]];
    }

    return result;
}

+ (ProductDefinition*) deserializeProductDef:(NSString*)json
{
    NSData* data = [json dataUsingEncoding:NSUTF8StringEncoding];
    NSDictionary* hash = [NSJSONSerialization JSONObjectWithData:data options:0 error:nil];
    return [self decodeProductDefinition:hash];
}

+ (NSString*) serializeProductMetadata:(NSArray*)appleProducts
{
    NSMutableArray* hashes = [[NSMutableArray alloc] init];
    for (id product in appleProducts) {
        if (NULL == [product productIdentifier]) {
            UnityPurchasingLog(@"Product is missing an identifier!");
            continue;
        }
        
        NSMutableDictionary* hash = [[NSMutableDictionary alloc] init];
        [hashes addObject:hash];
        
        [hash setObject:[product productIdentifier] forKey:@"storeSpecificId"];
        
        NSMutableDictionary* metadata = [[NSMutableDictionary alloc] init];
        [hash setObject:metadata forKey:@"metadata"];
        
        if (NULL != [product price]) {
            [metadata setObject:[product price] forKey:@"localizedPrice"];
        }
        
        if (NULL != [product priceLocale]) {
            NSString *currencyCode = [[product priceLocale] objectForKey:NSLocaleCurrencyCode];
            [metadata setObject:currencyCode forKey:@"isoCurrencyCode"];
        }

        NSNumberFormatter *numberFormatter = [[NSNumberFormatter alloc] init];
        [numberFormatter setFormatterBehavior:NSNumberFormatterBehavior10_4];
        [numberFormatter setNumberStyle:NSNumberFormatterCurrencyStyle];
        [numberFormatter setLocale:[product priceLocale]];
        NSString *formattedString = [numberFormatter stringFromNumber:[product price]];
        
        if (NULL == formattedString) {
            UnityPurchasingLog(@"Unable to format a localized price");
            [metadata setObject:@"" forKey:@"localizedPriceString"];
        } else {
            [metadata setObject:formattedString forKey:@"localizedPriceString"];
        }
        if (NULL == [product localizedTitle]) {
            UnityPurchasingLog(@"No localized title for: %@. Have your products been disapproved in itunes connect?", [product productIdentifier]);
            [metadata setObject:@"" forKey:@"localizedTitle"];
        } else {
            [metadata setObject:[product localizedTitle] forKey:@"localizedTitle"];
        }
        
        if (NULL == [product localizedDescription]) {
            UnityPurchasingLog(@"No localized description for: %@. Have your products been disapproved in itunes connect?", [product productIdentifier]);
            [metadata setObject:@"" forKey:@"localizedDescription"];
        } else {
            [metadata setObject:[product localizedDescription] forKey:@"localizedDescription"];
        }
    }
    
    
    NSData *data = [NSJSONSerialization dataWithJSONObject:hashes options:0 error:nil];
    return [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
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

UnityPurchasing* UnityPurchasing_instance = NULL;

UnityPurchasing* UnityPurchasing_getInstance() {
    if (NULL == UnityPurchasing_instance) {
        UnityPurchasing_instance = [[UnityPurchasing alloc] init];
    }
    return UnityPurchasing_instance;
}

// Make a heap allocated copy of a string.
// This is suitable for passing to managed code,
// which will free the string when it is garbage collected.
// Stack allocated variables must not be returned as results
// from managed to native calls.
char* UnityPurchasingMakeHeapAllocatedStringCopy (NSString* string)
{
    if (NULL == string) {
        return NULL;
    }
    char* res = (char*)malloc([string length] + 1);
    strcpy(res, [string UTF8String]);
    return res;
}

void setUnityPurchasingCallback(UnityPurchasingCallback callback) {
    [UnityPurchasing_getInstance() setCallback:callback];
}

void unityPurchasingRetrieveProducts(const char* json) {
    NSString* str = [NSString stringWithUTF8String:json];
    NSArray* productDefs = [UnityPurchasing deserializeProductDefs:str];
    NSMutableSet* productIds = [[NSMutableSet alloc] init];
    for (ProductDefinition* product in productDefs) {
        [productIds addObject:product.storeSpecificId];
    }
    [UnityPurchasing_getInstance() requestProducts:productIds];
}

void unityPurchasingPurchase(const char* json, const char* developerPayload) {
    NSString* str = [NSString stringWithUTF8String:json];
    ProductDefinition* product = [UnityPurchasing deserializeProductDef:str];
    [UnityPurchasing_getInstance() purchaseProduct:product];
}

void unityPurchasingFinishTransaction(const char* productJSON, const char* transactionId) {
    NSString* tranId = [NSString stringWithUTF8String:transactionId];
    [UnityPurchasing_getInstance() finishTransaction:tranId];
}

void unityPurchasingRestoreTransactions() {
    UnityPurchasingLog(@"restoreTransactions");
    [UnityPurchasing_getInstance() restorePurchases];
}

void unityPurchasingAddTransactionObserver() {
    UnityPurchasingLog(@"addTransactionObserver");
    [UnityPurchasing_getInstance() addTransactionObserver];
}

void unityPurchasingRefreshAppReceipt() {
    UnityPurchasingLog(@"refreshAppReceipt");
    [UnityPurchasing_getInstance() refreshReceipt];
}

char* getUnityPurchasingAppReceipt () {
    NSString* receipt = [UnityPurchasing_getInstance() getAppReceipt];
    return UnityPurchasingMakeHeapAllocatedStringCopy(receipt);
}
