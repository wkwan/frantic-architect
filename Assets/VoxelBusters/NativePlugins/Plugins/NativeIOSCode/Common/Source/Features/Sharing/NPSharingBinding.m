//
//  NPSharingBinding.m
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 13/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import "NPSharingBinding.h"
#import "NPSharingHandler.h"

#define kMailShareFinished 		"MailShareFinished"
#define kMessageShareFinished 	"MessagingShareFinished"
#define kWhatsAppShareFinished 	"WhatsAppShareFinished"

#pragma mark - Mail Share

bool canSendMail ()
{
    return [[NPSharingHandler MailShare] canSendMail];
}

void sendMail (const char* subject, 					const char* body,
			   bool isHTMLBody,							UInt8* attachmentByteArray,
			   int byteArrayLength, 					const char* mimeType,
			   const char* attachmentFileNameWithExtn, 	const char* recipients)
{
	[[NPSharingHandler MailShare] sendMailWithSubject:ConvertToNSString(subject)
											  andBody:ConvertToNSString(body)
											   isHtml:isHTMLBody
										 toRecipients:ConvertToNSArray(recipients)
								  alongWithAttachment:[Utility CreateNSDataFromByteArray:attachmentByteArray ofLength:byteArrayLength]
											   ofType:ConvertToNSString(mimeType)
									 havingFileNameAs:ConvertToNSString(attachmentFileNameWithExtn)
										   completion:^(MFMailComposeResult result) {
											 // Notify unity
											 NSString *resultStr	= [NSString stringWithFormat:@"%d", result];
											 NotifyEventListener(kMailShareFinished, [resultStr UTF8String]);
										 }];
}

#pragma mark - Messaging Share

bool isMessagingAvailable ()
{
    return [[NPSharingHandler MessagingShare] isMessagingAvailable];
}

void sendTextMessage (const char* body, const char* recipients)
{
    [[NPSharingHandler MessagingShare] sendMessageWithBody:ConvertToNSString(body)
											  toRecipients:ConvertToNSArray(recipients) completion:^(MessageComposeResult result) {
												// Notify unity
												NSString *resultStr	= [NSString stringWithFormat:@"%d", result];
												NotifyEventListener(kMessageShareFinished, [resultStr UTF8String]);
											}];
}

#pragma mark - WhatsApp share

bool canShareOnWhatsApp ()
{
	BOOL _canShareText	= [[NPSharingHandler WhatsAppShare] canShareTextMessage];
	BOOL _canShareImage	= [[NPSharingHandler WhatsAppShare] canShareImage];

	return (_canShareText && _canShareImage);
}

void shareTextMessageOnWhatsApp (const char* message)
{
	[[NPSharingHandler WhatsAppShare] shareTextMessage:ConvertToNSString(message)
											completion:^(bool completed) {
											  // Notify unity
											  NSString *completedStr	= [NSString stringWithFormat:@"%d", completed];
											  NotifyEventListener(kWhatsAppShareFinished, [completedStr UTF8String]);
										  }];
}

void shareImageOnWhatsApp (UInt8* imageByteArray, int byteArrayLength)
{
	[[NPSharingHandler WhatsAppShare] shareImage:[Utility CreateImageFromByteArray:imageByteArray ofLength:byteArrayLength]
									  completion:^(bool completed) {
										// Notify unity
										NSString *completedStr	= [NSString stringWithFormat:@"%d", completed];
										NotifyEventListener(kWhatsAppShareFinished, [completedStr UTF8String]);
									}];
}

#pragma mark - Share

void share (const char* message,    	const char* URLString,
            UInt8* imageByteArray,  	int byteArrayLength,
            const char *excludedOptions)
{
    [[NPSharingHandler Instance] shareMessage:ConvertToNSString(message)
										  URL:ConvertToNSString(URLString)
									 andImage:[Utility CreateImageFromByteArray:(void*)imageByteArray ofLength:byteArrayLength]
						  withExcludedSharing:ConvertToNSArray(excludedOptions)];
}
