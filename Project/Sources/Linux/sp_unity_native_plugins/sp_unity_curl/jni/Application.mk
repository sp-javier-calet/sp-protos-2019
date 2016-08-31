APP_ABI          := x86_64
APP_STL          := gnustl_static
APP_OPTIM        := release
APP_CFLAGS       := -std=c++11 -fno-stack-protector
APP_LDFLAGS      := -static-libstdc++ -Wl,-Bstatic -lm

NDK_TOOLCHAIN_VERSION=4.8
