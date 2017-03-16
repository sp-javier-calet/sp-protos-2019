package es.socialpoint.unity.notification;

import com.google.android.gms.iid.InstanceIDListenerService;

public class TokenListenerService extends InstanceIDListenerService {
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

