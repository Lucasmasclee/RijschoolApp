#import <Foundation/Foundation.h>

extern "C" {
    const char* _GetStoredCodeIOS() {
        NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
        NSString* code = [defaults stringForKey:@"rijschoolAppCode"];
        return [code UTF8String];
    }
} 