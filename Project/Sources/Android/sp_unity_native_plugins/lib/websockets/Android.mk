LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := websockets
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/include
LOCAL_SRC_FILES := libs/$(TARGET_ARCH_ABI)/libwebsockets.a

include $(PREBUILT_STATIC_LIBRARY)
