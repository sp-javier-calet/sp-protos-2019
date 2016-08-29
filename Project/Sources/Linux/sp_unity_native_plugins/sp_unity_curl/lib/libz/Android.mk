LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := libz
LOCAL_SRC_FILES := libs/$(TARGET_ARCH_ABI)/libz.a

include $(PREBUILT_STATIC_LIBRARY)
