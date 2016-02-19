LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
COMMON_SRC_PATH  := $(BASE_PATH)/../../../Common
UTILS_SRC_PATH  := $(COMMON_SRC_PATH)/sp_unity_utils
SRC_PATH  := $(BASE_PATH)/src
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_utils
LOCAL_MODULE_FILENAME := libsp_unity_utils

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_SRC_FILES := $(UTILS_SRC_PATH)/SPUnityUtils.cpp

LOCAL_C_INCLUDES := $(UTILS_SRC_PATH) \
                    $(COMMON_SRC_PATH)

LOCAL_LDLIBS := -lz -llog

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(BASE_PATH)/lib)
