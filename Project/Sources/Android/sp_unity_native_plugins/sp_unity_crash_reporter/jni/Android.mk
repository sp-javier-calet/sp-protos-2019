LOCAL_PATH := $(call my-dir)
BASE_PATH  := $(LOCAL_PATH)/..
COMMON_SRC_PATH  := $(BASE_PATH)/../../../Common/sp_unity_crash_reporter
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_crash_reporter
LOCAL_MODULE_FILENAME := libsp_unity_crash_reporter

LOCAL_ARM_MODE  := arm
LOCAL_CFLAGS    := -Werror
LOCAL_LDLIBS    := -llog
LOCAL_SRC_FILES := $(COMMON_SRC_PATH)/SPUnityBreadcrumbManager.cpp \
                   $(COMMON_SRC_PATH)/SPUnityBreadcrumbManagerFacade.cpp \
                   $(BASE_PATH)/src/SPUnityCrashReporter.cpp \
                   $(BASE_PATH)/src/SPUnityCrashRepoterFacade.cpp
                   

LOCAL_STATIC_LIBRARIES := breakpad_client
LOCAL_SHARED_LIBRARIES := sp_unity_utils

LOCAL_C_INCLUDES := $(COMMON_SRC_PATH)

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(BASE_PATH)/../)

ifeq ($(TARGET_ARCH),x86)
else
$(call import-add-path,$(BASE_PATH)/lib)
$(call import-module,google-breakpad/android/google_breakpad)
endif

$(call import-module,sp_unity_utils/jni)
