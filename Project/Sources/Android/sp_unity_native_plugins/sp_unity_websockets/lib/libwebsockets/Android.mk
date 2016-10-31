LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := websockets
BASE_PATH := $(LOCAL_PATH)/include
LOCAL_EXPORT_C_INCLUDES := $(BASE_PATH)
LOCAL_SRC_FILES := libs/android/$(TARGET_ARCH_ABI)/libwebsockets.a

include $(PREBUILT_STATIC_LIBRARY)
