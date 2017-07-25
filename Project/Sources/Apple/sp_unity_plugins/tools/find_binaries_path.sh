#! /bin/sh

PROJECT_PATH=$(git rev-parse --show-toplevel)
SEARCH_PATTERN="*Sparta/Binaries"
INSTALL_PATH=$(find $PROJECT_PATH -type d -wholename $SEARCH_PATTERN)
SPARTA_BINARIES_DIR=$(cd "$INSTALL_PATH" && pwd)

PLATFORM=$1

SRC=$PROJECT_DIR/build/$PLATFORM/.
DST=$SPARTA_BINARIES_DIR/$PLATFORM/

echo $SRC
echo $DST

cp -R $SRC $DST
