#!/bin/sh

LOCAL_PATH=`basename $0`
echo $LOCAL_PATH
MODULE_NAME=$1
MODULE_PATH=$2

echo "Cleaning up"
rm -rf $MODULE_PATH/libs
rm -rf $MODULE_PATH/obj

cd $MODULE_PATH/jni
echo "Compiling $MODULE_NAME Android Native plugin"
ndk-build NDK_APPLICATION_MK=Application.mk

echo "Installing Unity Plugin"
mkdir -p $MODULE_PATH/libs
cp -R $MODULE_PATH/libs $LOCAL_PATH/../../../Assets/Sparta/Binaries/Plugins/Android/


echo "Cleaning up"
rm -rf $MODULE_PATH/obj

echo ""
echo "Done!"
