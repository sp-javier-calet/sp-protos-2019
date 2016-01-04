#!/bin/bash

# Copy unity and common dependencies as a project lib during compilation.
# Only works from Plugins/Android root, called from /sp_unity_project/build.xml, 
# using -pre-build and -post-build targets:
#   <target name="-pre-build">
#       <exec executable="../build_plugin_copy_dependencies.sh" failonerror="true">
#           <arg value="copy"/>
#       </exec>
#   </target>
#   <target name="-post-build">
#       <exec executable="../build_plugin_copy_dependencies.sh" failonerror="true">
#           <arg value="clean"/>
#       </exec>
#   </target>

if [ -z "$1" ]; then
    echo "Required [copy|clean] option"
    exit 1
fi

if [ $1 = "copy" ]; then
     # Retrieve Unity classes
    UNITY_DIR=`find /Applications -type d -iname "Unity*" -maxdepth 1 | sort | tail -1`
    UNITY_ANDROID_DIR="${UNITY_DIR}/Unity.app/Contents/PlaybackEngines/AndroidPlayer/"
    UNITY_ANDROID_JAR=`find "${UNITY_ANDROID_DIR}" -iname classes.jar | grep il2cpp | head -1`
    echo "Copying Unity jar from ${UNITY_ANDROID_JAR}..."
    mkdir -p ./libs
    cp "${UNITY_ANDROID_JAR}" libs/unity-classes.jar

    # Copy game libraries in Plugins/Android
    cp ../*.jar libs/
fi 

if [ $1 = "clean" ]; then
    # Remove unity classes .jar
    rm libs/unity-classes.jar

    # Remove other .jar files moved to libs
    l=`ls ../*.jar`; for p in $l ; do file=`echo $p | rev | cut -d "/" -f 1 | rev`; rm "libs/$file"; done;
fi 