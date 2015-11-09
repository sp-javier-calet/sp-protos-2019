package es.socialpoint.unity.notification;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import es.socialpoint.unity.notification.IntentParameters.Origin;

public class PushNotificationReceiver extends BroadcastReceiver {

    private static final String TAG = "PushNotificationReceiver";
    
    @Override
    public void onReceive(Context context, Intent intent) {
        Log.e(TAG, "Received intent for push notification." + intent);
        
        Bundle intentExtras = intent.getExtras();
        NotificationShower shower = NotificationShower
                .create(context)
                .setOrigin(Origin.PUSH_NOTIFICATION);
        
        if(intentExtras.containsKey(IntentParameters.EXTRA_TITLE)) {
            shower.setTitle(intentExtras.getString(IntentParameters.EXTRA_TITLE));
        }
        
        if(intentExtras.containsKey(IntentParameters.EXTRA_TEXT)) {
            shower.setText(intentExtras.getString(IntentParameters.EXTRA_TEXT));
        }        
            
        shower.show();
    }
}
