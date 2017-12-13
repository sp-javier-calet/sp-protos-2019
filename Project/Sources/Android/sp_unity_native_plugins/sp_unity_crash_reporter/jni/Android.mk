LOCAL_PATH := $(call my-dir)
BASE_PATH := $(LOCAL_PATH)/..
PLUGINS_PATH := $(BASE_PATH)/..
COMMON_SRC_PATH := $(BASE_PATH)/../../../Common

CR_SRC_PATH := $(COMMON_SRC_PATH)/sp_unity_crash_reporter
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_crash_reporter
LOCAL_MODULE_FILENAME := libsp_unity_crash_reporter

LOCAL_CFLAGS    := -Werror
LOCAL_LDLIBS    := -llog
LOCAL_SRC_FILES := $(CR_SRC_PATH)/SPUnityBreadcrumbManager.cpp \
                   $(CR_SRC_PATH)/SPUnityBreadcrumbManagerFacade.cpp \
                   $(BASE_PATH)/src/SPUnityCrashReporter.cpp \
                   $(BASE_PATH)/src/SPUnityCrashReporterFacade.cpp
                   

LOCAL_STATIC_LIBRARIES := breakpad_client
LOCAL_SHARED_LIBRARIES := sp_unity_utils

LOCAL_C_INCLUDES := $(CR_SRC_PATH)

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(PLUGINS_PATH))

ifeq ($(TARGET_ARCH),x86)
else
$(call import-module,lib/google-breakpad/android/google_breakpad)
endif

$(call import-module,sp_unity_utils/jni)
