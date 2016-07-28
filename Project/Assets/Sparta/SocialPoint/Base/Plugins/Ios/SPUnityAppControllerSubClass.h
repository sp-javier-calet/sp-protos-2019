#import "UnityAppController.h"

@interface SPUnityAppControllerSubClass : UnityAppController

struct ForceTouchShortcutItem
{
    const char* Type;
    const char* Title;
    const char* Subtitle;
    const char* IconPath;
};

+ (void)load;

@end