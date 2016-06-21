#include "SPUnityAlertViewFacade.h"
#include "SPNativeCallsSender.h"
#import <UIKit/UIKit.h>

typedef void (^SPAlertViewBlock)(NSInteger buttonIndex, NSString* inputText);

@interface SPAlertView : NSObject<UIAlertViewDelegate>

@property (nonatomic, copy) SPAlertViewBlock block;
@property (nonatomic, strong) UITextField* textField;

@property (nonatomic, strong) UIAlertController* alertCtrl;
@property (nonatomic, strong) UIAlertView* alertView;

-(id)initWithTitle:(NSString*)title
           message:(NSString*)message
           signature:(NSString*)signature
           buttons:(NSArray*)buttons
             input:(BOOL)input
          callback:(SPAlertViewBlock)block;

-(void)show;
-(void)hide;

@end

@implementation SPAlertView

-(id)initWithTitle:(NSString*)title
           message:(NSString*)message
           signature:(NSString*)signature
           buttons:(NSArray*)buttons
             input:(BOOL)input
          callback:(SPAlertViewBlock)block;
{
    self = [super init];
    if(self)
    {
        
        NSString* formattedSignature = @"";
        if(signature != nil && ![signature isEqualToString:@""])
        {
            formattedSignature = [NSString stringWithFormat:@"(%@)", signature];
        }
        
        NSString* formattedMessage = [NSString stringWithFormat:@"%@ %@",message, formattedSignature];
         
        if([UIAlertController class])
        {
            self.alertCtrl = [UIAlertController alertControllerWithTitle:title
                                 message:[NSString stringWithFormat:@"%@",formattedMessage]
                         preferredStyle:UIAlertControllerStyleAlert];
            
            NSInteger buttonIndex = 0;
            for(id btnTitle in buttons)
            {
                UIAlertAction* action = [UIAlertAction actionWithTitle:btnTitle style:UIAlertActionStyleDefault
                   handler:^(UIAlertAction * action) {
                       [self clickedButtonAtIndex:buttonIndex];
                   }];
                buttonIndex++;
                [self.alertCtrl addAction:action];
            }
            
            if(input)
            {
                [self.alertCtrl addTextFieldWithConfigurationHandler:^(UITextField* textField) {
                    self.textField = textField;
                }];
            }
        }
        else
        {
            self.alertView = [[UIAlertView alloc]
                              initWithTitle:title
                              message:[NSString stringWithFormat:@"%@",formattedMessage]
                              delegate:self
                              cancelButtonTitle:nil
                              otherButtonTitles:nil];


            for(id btnTitle in buttons)
            {
                [self.alertView addButtonWithTitle:btnTitle];
            }
            
            if(input)
            {
                self.alertView.alertViewStyle = UIAlertViewStylePlainTextInput;
                self.textField = [self.alertView textFieldAtIndex:0];
            }
        }
        self.block = block;
    }
    return self;
}

-(void)show
{
    if(self.alertCtrl)
    {
        UIViewController* root = UIApplication.sharedApplication.keyWindow.rootViewController;
        [root presentViewController:self.alertCtrl animated:YES completion:nil];
    }
    else
    {
        [self.alertView show];
    }
}

-(void)hide
{
    self.block = nil;
    [self.alertCtrl removeFromParentViewController];
    [self.alertView dismissWithClickedButtonIndex:0 animated:YES];
}

-(void)clickedButtonAtIndex:(NSInteger)buttonIndex
{
    if(self.block)
    {
        self.block(buttonIndex, self.textField.text);
    }
}

#pragma mark - alert view delegate

-(void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex
{
    [self clickedButtonAtIndex:buttonIndex];
}

-(void)alertViewCancel:(UIAlertView *)alertView
{
    [self clickedButtonAtIndex:-1];
}

@end

__strong SPAlertView* s_spAlertView = nil;

EXPORT_API void SPUnityAlertViewShow(SPUnityAlertViewDataStruct data)
{
    SPUnityAlertViewHide();
    
    NSArray* buttons = [[NSString stringWithUTF8String:data.buttons]
                        componentsSeparatedByString:@"|"];
    NSString* title = [NSString stringWithUTF8String:data.title];
    NSString* message = [NSString stringWithUTF8String:data.message];
    NSString* signature = [NSString stringWithUTF8String:data.signature];
    
    
    s_spAlertView = [[SPAlertView alloc]
                                   initWithTitle:title
                                   message:message
                                   signature:signature
                                   buttons:buttons
                                   input:data.input
                                   callback: ^(NSInteger buttonIndex, NSString* inputText)
        {
            NSString* msg = [NSString stringWithFormat:@"%ld %@", (long)buttonIndex, inputText];
            s_spAlertView = nil;
            
            SPNativeCallsSender::SendMessage("ResultMessage", msg.UTF8String);
        }];

    [s_spAlertView show];

}

EXPORT_API void SPUnityAlertViewHide()
{
    if(s_spAlertView)
    {
        [s_spAlertView hide];
        s_spAlertView = nil;
    }
}
