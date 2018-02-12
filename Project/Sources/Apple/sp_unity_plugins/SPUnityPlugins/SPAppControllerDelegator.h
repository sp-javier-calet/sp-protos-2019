#ifndef __sparta__SPAppControllerDelegator__
#define __sparta__SPAppControllerDelegator__

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "SPAppControllerDelegate.h"

@interface SPAppControllerDelegator : NSObject<SPAppControllerDelegate>

- (void)addAllDelegates;
- (BOOL)addDelegate:(id<SPAppControllerDelegate>)delegate;
- (BOOL)removeDelegate:(id<SPAppControllerDelegate>)delegate;

@end

#endif
