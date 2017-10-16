package es.socialpoint.unity.notification;

import com.google.firebase.iid.FirebaseInstanceIdService;

public class TokenListenerService extends FirebaseInstanceIdService {
    /**
     * Called if InstanceID token is updated. This may occur if the security of
     * the previous token had been compromised. This call is initiated by the
     * InstanceID provider.
     */
    @Override
    public void onTokenRefresh() {
        NotificationBridge.registerForRemote();
    }
}

