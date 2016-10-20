//
//  ViewController.m
//  LinkerValidator
//
//  Created by Ivan Hernández on 7/1/16.
//
//

#import "ViewController.h"
#include "CurlTestClient.hpp"

@interface ViewController ()

@end

@implementation ViewController

- (void)viewDidLoad
{
    [super viewDidLoad];
    // Do any additional setup after loading the view, typically from a nib.

    CurlTestClient test;
    test.run();
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

@end
