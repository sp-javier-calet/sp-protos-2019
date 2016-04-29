LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_crash_reporter
LOCAL_MODULE_FILENAME := libsp_unity_crash_reporter

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_LDLIBS    := -llog
LOCAL_SRC_FILES := $(BASE_PATH)/src/SPUnityCrashReporter.cpp \
                   $(BASE_PATH)/src/SPUnityCrashRepoterFacade.cpp

LOCAL_STATIC_LIBRARIES := breakpad_client
LOCAL_SHARED_LIBRARIES := sp_unity_utils

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(BASE_PATH)/../)

ifeq ($(TARGET_ARCH),x86)
else
$(call import-add-path,$(BASE_PATH)/lib)
$(call import-module,google-breakpad/android/google_breakpad)
endif

$(call import-module,sp_unity_utils/jni)
