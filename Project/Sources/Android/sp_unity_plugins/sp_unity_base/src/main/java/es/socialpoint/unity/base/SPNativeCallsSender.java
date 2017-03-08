package es.socialpoint.unity.base;

public class SPNativeCallsSender {

    private static String Separator;
    private static String GameObjectName;
    private static String MethodName;

    static UnityGameObject _gameObject;

    static void Init(String gameObjectName, String methodName, String separator) {
        GameObjectName = gameObjectName;
        MethodName = methodName;
        Separator = separator;
        _gameObject = new UnityGameObject(GameObjectName);
    }

    static final String combineMethodMessage(String methodName, String argKey)
    {
        return methodName + Separator + argKey;
    }

    static public void SendMessage(String methodName) {
        _gameObject.SendMessage(MethodName, methodName);
    }

    static public void SendMessage(String methodName, String argKey) {
        _gameObject.SendMessage(MethodName, combineMethodMessage(methodName,argKey));
    }
}
