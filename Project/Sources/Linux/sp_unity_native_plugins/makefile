ROOT_DIR:=$(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))

BASE_PATH = $(ROOT_DIR)
COMMON_SRC_PATH = $(BASE_PATH)/../../Common
CURL_COMMON_SRC_PATH = $(COMMON_SRC_PATH)/sp_unity_curl

SRC_PATH = $(BASE_PATH)/sp_unity_curl/src

CURL_INCLUDE_PATH = $(BASE_PATH)/sp_unity_curl/lib/curl/include/x86_64
CURL_LIB_PATH = $(BASE_PATH)/sp_unity_curl/lib/curl/libs/x86_64

LIBZ_INCLUDE_PATH = $(BASE_PATH)/sp_unity_curl/lib/libz/include/x86_64
LIBZ_LIB_PATH = $(BASE_PATH)/sp_unity_curl/lib/libz/libs/x86_64

OPENSSL_INCLUDE_PATH = $(BASE_PATH)/sp_unity_curl/lib/openssl/include/x86_64
OPENSSL_LIB_PATH = $(BASE_PATH)/sp_unity_curl/lib/openssl/libs/x86_64

CFLAGS = -std=c++11 -fPIC -Wall

INCLUDES = -I$(CURL_COMMON_SRC_PATH) -I$(CURL_INCLUDE_PATH) -I$(LIBZ_INCLUDE_PATH) -I$(OPENSSL_INCLUDE_PATH)

LFLAGS =  -L$(CURL_LIB_PATH) -L$(LIBZ_LIB_PATH) -L$(OPENSSL_LIB_PATH)
LIBS = -lcurl -lssl -lcrypto -lz

CC = g++ # C++ compiler
LDFLAGS = -shared # linking flags
RM = rm -f  # rm command
TARGET_LIB = libsp_unity_curl.so # target lib

SRCS = $(CURL_COMMON_SRC_PATH)/SPUnityCurlFacade.cpp $(CURL_COMMON_SRC_PATH)/SPUnityCurlManager.cpp $(SRC_PATH)/SPUnityCurlFacadeLinux.cpp # source files
OBJS = $(SRCS:.cpp=.o)

.PHONY: depend clean

all:    $(TARGET_LIB)
		@echo  $(TARGET_LIB) has been compiled
		
$(TARGET_LIB): $(OBJS)
		$(CC) $(CFLAGS) $(INCLUDES) -o $(TARGET_LIB) $(OBJS) $(LDFLAGS) $(LFLAGS) $(LIBS)

.cpp.o:
		$(CC) $(CFLAGS) $(INCLUDES) -c $<  -o $@

clean:
		$(RM) *.o *~ $(TARGET_LIB)

depend: $(SRCS)
		makedepend $(INCLUDES) $^

# DO NOT DELETE THIS LINE -- make depend needs it
