LOCAL_PATH := $(call my-dir)
BASE_PATH := $(LOCAL_PATH)/..
PLUGINS_PATH := $(BASE_PATH)/..
COMMON_SRC_PATH := $(BASE_PATH)/../../../Common


UTILS_SRC_PATH := $(COMMON_SRC_PATH)/sp_unity_utils
SRC_PATH := $(BASE_PATH)/src
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_utils
LOCAL_MODULE_FILENAME := libsp_unity_utils

LOCAL_CFLAGS    := -Werror
LOCAL_SRC_FILES := $(SRC_PATH)/main.cpp \
		$(SRC_PATH)/UnityGameObject.cpp \
		$(UTILS_SRC_PATH)/SPUnityUtils.cpp \
		$(UTILS_SRC_PATH)/SPUnityFileUtils.cpp \
		$(SRC_PATH)/SPNativeCallsSender.cpp \
		$(SRC_PATH)/JniEnv.cpp 

LOCAL_EXPORT_C_INCLUDES := $(SRC_PATH) \
							$(UTILS_SRC_PATH)

LOCAL_LDLIBS := -llog

include $(BUILD_SHARED_LIBRARY)
