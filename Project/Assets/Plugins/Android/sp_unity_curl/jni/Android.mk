LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_curl
LOCAL_MODULE_FILENAME := libsp_unity_curl

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_SRC_FILES := $(BASE_PATH)/src/SPUnityCurlFacade.cpp \
                   $(BASE_PATH)/src/SPUnityCurlManager.cpp \
                   $(BASE_PATH)/src/SPUnityCurlFacadeAndroid.cpp

LOCAL_LDLIBS := -lz -llog
LOCAL_STATIC_LIBRARIES := curl ssl crypto

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(BASE_PATH)/lib) 
$(call import-module,curl)
$(call import-module,openssl)