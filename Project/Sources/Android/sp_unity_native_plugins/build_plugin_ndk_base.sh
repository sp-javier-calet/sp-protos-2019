#!/bin/sh

echo "Cleaning up"
rm -rf libs
rm -rf obj

cd $2/jni
echo "Compiling $1 Android Native plugin - $2"
ndk-build NDK_APPLICATION_MK=Application.mk

cd ..

echo "Cleaning up"
rm -rf obj

echo "Installing Unity Plugin"
cp -R libs ../../../../Assets/Libraries/Binaries/Plugins/Android/

echo ""
echo "Done!"
