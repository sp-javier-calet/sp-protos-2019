LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_crash_reporter
LOCAL_MODULE_FILENAME := libsp_unity_crash_reporter

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_LDLIBS    := -llog
LOCAL_SRC_FILES := $(BASE_PATH)/src/SPUnityCrashReporter.cpp \
                   $(BASE_PATH)/src/NativeInterface.cpp

LOCAL_STATIC_LIBRARIES := breakpad_client

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(BASE_PATH)/../) 


ifeq ($(TARGET_ARCH),x86)
else
$(call import-module,google-breakpad/android/google_breakpad)
endif
