#!/bin/sh

echo "Compiling $1 Android Project - $2"

# Enables exit on error
set -e


if [ ! -z "$COPY_UNITY_LIB" ]
then
	UNITY_DIR=`find /Applications -type d -iname "Unity*" -maxdepth 1 | sort | tail -1`
	UNITY_ANDROID_DIR="${UNITY_DIR}/Unity.app/Contents/PlaybackEngines/AndroidPlayer/"
	UNITY_ANDROID_JAR=`find "${UNITY_ANDROID_DIR}" -iname classes.jar | grep il2cpp | head -1`
	echo "Copying Unity jar from ${UNITY_ANDROID_JAR}..."
	mkdir -p $2/libs
	cp "${UNITY_ANDROID_JAR}" $2/libs/unity-classes.jar
fi

mkdir -p $2/res

# Remove all .meta files
find $2 -name "*.meta" -type f -delete

# Update and build
android update lib-project -p $2
ant clean -buildfile $2/build.xml
ant release -buildfile $2/build.xml

if [ ! -z "$COPY_UNITY_LIB" ]
then
	rm -rf $2/libs/unity-classes.jar
fi
