LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := crypto
BASE_PATH := $(LOCAL_PATH)/include
LOCAL_EXPORT_C_INCLUDES := $(BASE_PATH)
LOCAL_SRC_FILES := libs/$(TARGET_ARCH_ABI)/libcrypto.a

include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)

LOCAL_MODULE := ssl
BASE_PATH := $(LOCAL_PATH)/include
LOCAL_EXPORT_C_INCLUDES := $(BASE_PATH)
LOCAL_SRC_FILES := libs/$(TARGET_ARCH_ABI)/libssl.a

include $(PREBUILT_STATIC_LIBRARY)