LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
COMMON_SRC_PATH  := $(BASE_PATH)/../../../Common
CURL_SRC_PATH  := $(COMMON_SRC_PATH)/sp_unity_curl
SRC_PATH  := $(BASE_PATH)/src
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_curl
LOCAL_MODULE_FILENAME := libsp_unity_curl

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_SRC_FILES := $(CURL_SRC_PATH)/SPUnityCurlFacade.cpp \
                   $(CURL_SRC_PATH)/SPUnityCurlManager.cpp \
                   $(SRC_PATH)/SPUnityCurlFacadeAndroid.cpp \
                   $(CURL_SRC_PATH)/CurlHttpClientCallbacks.cpp \
                   $(CURL_SRC_PATH)/SSLCertificate.cpp \
                   $(CURL_SRC_PATH)/SSLCertificateReader.cpp \
                   $(CURL_SRC_PATH)/SSLCertificateValidator.cpp

LOCAL_C_INCLUDES := $(CURL_SRC_PATH) \
                    $(COMMON_SRC_PATH)

LOCAL_LDLIBS := -lz -llog
LOCAL_STATIC_LIBRARIES := curl ssl crypto

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(BASE_PATH)/lib) 
$(call import-module,curl)
$(call import-module,openssl)
