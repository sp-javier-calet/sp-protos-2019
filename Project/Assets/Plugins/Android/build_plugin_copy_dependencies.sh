#!/bin/bash

# Retrieve Unity classes
UNITY_DIR=`find /Applications -type d -iname "Unity*" -maxdepth 1 | sort | tail -1`
UNITY_ANDROID_DIR="${UNITY_DIR}/Unity.app/Contents/PlaybackEngines/AndroidPlayer/"
UNITY_ANDROID_JAR=`find "${UNITY_ANDROID_DIR}" -iname classes.jar | grep il2cpp | head -1`
echo "Copying Unity jar from ${UNITY_ANDROID_JAR}..."
mkdir -p ./libs
cp "${UNITY_ANDROID_JAR}" libs/unity-classes.jar

# Copy game libraries in Plugins/Android
cp ../*.jar libs/
