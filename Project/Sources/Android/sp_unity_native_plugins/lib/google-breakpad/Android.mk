LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := breakpad_client
LOCAL_EXPORT_C_INCLUDES := $(LOCAL_PATH)/include/breakpad/common/android/include \
                           $(LOCAL_PATH)/include/breakpad
LOCAL_SRC_FILES := $(LOCAL_PATH)/libs/$(TARGET_ARCH_ABI)/libbreakpad_client.a
LOCAL_EXPORT_LDLIBS := -llog

include $(PREBUILT_STATIC_LIBRARY)
