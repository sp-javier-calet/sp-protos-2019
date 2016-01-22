#!/bin/sh

echo "Cleaning up"
rm -rf libs
rm -rf obj

cd $2/jni
echo "Compiling $1 Android Native plugin - $2"
ndk-build NDK_APPLICATION_MK=Application.mk

cd ..

echo "Installing Unity Plugin"
mkdir -p libs
pushd obj/local > /dev/null
find . -iname "*.a" -exec rsync -R {} ../../libs \;
popd > /dev/null
cp -R libs ../../../../Assets/Libraries/Binaries/Plugins/Android/


echo "Cleaning up"
rm -rf obj

echo ""
echo "Done!"
