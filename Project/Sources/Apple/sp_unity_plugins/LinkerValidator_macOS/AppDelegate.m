//
//  AppDelegate.m
//  LinkerValidator_macOS
//
//  Created by Miguel Ibero on 18/2/16.
//
//

#import "AppDelegate.h"

void SPUnityCurlInit();

@interface AppDelegate ()

@property (weak) IBOutlet NSWindow *window;
@end

@implementation AppDelegate

- (void)applicationDidFinishLaunching:(NSNotification *)aNotification {
    // Insert code here to initialize your application
    
    SPUnityCurlInit();
}

- (void)applicationWillTerminate:(NSNotification *)aNotification {
    // Insert code here to tear down your application
}

@end
