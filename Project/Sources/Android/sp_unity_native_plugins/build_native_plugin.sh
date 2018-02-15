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
    echo "\n *** ERROR BUILDING ANDROID NDK PLUGIN *** \n\n\n"
    exit 1
}


echo "\n\n *** BUILD ANDROID NDK PLUGIN ***  \n"

clean $MODULE_PATH

cd $MODULE_NAME/jni 2>&1
echo "Compiling '$MODULE_NAME' Android Native plugin"
$NDK_PATH/ndk-build NDK_APPLICATION_MK=Application.mk 2>&1
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

SRC_DIR="$MODULE_PATH/libs"

mkdir -p $SRC_DIR

PROJECT_PATH=$(git rev-parse --show-toplevel)
echo "Project Path:" $PROJECT_PATH

SEARCH_PATTERN="*Sparta/Binaries/Android"
INSTALL_PATH=$(find $PROJECT_PATH -type d -wholename $SEARCH_PATTERN)
DST_DIR=$(cd "$INSTALL_PATH" && pwd)

echo "Source dir:" $SRC_DIR
echo "Destiny dir:" $DST_DIR

mkdir -p $DST_DIR

cp -R $SRC_DIR $DST_DIR
INSTALL_SUCCESS=$?

# Check success code
if [ $INSTALL_SUCCESS != 0 ] ; then
    echo "\nINSTALL FAILED\n"
    abort
else
    echo "Plugin installed in $DST_DIR"
    echo "\nINSTALL SUCCESS\n"
fi

clean $MODULE_PATH

echo "*** '$MODULE_NAME' Successfully compiled and installed *** "

