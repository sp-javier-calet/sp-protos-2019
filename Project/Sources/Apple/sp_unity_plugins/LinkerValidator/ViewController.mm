//
//  ViewController.m
//  LinkerValidator
//
//  Created by Ivan Hernández on 7/1/16.
//
//

#import "ViewController.h"
#include "CurlTestClient.hpp"
#include "LibWebsocketTestClient.hpp"

@interface ViewController ()

@end

@implementation ViewController

- (void)viewDidLoad
{
    [super viewDidLoad];
    // Do any additional setup after loading the view, typically from a nib.

    dispatch_async(dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_DEFAULT, 0), ^{
      LibWebsocketTestClient testWebsocket;
      testWebsocket.run();

      CurlTestClient test;
      test.run();

    });
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

@end
