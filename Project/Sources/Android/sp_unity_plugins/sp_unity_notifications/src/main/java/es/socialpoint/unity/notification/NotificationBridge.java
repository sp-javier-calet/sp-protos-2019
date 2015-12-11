package es.socialpoint.unity.notification;

import android.app.Activity;
import android.app.AlarmManager;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.os.AsyncTask;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.gcm.GoogleCloudMessaging;
import com.google.android.gms.iid.InstanceID;
import com.unity3d.player.UnityPlayer;

import es.socialpoint.unity.configuration.Metadata;

public class NotificationBridge {
    private static final String TAG = "NotificationBridge";
    private static final String SENDER_ID_KEY = "GOOGLE_API_PROJECT_NUMBER";

    private static AsyncTask<Void, Void, Void> mRegisterTask;
    private static String mPushNotificationToken;
    private static String mSenderId;
    private static int mAlarmIdCounter = 0;

    public static void schedule(int id, long delay, String title, String text) {
        Activity currentActivity = UnityPlayer.currentActivity;
        int alarmId = ++mAlarmIdCounter;

        Intent intent = new Intent(currentActivity, AlarmReceiver.class);
        intent.putExtra(IntentParameters.EXTRA_ID, id);
        intent.putExtra(IntentParameters.EXTRA_ALARM_ID, alarmId);
        intent.putExtra(IntentParameters.EXTRA_TITLE, title);
        intent.putExtra(IntentParameters.EXTRA_TEXT, text);
        AlarmManager am = (AlarmManager)currentActivity.getSystemService(Context.ALARM_SERVICE);
        Log.d(TAG, "Scheduling alarm " + alarmId + " [ " + id + " - " + title + " : " + text + "] with delay " + delay);
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, alarmId, intent, 0);
        am.set(AlarmManager.RTC_WAKEUP, System.currentTimeMillis() + delay * 1000, pendingIntent);
    }

    public static void clearReceived() {
        Log.d(TAG, "Clearing received notifications");
        Activity currentActivity = UnityPlayer.currentActivity;
        NotificationManager mng = (NotificationManager)currentActivity.getSystemService(Context.NOTIFICATION_SERVICE);
        mng.cancelAll();
    }

    public static void cancelPending() {
        Log.d(TAG, "Cancelling pending notifications (" + mAlarmIdCounter + ")");
        Activity currentActivity = UnityPlayer.currentActivity;
        AlarmManager am = (AlarmManager)currentActivity.getSystemService(Context.ALARM_SERVICE);
        
        for(int i = 0; i <= mAlarmIdCounter; i++) {
            Intent intent = new Intent(currentActivity, AlarmReceiver.class);
            PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, i, intent, 0);
            am.cancel(pendingIntent);
        }

        mAlarmIdCounter = 0;
    }

    private static boolean isPlayServicesAvailable() {
        int resultCode = GooglePlayServicesUtil.isGooglePlayServicesAvailable(UnityPlayer.currentActivity);
        return resultCode == ConnectionResult.SUCCESS;
    }

    private static String getSenderId(Context context) {
        if(mSenderId == null || mSenderId.isEmpty()) {
            Metadata metadata = new Metadata(context);
            mSenderId = metadata.get(SENDER_ID_KEY, "");
        }
        return mSenderId;
    }

    public static void registerForRemote() {
        // Check play services availability 
        if(!isPlayServicesAvailable()) {
            return;
        }
        
        // Cancel current register task, if exist
        if(mRegisterTask != null) {
            mRegisterTask.cancel(true);
        }
        
        mRegisterTask = new AsyncTask<Void, Void, Void>() {
                @Override
                protected Void doInBackground(Void... params) {
                    try {
                        InstanceID instanceID = InstanceID.getInstance(UnityPlayer.currentActivity);
                        String token = instanceID.getToken(getSenderId(UnityPlayer.currentActivity),
                                GoogleCloudMessaging.INSTANCE_ID_SCOPE, null);
                        Log.i(TAG, "GCM Registration Token: " + token);
                        
                        // Notify registered token
                        setNotificationToken(token);

                    } catch (Exception e) {
                        Log.e(TAG, "Failed to complete token refresh", e);
                    }
                    return null;
                }

                @Override
                protected void onPostExecute(Void result) {
                    mRegisterTask = null;
                }
            }.execute(null, null, null);
    }

    private static void setNotificationToken(String token) {
        mPushNotificationToken = token;
    }

    public static String getNotificationToken() {
        return mPushNotificationToken;
    }
}
