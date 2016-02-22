#!/bin/sh

MODULE_NAME=$1
MODULE_PATH=$2

clean() {
    echo "Cleaning up\n"
    rm -rf $1/libs
    rm -rf $1/obj
}

abort()
{
    echo "\n *** ERROR BUILDING ANDROID NDK PLUGIN *** \n\n\n"
    exit 1
}

# Redirect error output

echo "\n\n *** BUILD ANDROID NDK PLUGIN ***  \n"

clean $MODULE_PATH

cd $MODULE_PATH/jni
echo "Compiling '$MODULE_NAME' Android Native plugin"
OUTPUT="$( ndk-build NDK_APPLICATION_MK=Application.mk 2>&1)"
BUILD_SUCCESS=$?
echo "$OUTPUT\n";
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
INSTALL_PATH=/Assets/Sparta/Binaries/Plugins/Android/

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

