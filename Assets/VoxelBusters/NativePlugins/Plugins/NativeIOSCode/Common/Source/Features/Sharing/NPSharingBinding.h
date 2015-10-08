//
//  NPSharingBinding.h
//  NativePluginIOSWorkspace
//
//  Created by Ashwin kumar on 13/01/15.
//  Copyright (c) 2015 Voxel Busters Interactive LLP. All rights reserved.
//

#import <Foundation/Foundation.h>

// Mail share
UIKIT_EXTERN bool canSendMail ();
UIKIT_EXTERN void sendMail (const char* subject, 					const char* body,
							bool isHTMLBody,						UInt8* attachmentByteArray,
							int byteArrayLength, 					const char* mimeType,
							const char* attachmentFileNameWithExtn, const char* recipients);

// Messaging share
UIKIT_EXTERN bool isMessagingAvailable ();
UIKIT_EXTERN void sendTextMessage (const char* body, const char* recipients);

// WhatsApp share
UIKIT_EXTERN bool canShareOnWhatsApp ();
UIKIT_EXTERN void shareTextMessageOnWhatsApp (const char* message);
UIKIT_EXTERN void shareImageOnWhatsApp (UInt8* imageByteArray, int byteArrayLength);

// Share
UIKIT_EXTERN void share (const char* message,    const char* URLString,
						 UInt8* imageByteArray,  int byteArrayLength,
						 const char *excludedOptions);