LOCAL_PATH := $(call my-dir)
BASE_PATH := $(LOCAL_PATH)/..
PLUGINS_PATH := $(BASE_PATH)/..
COMMON_SRC_PATH := $(BASE_PATH)/../../../Common

WS_SRC_PATH := $(COMMON_SRC_PATH)/sp_unity_websockets
include $(CLEAR_VARS)

LOCAL_MODULE := sp_unity_websockets
LOCAL_MODULE_FILENAME := libsp_unity_websockets
LOCAL_DISABLE_FATAL_LINKER_WARNINGS := true

LOCAL_CFLAGS    := -Werror
LOCAL_SRC_FILES := $(WS_SRC_PATH)/SPUnityWebSockets.cpp \
                   $(WS_SRC_PATH)/WebSocketConnection.cpp \
                   $(WS_SRC_PATH)/WebSocketsManager.cpp

LOCAL_C_INCLUDES := $(WS_SRC_PATH)

LOCAL_STATIC_LIBRARIES := websockets ssl crypto 

include $(BUILD_SHARED_LIBRARY)

# Import modules
$(call import-add-path,$(PLUGINS_PATH))
$(call import-module,lib/openssl)
$(call import-module,lib/websockets)