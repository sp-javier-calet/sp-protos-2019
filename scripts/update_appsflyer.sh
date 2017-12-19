#!/bin/bash

SCRIPT_PATH="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
PROJ_PATH="Project/Assets"
PLUGINS_PATH="${PROJ_PATH}/Plugins"
SPARTA_PATH="${PROJ_PATH}/Sparta/External/AppsFlyer"

rm -rf ${PLUGINS_PATH}/Android/support-*
rm -rf ${PLUGINS_PATH}/Android/animated-vector-drawable-*
rm -rf ${PLUGINS_PATH}/Android/appcompat-*
rm -rf ${PLUGINS_PATH}/Android/cardview-*
rm -rf ${PLUGINS_PATH}/Android/customtabs-*

UNITY_FILES="AppsFlyer.cs AppsFlyerTrackerCallbacks.cs AFInAppEvents.cs"
ANDROID_FILES="AF-Android-SDK.jar AppsFlyerAndroidPlugin.jar"
IOS_FILES="AppsFlyerAppController.mm AppsFlyerDelegate.h AppsFlyerDelegate.mm AppsFlyerTracker.h AppsFlyerWrapper.h AppsFlyerWrapper.mm libAppsFlyerLib.a"

for FILE in $UNITY_FILES ; do
	FILE_PATH="${PLUGINS_PATH}/${FILE}"
	if [ -f $FILE_PATH ]; then
		mv "${FILE_PATH}" "${SPARTA_PATH}"
	fi
done

for FILE in $ANDROID_FILES ; do
	FILE_PATH="${PLUGINS_PATH}/Android/${FILE}"
	if [ -f $FILE_PATH ]; then
		mv "${FILE_PATH}" "${SPARTA_PATH}/Plugins/Android"
	fi
done

for FILE in $IOS_FILES ; do
	FILE_PATH="${PLUGINS_PATH}/iOS/${FILE}"
	if [ -f $FILE_PATH ]; then
		mv "${FILE_PATH}" "${SPARTA_PATH}/Plugins/iOS"
	fi
done

patch --verbose -p1 < "${SCRIPT_PATH}/appsflyer.patch"
