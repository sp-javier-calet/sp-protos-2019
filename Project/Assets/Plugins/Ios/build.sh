#!/bin/bash

CURL_VERSION="7.44.0"
OPENSSL_VERSION="1.0.2d"
LIBZ_VERSION="1.2.8"

CURL_URL="http://curl.haxx.se/download/curl-${CURL_VERSION}.tar.gz"
OPENSSL_URL="https://www.openssl.org/source/openssl-${OPENSSL_VERSION}.tar.gz"
LIBZ_URL="http://zlib.net/zlib-${LIBZ_VERSION}.tar.gz"

XCODE_HOME=`xcode-select --print-path`
SCRIPT_PATH=$(cd `dirname "${BASH_SOURCE[0]}"` && pwd)
OUT_PATH="${SCRIPT_PATH}"
LIBS_PATH="${OUT_PATH}/libs"
BUILD_DIR="${TMPDIR}sp_unity_curl_build"

function prepare_lib {
    NAME="${1}"
    URL="${2}"
    DIR_REGEX="${3}"
    if [ -z "${DIR_REGEX}" ]
    then
        DIR_REGEX="${NAME}*"
    fi

    FILE="${BUILD_DIR}/${NAME}.tar.gz"
    DIR="${BUILD_DIR}/${NAME}"
    if [ ! -d "${DIR}" ] 
    then
        if [ ! -f "${FILE}" ] 
        then
            echo "downloading ${NAME}..."
            curl  -o "${FILE}" "${URL}"
        fi

        if [ -f "${FILE}" ] 
        then
            echo "extracting ${NAME}..."
            tar xzvf "${FILE}" -C "${BUILD_DIR}" > /dev/null 2>&1
        fi
    fi

    if [ ! -d "${DIR}" ] 
    then
        TEMP_DIR=`find "${BUILD_DIR}" -type d -iname "${DIR_REGEX}" |head -n1`
        if [ ! -z "${TEMP_DIR}" ]
        then
            mv "${TEMP_DIR}" "${DIR}"
        fi
    fi
}

function setup_lib {
    ARCH="${1}"
    PLATFORM="${2}"

    export CC=`xcrun -find -sdk ${PLATFORM} gcc`
    export LD=`xcrun -find -sdk ${PLATFORM} ld`
    export AR=`xcrun -find -sdk ${PLATFORM} ar`
    export AS=`xcrun -find -sdk ${PLATFORM} as`
    export NM=`xcrun -find -sdk ${PLATFORM} nm`
    export RANLIB=`xcrun -find -sdk ${PLATFORM} ranlib`

    if [[ "${ARCH}" == "arm64" ]] || [[ "${ARCH}" == "x86_64" ]]
    then
        BITS=64
    else
        BITS=32
    fi

    SDK_ROOT=`xcrun --sdk ${PLATFORM} --show-sdk-path`
    FLAGS="-arch "${ARCH}" -pipe -isysroot "${SDK_ROOT}" -m${BITS} -miphoneos-version-min=4.3"
    export CFLAGS="${FLAGS}"
    export LDFLAGS="${FLAGS}"

    mkdir -p "${LIBS_PATH}/${ARCH}"
}

function make_lib {
    ARGS="${1}"
    if [ $? -ne 0 ]
    then
        popd > /dev/null
        exit $?
    fi

    echo "${ARGS}"
    make clean
    make ${ARGS} CC="${CC}" CFLAG="${CFLAGS}"

    if [ $? -ne 0 ]
    then
        popd > /dev/null
        exit $?
    fi
}

function combine_lib {
    LIB="${1}"
    LIBS="${2}"
    lipo -create ${LIBS} -o "${LIB}"
    if [ $? -ne 0 ]
    then
        exit $?
    fi
}

function build_libz {
    ARCH="${1}"
    PLATFORM="${2}"
    LIB="${LIBS_PATH}/${ARCH}/libz.a"

    if [ ! -f "${LIB}" ] 
    then
        echo "building libz for arch ${ARCH}..."
        pushd "${LIBZ_DIR}" > /dev/null
        setup_lib "${ARCH}" "${PLATFORM}"
        ./configure > /dev/null
        make_lib -j > /dev/null
        cp ./libz.a "${LIB}"
        popd > /dev/null
    fi
}

function build_openssl {
    ARCH="${1}"
    PLATFORM="${2}"
    CRYPTO_LIB="${LIBS_PATH}/${ARCH}/libcrypto.a"
    SSL_LIB="${LIBS_PATH}/${ARCH}/libssl.a"

    if [ ! -f "${CRYPTO_LIB}" ] || [ ! -f "${SSL_LIB}" ]
    then
        echo "building openssl for arch ${ARCH}..."
        pushd "${OPENSSL_DIR}" > /dev/null
        setup_lib "${ARCH}" "${PLATFORM}"
        export CPPFLAGS="${CPPFLAGS} -I${LIBZ_DIR}"
        export LDFLAGS="${LDFLAGS}  -L${OUT_PATH}"
        ./Configure "BSD-generic${BITS}" > /dev/null
        make_lib > /dev/null 2>&1
        cp ./libcrypto.a "${CRYPTO_LIB}"
        cp ./libssl.a "${SSL_LIB}"
        popd > /dev/null
    fi
}

