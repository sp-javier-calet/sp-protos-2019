package es.socialpoint.unity.notification;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import es.socialpoint.unity.notification.IntentParameters.Origin;

public class AlarmReceiver extends BroadcastReceiver {

    private static final String TAG = "AlarmReceiver";

    @Override
    public void onReceive(Context context, Intent intent) {
        NotificationShower
            .create(context)
            .setId(intent.getIntExtra(IntentParameters.EXTRA_ID, 0))
            .setTitle(intent.getStringExtra(IntentParameters.EXTRA_TITLE))
            .setText(intent.getStringExtra(IntentParameters.EXTRA_TEXT))
            .setOrigin(Origin.LOCAL_NOTIFICATION)
            .show();
    }
}
