#!/bin/sh

LOCAL_PATH=$( pwd )

MODULE_NAME=$1
MODULE_PATH=$LOCAL_PATH/$MODULE_NAME

echo "Compiling module in $MODULE_PATH"

if [ ! -z "$2" ]; then
    NDK_PATH=$2
fi

clean() {
    echo "Cleaning up\n"
    rm -rf $1/libs
    rm -rf $1/obj
}

abort()
{
    echo "\n *** ERROR BUILDING LINUX NDK PLUGIN *** \n\n\n"
    exit 1
}


echo "\n\n *** BUILD LINUX NDK PLUGIN ***  \n"

clean $MODULE_PATH

cd $MODULE_NAME/jni 2>&1
echo "Compiling '$MODULE_NAME' Linux Native plugin"
$NDK_PATH/ndk-build V=1 NDK_APPLICATION_MK=Application.mk 2>&1
BUILD_SUCCESS=$?
cd ..

# Check success code
if [ $BUILD_SUCCESS != 0 ] ; then
    echo "\nCOMPILATION FAILED\n"
    abort
else
    echo "\nCOMPILATION SUCCESS\n"
fi

echo "Installing Unity Plugin..."
mkdir -p $MODULE_PATH/libs

PROJECT_ROOT=$MODULE_PATH/../../../..
INSTALL_PATH=/Assets/Sparta/Binaries/Plugins/Linux/

mkdir -p $PROJECT_ROOT$INSTALL_PATH

cp -R $MODULE_PATH/libs $PROJECT_ROOT$INSTALL_PATH
INSTALL_SUCCESS=$?

# Check success code
if [ $INSTALL_SUCCESS != 0 ] ; then
    echo "\nINSTALL FAILED\n"
    abort
else
    echo "Plugin installed in $INSTALL_PATH"
    echo "\nINSTALL SUCCESS\n"
fi

clean $MODULE_PATH

echo "*** '$MODULE_NAME' Successfully compiled and installed *** "
