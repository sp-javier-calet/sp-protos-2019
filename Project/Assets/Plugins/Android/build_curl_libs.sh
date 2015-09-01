#!/bin/bash

CURL_VERSION="7.44.0"
OPENSSL_VERSION="1.0.2d"
LIBZ_VERSION="1.2.8"
ANDROID_PLATFORM="9"
TOOLCHAIN_VERSION="4.9"

CURL_URL="http://curl.haxx.se/download/curl-${CURL_VERSION}.tar.gz"
OPENSSL_URL="https://www.openssl.org/source/openssl-${OPENSSL_VERSION}.tar.gz"
LIBZ_URL="http://zlib.net/zlib-${LIBZ_VERSION}.tar.gz"

MAKE_TOOLCHAIN_PATH="${NDK_ROOT}/build/tools/make-standalone-toolchain.sh"
SCRIPT_PATH=$(cd `dirname "${BASH_SOURCE[0]}"` && pwd)
BUILD_DIR="${TMPDIR}sp_unity_curl_build"
LIBS_PATH="${SCRIPT_PATH}/sp_unity_curl/lib"

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
    TOOLCHAIN=""
    TOOLCHAIN_PREFIX=""
    if [[ "${ARCH}" == "armeabi" ]] || [[ "${ARCH}" == "armeabi-v7a" ]]
    then
        TOOLCHAIN="arm-linux-androideabi"
    fi
    if [[ "${ARCH}" == "aarch64" ]]
    then
        TOOLCHAIN="aarch64-linux-android"
    fi
    if [[ -z "${TOOLCHAIN}" ]]
    then
        TOOLCHAIN="${ARCH}"
    fi
    if [[ "${TOOLCHAIN}" == "x86" ]]
    then
        TOOLCHAIN_PREFIX="i686-linux-android"
    else
        TOOLCHAIN_PREFIX="${TOOLCHAIN}"
    fi

    TOOLCHAIN_PATH="${BUILD_DIR}/android-toolchain-${TOOLCHAIN}"
    if [ ! -d "${TOOLCHAIN_PATH}" ] 
    then
        echo "making toolchain for arch ${ARCH}..."
        ${MAKE_TOOLCHAIN_PATH} --platform=android-"${ANDROID_PLATFORM}" --toolchain="${TOOLCHAIN}-${TOOLCHAIN_VERSION}" --install-dir="${TOOLCHAIN_PATH}"

        if [ $? -ne 0 ]
        then
            popd > /dev/null
            exit $?
        fi
    fi

    FLAGS=""
    CPPFLAGS=""
    CFLAGS=""
    LDFLAGS=""
    TOOLCHAIN_BASENAME=${TOOLCHAIN_PATH}/bin/${TOOLCHAIN_PREFIX}
    if [[ "${ARCH}" == "armeabi-v7a" ]]
    then
        FLAGS="${FLAGS} -march=armv7-a -mfloat-abi=softfp -mfpu=neon"
    fi

    if [[ "${ARCH}" == "x86" ]]
    then
        CFLAGS="${CFLAGS} -DOPENSSL_BN_ASM_PART_WORDS"
    fi

    export CC=${TOOLCHAIN_BASENAME}-gcc
    export CXX=${TOOLCHAIN_BASENAME}-g++
    export LD=${TOOLCHAIN_BASENAME}-ld
    export AR=${TOOLCHAIN_BASENAME}-ar
    export RANLIB=${TOOLCHAIN_BASENAME}-ranlib
    export STRIP=${TOOLCHAIN_BASENAME}-strip
    export CPPFLAGS="${CPPFLAGS} ${FLAGS}"
    export CFLAGS="${CFLAGS} ${FLAGS}"
    export LDFLAGS="${LDFLAGS} ${FLAGS}"
}

function make_lib {
    ARGS="${1}"
    if [ $? -ne 0 ]
    then
        popd > /dev/null
        exit $?
    fi

    make clean
    make ${ARGS} CC="${CC}" CFLAG="${CFLAGS}"

    if [ $? -ne 0 ]
    then
        popd > /dev/null
        exit $?
    fi
}

