#!/bin/sh

cd $2/jni
echo "Compiling $1 Android Native plugin - $2"
ndk-build NDK_APPLICATION_MK=Application.mk

cd ..
echo "Extracting generated libraries"
rsync -av libs ..

echo "Cleaning up"
rm -rf libs
rm -rf obj

echo ""
echo "Done!"