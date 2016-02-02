LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := curl
LOCAL_SRC_FILES := $(LOCAL_PATH)/libs/$(TARGET_ARCH_ABI)/libcurl.a

include $(PREBUILT_STATIC_LIBRARY)
