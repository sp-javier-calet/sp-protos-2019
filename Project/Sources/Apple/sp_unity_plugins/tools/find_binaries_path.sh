#! /bin/sh

PROJECT_PATH=$(git rev-parse --show-toplevel)
ASSETS_DIR_PATH=$(find $PROJECT_PATH -name 'Assets' -maxdepth 2 -type d) #else Builds folder content will be found instead.
SEARCH_PATTERN="*Sparta/Binaries"
INSTALL_PATH=$(find $ASSETS_DIR_PATH -type d -wholename $SEARCH_PATTERN)
SPARTA_BINARIES_DIR=$(cd "$INSTALL_PATH" && pwd)

PLATFORM=$1

SRC=$PROJECT_DIR/build/$PLATFORM/.
DST=$SPARTA_BINARIES_DIR/$PLATFORM/

echo $SRC
echo $DST

cp -R $SRC $DST
