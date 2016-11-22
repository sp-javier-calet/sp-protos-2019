LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := nghttp2
LOCAL_SRC_FILES := libs/$(TARGET_ARCH_ABI)/libnghttp2.a

include $(PREBUILT_STATIC_LIBRARY)
