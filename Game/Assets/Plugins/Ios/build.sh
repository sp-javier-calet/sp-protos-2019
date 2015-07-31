#!/bin/bash
SDK_VERSION="8.1"
XCODE_HOME=`xcode-select --print-path`
CPUS=`sysctl -n hw.logicalcpu_max`
SCRIPT_PATH=$(cd `dirname "${BASH_SOURCE[0]}"` && pwd)
OUT_PATH="${SCRIPT_PATH}"
LIB_BASE_DIR="${SCRIPT_PATH}/../../.."
OPENSSL_BASE_DIR="${LIB_BASE_DIR}/openssl"
LIBZ_BASE_DIR="${LIB_BASE_DIR}/libz"

LIBS=""

function build {
    ARCH="${1}"
    PLATFORM="${2}"
    HOST="${3}"

    PLATFORM_LOWER=`echo "${PLATFORM}" | tr '[:upper:]' '[:lower:]'`
    export CC=`xcrun -find -sdk ${PLATFORM_LOWER} gcc`
    export LD=`xcrun -find -sdk ${PLATFORM_LOWER} ld`
    export AR=`xcrun -find -sdk ${PLATFORM_LOWER} ar`
    export AS=`xcrun -find -sdk ${PLATFORM_LOWER} as`
    export NM=`xcrun -find -sdk ${PLATFORM_LOWER} nm`
    export RANLIB=`xcrun -find -sdk ${PLATFORM_LOWER} ranlib`

    SDK_ROOT="${XCODE_HOME}/Platforms/${PLATFORM}.platform/Developer/SDKs/${PLATFORM}${SDK_VERSION}.sdk"
    FLAGS="-arch ${ARCH} -pipe -isysroot ${SDK_ROOT}"
    export CFLAGS="${FLAGS} -miphoneos-version-min=${SDK_VERSION}"
    export CPPFLAGS="-I${OPENSSL_BASE_DIR}/include"
    export LDFLAGS="${FLAGS} -L${OPENSSL_BASE_DIR}/libs/ios"

    if [[ "$PLATFORM_LOWER" == "iphonesimulator" ]]
    then
        export CPPFLAGS="${CPPFLAGS} -I${LIBZ_BASE_DIR}/include"
        export LDFLAGS="${LDFLAGS} -L${LIBZ_BASE_DIR}/lib/ios"
    fi

    ./configure --disable-shared --enable-static --with-zlib --with-ssl --disable-dependency-tracking --disable-ldap --disable-imap --disable-gopher --disable-rtsp --without-libidn --host="${HOST}-apple-darwin"

    if [ $? -ne 0 ]
    then
      return $?
    fi

    make clean
    make -j ${CPUS}

    if [ $? -ne 0 ]
    then
      return $?
    fi

    LIB="${OUT_PATH}/libcurl-${ARCH}.a"
    cp lib/.libs/libcurl.a ${LIB}
    LIBS="${LIBS} ${LIB}"
}

build "armv7" "iPhoneOS" "armv7"
build "armv7s" "iPhoneOS" "armv7s"
build "arm64" "iPhoneOS" "aarch64"
build "i386" "iPhoneSimulator" "i386"
build "x86_64" "iPhoneSimulator" "x86_64"

lipo -create ${LIBS} -o ${OUT_PATH}/libcurl.a
rm ${LIBS}
