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
	#cp "${UNITY_ANDROID_JAR}" $2/libs/unity-classes.jar
fi

# Create required folders, if needed
mkdir -p $2/res

PLUGINS_PATH="$2/.."
TMP_DIR="$PLUGINS_PATH/.build_tmp"

mkdir -p $TMP_DIR

# Remove all .meta files in Plugins/Android (for project references)
#find $2/.. -name "*.meta" -type f -delete
#l=`find $2/.. -name "[!.]*.meta" -type f`; for p in $l ; do file=`echo $p | rev | cut -d "/" -f 1 | rev`; path=${p%$file}; dest="$path.$file"; echo "mv $p $dest"; mv $p $dest; done;
l=`find $PLUGINS_PATH -name "[!.]*.meta" -type f`; for p in $l ; do file=`echo $p | rev | cut -d "/" -f 1 | rev`; path=${p%$file}; mkdir -p "$TMP_DIR/$path"; dest="$TMP_DIR/$path.$file"; mv $p $dest; done;


# Update and build
android update lib-project -p $2
ant clean -buildfile $2/build.xml
ant release -buildfile $2/build.xml

#if [ ! -z "$COPY_UNITY_LIB" ]
#then
#    rm -rf $2/libs/unity-classes.jar
#fi

#l=`find $PLUGINS_PATH -name ".?*.meta" -type f`; for p in $l ; do file=`echo $p | rev | cut -d "/" -f 1 | rev`; path=${p%$file}; punto="."; file2=${file#"."}; dest="$path$file2"; echo "mv $p $dest"; mv $p $dest; done;
l=`find $PLUGINS_PATH -name ".?*.meta" -type f`; for p in $l ; do file=`echo $p | rev | cut -d "/" -f 1 | rev`; path=${p%$file}; path=${path#$TMP_DIR}; mkdir -p "$TMP_DIR/$path"; file=${file#"."};  dest="$path$file"; mv $p $dest; done;
rm -r $TMP_DIR