//
//  ViewController.m
//  LinkerValidator_tvOs
//
//  Created by Ivan Hernández on 7/1/16.
//
//

#import "ViewController.h"

void SPUnityCurlInit();

@interface ViewController ()

@end

@implementation ViewController

- (void)viewDidLoad {
  [super viewDidLoad];
  // Do any additional setup after loading the view, typically from a nib.

  SPUnityCurlInit();
}

- (void)didReceiveMemoryWarning {
  [super didReceiveMemoryWarning];
  // Dispose of any resources that can be recreated.
}

@end
