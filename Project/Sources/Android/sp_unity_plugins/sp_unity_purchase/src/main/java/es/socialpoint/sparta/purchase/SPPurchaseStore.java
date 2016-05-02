package es.socialpoint.sparta.purchase;

import es.socialpoint.unity.base.UnityGameObject;

/**
 * Created by abarrera on 02/05/16.
 */
public class SPPurchaseStore {

    //Class to send messages to Unity
    private static UnityGameObject _unityMessageSender;

    public static void Init(String listenerObjectName)
    {
        _unityMessageSender = new UnityGameObject(listenerObjectName);
        _unityMessageSender.SendMessage("StoreDebugLog", "*** TEST Hello World");
    }
}
