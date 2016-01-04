#!/bin/sh
echo "Compiling $1 Android Project - $2"

# Enables exit on error
set -e

# Base paths
PLUGINS_PATH="$2/.."
TMP_DIR="$PLUGINS_PATH/.build_tmp"

# Create Android build required folders, if needed
echo "Creating required folders for project - $2"
mkdir -p $2/res

# Move .meta files temporary to work properly with unity and referenced projects
echo "Removing .meta files"
mkdir -p $TMP_DIR
l=`find $PLUGINS_PATH -name "[!.]*.meta" -type f`; for p in $l ; do file=`echo $p | rev | cut -d "/" -f 1 | rev`; path=${p%$file}; mkdir -p "$TMP_DIR/$path"; dest="$TMP_DIR/$path.$file"; mv $p $dest; done;

# Update and build
android update lib-project -p $2
ant clean -buildfile $2/build.xml
ant release -buildfile $2/build.xml

# Restore .meta files
echo "Restoring .meta files"
l=`find $PLUGINS_PATH -name ".?*.meta" -type f`; for p in $l ; do file=`echo $p | rev | cut -d "/" -f 1 | rev`; path=${p%$file}; path=${path#$TMP_DIR}; mkdir -p "$TMP_DIR/$path"; file=${file#"."};  dest="$path$file"; mv $p $dest; done;
rm -r $TMP_DIR

echo "Completed successfully"
