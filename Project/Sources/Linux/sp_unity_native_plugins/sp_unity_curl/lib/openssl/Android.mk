LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := crypto
LOCAL_SRC_FILES := libs/$(TARGET_ARCH_ABI)/libcrypto.a

include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE := ssl
LOCAL_SRC_FILES := libs/$(TARGET_ARCH_ABI)/libssl.a

include $(PREBUILT_STATIC_LIBRARY)
