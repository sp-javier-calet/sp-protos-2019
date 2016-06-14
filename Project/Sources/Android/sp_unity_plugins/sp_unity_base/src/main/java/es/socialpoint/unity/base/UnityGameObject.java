package es.socialpoint.unity.base;

public class UnityGameObject {

    static SPUnityActivity _activity;

    static String _unityListenerName;

    static void Init(SPUnityActivity activity) {
        _activity = activity;
    }

    public UnityGameObject(String objectName) {
        _unityListenerName = objectName;
    }

    public void SendMessage(String function, String message) {
        if(_activity != null) {
            _activity.UnitySendMessage(_unityListenerName, function, message);
        }
    }
}
