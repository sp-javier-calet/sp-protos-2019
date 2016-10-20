LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
COMMON_SRC_PATH  := $(BASE_PATH)/../../../Common
CURL_SRC_PATH  := $(COMMON_SRC_PATH)/sp_unity_curl
CURL_INLCUDE_PATH := $(BASE_PATH)/lib/curl/include/$(TARGET_ARCH_ABI)
SRC_PATH  := $(BASE_PATH)/src
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_curl
LOCAL_MODULE_FILENAME := libsp_unity_curl

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_SRC_FILES := $(CURL_SRC_PATH)/Certificate.cpp \
                   $(CURL_SRC_PATH)/ConnectionManager.cpp \
                   $(CURL_SRC_PATH)/CurlClient.cpp \
                   $(CURL_SRC_PATH)/SPUnityCurl.cpp \
                   $(SRC_PATH)/SPUnityCurlFacadeAndroid.cpp

LOCAL_C_INCLUDES := $(CURL_SRC_PATH) \
                    $(CURL_INLCUDE_PATH)

LOCAL_LDLIBS := -lz -llog
LOCAL_STATIC_LIBRARIES := curl ssl crypto nghttp2

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(BASE_PATH)/lib)
$(call import-module,curl)
$(call import-module,openssl)
$(call import-module,nghttp2)
