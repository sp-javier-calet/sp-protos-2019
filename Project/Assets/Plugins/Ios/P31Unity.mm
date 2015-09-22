//
//  P31Unity.m
//  P31SharedTools
//
//  Created by Mike Desaro on 9/12/13.
//  Copyright (c) 2013 prime31. All rights reserved.
//

#import "P31Unity.h"



#ifdef __cplusplus
extern "C" {
#endif
	void UnitySendMessage( const char * className, const char * methodName, const char * param );
#ifdef __cplusplus
}
#endif

#if UNITY_VERSION < 500
void UnityPause( bool pause );
#else
void UnityPause( int pause );
#endif


@implementation P31Unity

+ (void)unityPause:(NSNumber*)shouldPause
{
	BOOL shouldPauseBool = [shouldPause boolValue];
	UnityPause( shouldPauseBool );
}

@end
