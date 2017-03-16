package es.socialpoint.unity.notification;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.helpshift.supportCampaigns.InitializeHelpshiftUtil;
import com.helpshift.supportCampaigns.UnityAPIDelegate;

import es.socialpoint.unity.notification.IntentParameters.Origin;

public class PushNotificationReceiver extends BroadcastReceiver {

    private static final String TAG = "PushReceiver";

    private static final String MANAGED_ORIGIN = "socialpoint";
    private static final String ORIGIN_KEY = "origin";

    @Override
    public void onReceive(Context context, Intent intent) {
        Log.e(TAG, "Received intent for push notification." + intent);

        boolean handled = handlePushNotification(context, intent) || handleExtenalPushNotification(context, intent);

        if (!handled) {
            Log.w(TAG, "Unhandled push notification");
        }
    }

    private boolean isManagedNotification(Intent intent) {
        Bundle intentExtras = intent.getExtras();
        return MANAGED_ORIGIN.equals(intentExtras.getString(ORIGIN_KEY));
    }

    private boolean handlePushNotification(Context context, Intent intent) {

        if (!isManagedNotification(intent)) {
            return false;
        }

        Bundle intentExtras = intent.getExtras();
        NotificationShower shower = NotificationShower
                .create(context, intent.getExtras())
                .setOrigin(Origin.PUSH_NOTIFICATION);
        if (intentExtras != null) {
            if (intentExtras.containsKey(IntentParameters.EXTRA_TITLE)) {
                shower.setTitle(intentExtras.getString(IntentParameters.EXTRA_TITLE));
            }

            if (intentExtras.containsKey(IntentParameters.EXTRA_TEXT)) {
                shower.setText(intentExtras.getString(IntentParameters.EXTRA_TEXT));
            }
        }

        shower.show();

        return true;
    }

    private boolean handleExtenalPushNotification(Context context, Intent intent) {
        if("helpshift".equals(intent.getStringExtra("origin"))) {
            UnityAPIDelegate.installDex(context);
            InitializeHelpshiftUtil.initHelpshift(context);
            UnityAPIDelegate.handlePush(context, intent);
            return true;
        }
        return false;
    }

    protected static <T extends BroadcastReceiver> T loadHandler(String clazzName) {
        T newInstance = null;

        try {
            Class<?> serviceClass = Class.forName(clazzName);
            newInstance = (T) serviceClass.newInstance();
        } catch (Exception e) {
            Log.w(TAG, "Notification receiver class '" + clazzName + "' could not be created. Reason:");
            e.printStackTrace();
        }

        return newInstance;
    }
}
