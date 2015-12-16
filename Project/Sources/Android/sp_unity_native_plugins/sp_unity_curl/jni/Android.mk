LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
BASE_SRC_PATH  := $(BASE_PATH)/../../Ios
SRC_PATH  := $(BASE_PATH)/src
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_curl
LOCAL_MODULE_FILENAME := libsp_unity_curl

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_SRC_FILES := $(BASE_SRC_PATH)/SPUnityCurlFacade.cpp \
                   $(BASE_SRC_PATH)/SPUnityCurlManager.cpp \
                   $(SRC_PATH)/SPUnityCurlFacadeAndroid.cpp

LOCAL_C_INCLUDES := $(BASE_SRC_PATH)

LOCAL_LDLIBS := -lz -llog
LOCAL_STATIC_LIBRARIES := curl ssl crypto

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(BASE_PATH)/lib) 
$(call import-module,curl)
$(call import-module,openssl)