function build_openssl {
    ARCH="${1}"
    OPENSSL_LIBS_PATH="${LIBS_PATH}/openssl/libs/${ARCH}"
    CRYPTO_LIB="${OPENSSL_LIBS_PATH}/libcrypto.a"
    SSL_LIB="${OPENSSL_LIBS_PATH}/libssl.a"

    if [ ! -d "${OPENSSL_LIBS_PATH}" ]
    then
        mkdir -p "${OPENSSL_LIBS_PATH}"
    fi

    if [ ! -f "${CRYPTO_LIB}" ] || [ ! -f "${SSL_LIB}" ]
    then
        echo "building openssl for arch ${ARCH}..."
        pushd "${OPENSSL_DIR}" > /dev/null
        setup_lib "${ARCH}"
        if [[ "${ARCH}" == "x86" ]] || [[ "${ARCH}" == "x86_64" ]]
        then
            SUFFIX="-x86"
        fi
        ./Configure "android${SUFFIX}" > /dev/null 2>&1
        make_lib > /dev/null 2>&1
        cp ./libcrypto.a "${CRYPTO_LIB}"
        cp ./libssl.a "${SSL_LIB}"
        popd > /dev/null
    fi
}

function build_curl {
    ARCH="${1}"
    CURL_LIBS_PATH="${LIBS_PATH}/curl/libs/${ARCH}"
    OPENSSL_LIBS_PATH="${LIBS_PATH}/openssl/libs/${ARCH}"
    LIB="${CURL_LIBS_PATH}/libcurl.a"

    if [ ! -d "${CURL_LIBS_PATH}" ]
    then
        mkdir -p "${CURL_LIBS_PATH}"
    fi

    if [ ! -f "${LIB}" ] 
    then
        echo "building curl for arch ${ARCH}..."
        pushd "${CURL_DIR}" > /dev/null
        setup_lib "${ARCH}"
        export CPPFLAGS="${CPPFLAGS} -I${OPENSSL_DIR}/include"
        export LDFLAGS="${LDFLAGS} -L${OPENSSL_LIBS_PATH}"
        ./configure --disable-shared --enable-static --enable-threaded-resolver \
            --with-zlib --with-ssl --disable-dependency-tracking \
            --disable-ldap --disable-imap --disable-gopher --disable-rtsp \
            --without-libidn --host="arm-linux" > /dev/null 2>&1
        make_lib -j > /dev/null 2>&1
        cp ./lib/.libs/libcurl.a ${LIB}
        popd > /dev/null
    fi
}

if [ ! -f "${NDK_ROOT}/ndk-build" ]
then
    echo "please define a valid NDK_ROOT var"
    exit 0
fi
echo "building curl for Android in ${BUILD_DIR}..."
mkdir -p "${BUILD_DIR}"

prepare_lib "openssl" "${OPENSSL_URL}"
OPENSSL_DIR="${DIR}"
if [ -d "${OPENSSL_DIR}" ] 
then
    build_openssl "armeabi"
    build_openssl "armeabi-v7a"
    #build_openssl "arm64"
    build_openssl "x86"
    #build_openssl "x86_64"
fi


prepare_lib "curl" "${CURL_URL}"
CURL_DIR="${DIR}"
if [ -d "${CURL_DIR}" ] 
then
    build_curl "armeabi"
    build_curl "armeabi-v7a"
    #build_curl "arm64"
    build_curl "x86"
    #build_curl "x86_64"

fi

echo "copying openssl headers..."
OPENSSL_HEADERS_PATH="${LIBS_PATH}/openssl/include/openssl"
rm -rf "${OPENSSL_HEADERS_PATH}"
mkdir -p "${OPENSSL_HEADERS_PATH}"
cp -r "${OPENSSL_DIR}"/include/openssl/*.h "${OPENSSL_HEADERS_PATH}"

echo "copying curl headers..."
CURL_HEADERS_PATH="${LIBS_PATH}/curl/include/curl"
rm -rf "${CURL_HEADERS_PATH}"
mkdir -p "${CURL_HEADERS_PATH}"
cp -r "${CURL_DIR}"/include/curl/*.h "${CURL_HEADERS_PATH}"
