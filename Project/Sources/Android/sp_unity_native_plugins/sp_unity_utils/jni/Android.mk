LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
COMMON_SRC_PATH  := $(BASE_PATH)/../../../Common/sp_unity_utils
SRC_PATH  := $(BASE_PATH)/src
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_utils
LOCAL_MODULE_FILENAME := libsp_unity_utils

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_SRC_FILES := $(SRC_PATH)/main.cpp \
		$(SRC_PATH)/UnityGameObject.cpp \
		$(COMMON_SRC_PATH)/SPUnityUtils.cpp \
		$(COMMON_SRC_PATH)/breadcrumbs/SPUnityBreadcrumbManager.cpp

LOCAL_EXPORT_C_INCLUDES := $(SRC_PATH)\
		$(COMMON_SRC_PATH)/breadcrumbs

LOCAL_LDLIBS := -llog

include $(BUILD_SHARED_LIBRARY)
