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
    
    if [[ "${ARCH}" == "i386" ]] || [[ "${ARCH}" == "x86_64" ]]
    then
        PLATFORM="iphonesimulator"
    else
        PLATFORM="iphoneos"
    fi

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
    LIB="${LIBS_PATH}/${ARCH}/libz.a"

    if [ ! -f "${LIB}" ] 
    then
        echo "building libz for arch ${ARCH}..."
        pushd "${LIBZ_DIR}" > /dev/null
        setup_lib "${ARCH}"
        ./configure > /dev/null
        make_lib -j > /dev/null
        cp ./libz.a "${LIB}"
        popd > /dev/null
    fi
}

function build_openssl {
    ARCH="${1}"
    CRYPTO_LIB="${LIBS_PATH}/${ARCH}/libcrypto.a"
    SSL_LIB="${LIBS_PATH}/${ARCH}/libssl.a"

    if [ ! -f "${CRYPTO_LIB}" ] || [ ! -f "${SSL_LIB}" ]
    then
        echo "building openssl for arch ${ARCH}..."
        pushd "${OPENSSL_DIR}" > /dev/null
        setup_lib "${ARCH}"
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
    LIB="${LIBS_PATH}/${ARCH}/libcurl.a"
    if [[ "${ARCH}" == "arm64" ]]
    then
        HOST="aarch64"
    else
        HOST="${ARCH}"
    fi

    if [ ! -f "${LIB}" ] 
    then
        echo "building curl for arch ${ARCH}..."
        pushd "${CURL_DIR}" > /dev/null
        setup_lib "${ARCH}"
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
        build_openssl "armv7"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
        build_openssl "armv7s"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
        build_openssl "arm64"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
        build_openssl "i386"
        SSL_LIBS="${SSL_LIBS} ${SSL_LIB}"
        CRYPTO_LIBS="${CRYPTO_LIBS} ${CRYPTO_LIB}"
        build_openssl "x86_64"
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
        build_curl "armv7"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
        build_curl "armv7s"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
        build_curl "arm64"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
        build_curl "i386"
        CURL_LIBS="${CURL_LIBS} ${LIB}"
        build_curl "x86_64"
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
CURL_HEADERS_PATH="curl"
rm -rf "${CURL_HEADERS_PATH}"
mkdir -p "${CURL_HEADERS_PATH}"
cp -r "${CURL_DIR}"/include/curl/*.h "${CURL_HEADERS_PATH}"
echo "patching curl headers to support both 32 and 64 bits..."
patch -d "${CURL_HEADERS_PATH}" -p0 <<EOF
--- original/curlbuild.h
+++ curlbuild.h
@@ -161,0 +162,2 @@
+#ifdef __LP64__
+
@@ -197,0 +200,40 @@
+#else
+
+/* The size of \`long', as computed by sizeof. */
+#define CURL_SIZEOF_LONG 4
+
+/* Integral data type used for curl_socklen_t. */
+#define CURL_TYPEOF_CURL_SOCKLEN_T socklen_t
+
+/* The size of \`curl_socklen_t', as computed by sizeof. */
+#define CURL_SIZEOF_CURL_SOCKLEN_T 4
+
+/* Data type definition of curl_socklen_t. */
+typedef CURL_TYPEOF_CURL_SOCKLEN_T curl_socklen_t;
+
+/* Signed integral data type used for curl_off_t. */
+#define CURL_TYPEOF_CURL_OFF_T long long
+
+/* Data type definition of curl_off_t. */
+typedef CURL_TYPEOF_CURL_OFF_T curl_off_t;
+
+/* curl_off_t formatting string directive without "%" conversion specifier. */
+#define CURL_FORMAT_CURL_OFF_T "lld"
+
+/* unsigned curl_off_t formatting string without "%" conversion specifier. */
+#define CURL_FORMAT_CURL_OFF_TU "llu"
+
+/* curl_off_t formatting string directive with "%" conversion specifier. */
+#define CURL_FORMAT_OFF_T "%lld"
+
+/* The size of \`curl_off_t', as computed by sizeof. */
+#define CURL_SIZEOF_CURL_OFF_T 8
+
+/* curl_off_t constant suffix. */
+#define CURL_SUFFIX_CURL_OFF_T LL
+
+/* unsigned curl_off_t constant suffix. */
+#define CURL_SUFFIX_CURL_OFF_TU ULL
+
+#endif
+
EOF