function build_curl {
    ARCH="${1}"
    PLATFORM="${2}"
    HOST="${3}"
    LIB="${LIBS_PATH}/${ARCH}/libcurl.a"

    if [ ! -f "${LIB}" ] 
    then
        echo "building curl for arch ${ARCH}..."
        pushd "${CURL_DIR}" > /dev/null
        setup_lib "${ARCH}" "${PLATFORM}"
        export CPPFLAGS="${CPPFLAGS} -I${OPENSSL_DIR}/include -I${LIBZ_DIR}"
        export LDFLAGS="${LDFLAGS} -L${LIBS_PATH}/${ARCH}"
        ./configure --disable-shared --enable-static --enable-threaded-resolver \
            --with-zlib --with-ssl --disable-dependency-tracking \
            --disable-ldap --disable-imap --disable-gopher --disable-rtsp \
            --without-libidn --host="${HOST}-apple-darwin" > /dev/null 2>&1
        make_lib -j > /dev/null 2>&1
        cp ./lib/.libs/libcurl.a ${LIB}
        popd > /dev/null
    fi
}

echo "building curl for iOS in ${BUILD_DIR}..."
mkdir -p "${BUILD_DIR}"

COMBINED_LIBZ_LIB="${OUT_PATH}/libz.a"
prepare_lib "libz" "${LIBZ_URL}" "zlib-*"
LIBZ_DIR="${DIR}"
if [ ! -f "${COMBINED_LIBZ_LIB}" ]
then
    if [ -d "${LIBZ_DIR}" ] 
    then
        build_libz "i386" "iphonesimulator"
        LIBZ_LIBS="${LIBZ_LIBS} ${LIB}"
        build_libz "x86_64" "iphonesimulator"
        LIBZ_LIBS="${LIBZ_LIBS} ${LIB}"
    fi
fi

COMBINED_SSL_LIB="${OUT_PATH}/libssl.a"
COMBINED_CRYPTO_LIB="${OUT_PATH}/libcrypto.a"
prepare_lib "openssl" "${OPENSSL_URL}"
OPENSSL_DIR="${DIR}"
if [ ! -f "${COMBINED_SSL_LIB}" ] || [ ! -f "${COMBINED_CRYPTO_LIB}" ]
then
    if [ -d "${OPENSSL_DIR}" ] 
    then
        build_openssl "armv7" "iphoneos"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
        build_openssl "armv7s" "iphoneos"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
        build_openssl "arm64" "iphoneos"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
        build_openssl "i386" "iphonesimulator"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
        build_openssl "x86_64" "iphonesimulator"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
    fi
fi

COMBINED_CURL_LIB="${OUT_PATH}/libcurl.a"
prepare_lib "curl" "${CURL_URL}"
CURL_DIR="${DIR}"
if [ ! -f "${COMBINED_CURL_LIB}" ]
then
    if [ -d "${CURL_DIR}" ] 
    then
        build_curl "armv7" "iphoneos" "armv7"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
        build_curl "armv7s" "iphoneos" "armv7s"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
        build_curl "arm64" "iphoneos" "aarch64"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
        build_curl "i386" "iphonesimulator" "i386"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
        build_curl "x86_64" "iphonesimulator" "x86_64"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
    fi
fi

if [ ! -f "${COMBINED_LIBZ_LIB}" ]
then
    echo "combining libz into one lib..."
    combine_lib "${COMBINED_LIBZ_LIB}" "${LIBZ_LIBS}"
fi
if [ ! -f "${COMBINED_CRYPTO_LIB}" ]
then
    echo "combining crypto into one lib..."
    combine_lib "${COMBINED_CRYPTO_LIB}" "${CRYPTO_LIBS}"
fi
if [ ! -f "${COMBINED_SSL_LIB}" ]
then
    echo "combining ssl into one lib..."
    combine_lib "${COMBINED_SSL_LIB}" "${SSL_LIBS}"
fi
if [ ! -f "${COMBINED_CURL_LIB}" ]
then
    echo "combining curl into one lib..."
    combine_lib "${COMBINED_CURL_LIB}" "${CURL_LIBS}"
fi
echo "deleting arch libs..."
rm -rf ${LIBS_PATH}
echo "copying curl headers..."
rm -rf "curl"
mkdir curl
cp -r "${CURL_DIR}"/include/curl/*.h curl
