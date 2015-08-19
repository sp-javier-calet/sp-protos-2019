#!/bin/sh

echo "Compiling $1 Android Project - $2"

# Enables exit on error
set -e

mkdir -p $2/res

# Remove all .meta files
find $2 -name "*.meta" -type f -delete

# Update and build
android update lib-project -p $2
ant clean -buildfile $2/build.xml
ant release -buildfile $2/build.xml

