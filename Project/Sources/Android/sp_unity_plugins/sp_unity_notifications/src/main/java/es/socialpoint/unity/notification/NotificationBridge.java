package es.socialpoint.unity.notification;

import android.app.Activity;
import android.app.AlarmManager;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GooglePlayServicesUtil;
import com.google.android.gms.gcm.GoogleCloudMessaging;
import com.google.android.gms.iid.InstanceID;
import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import es.socialpoint.unity.configuration.Metadata;

public class NotificationBridge {
    private static final String TAG = "NotificationBridge";
    private static final String SENDER_ID_KEY = "GOOGLE_API_PROJECT_NUMBER";
    public static final String DEFAULT_CHANNEL_ID = "default";

    private static Handler mHandler = new Handler(Looper.getMainLooper());
    private static AsyncTask<Void, Void, Void> mRegisterTask;
    private static String mPushNotificationToken;
    private static String mPushNotificationTokenError;
    private static String mSenderId;
    private static int mAlarmIdCounter = 0;

    public static void schedule(int id, long delay, String title, String text, String channelId) {
        Activity currentActivity = UnityPlayer.currentActivity;
        int alarmId = ++mAlarmIdCounter;

        Intent intent = new Intent(currentActivity, AlarmReceiver.class);
        intent.putExtra(IntentParameters.EXTRA_ID, id);
        intent.putExtra(IntentParameters.EXTRA_ALARM_ID, alarmId);
        intent.putExtra(IntentParameters.EXTRA_TITLE, title);
        intent.putExtra(IntentParameters.EXTRA_TEXT, text);
        intent.putExtra(IntentParameters.EXTRA_CHANNEL_ID, channelId);
        AlarmManager am = (AlarmManager)currentActivity.getSystemService(Context.ALARM_SERVICE);
        Log.d(TAG, "Scheduling alarm " + alarmId + " [ " + id + " - " + title + " : " + text + "] with delay " + delay + " on channel '" + channelId + "'");

        // Use FLAG_UPDATE_CURRENT to override PendingIntent for current alarmId - even if it is currently cancelled -.
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, alarmId, intent, PendingIntent.FLAG_UPDATE_CURRENT);
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

        int alarmId = 1;
        Intent intent = new Intent(currentActivity, AlarmReceiver.class);
        PendingIntent pendingIntent = null;
        do {
            /* Use FLAG_NO_CREATE to get only existing PendingIntent for the current context.
             * Since we schedule the alarms using an incremental alarmId, we can search for them using an incremental index */
            pendingIntent = PendingIntent.getBroadcast(currentActivity, alarmId, intent, PendingIntent.FLAG_NO_CREATE);
            if(pendingIntent != null) {
                Log.d(TAG, "Cancelling Alarm with id: " + alarmId);
                am.cancel(pendingIntent);
            }

            // Increment AlarmId
            alarmId++;
        } while(pendingIntent != null);

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
            mRegisterTask = null;
        }

        boolean postSuccess = mHandler.post(new Runnable() {
            public void run() {
                mRegisterTask = new AsyncTask<Void, Void, Void>() {
                    @Override
                    protected Void doInBackground(Void... params) {
                        try {
                            InstanceID instanceID = InstanceID.getInstance(UnityPlayer.currentActivity);
                            String senderId = getSenderId(UnityPlayer.currentActivity);

                            if(senderId != null && !senderId.isEmpty()) {
                                String token = instanceID.getToken(senderId,
                                        GoogleCloudMessaging.INSTANCE_ID_SCOPE, null);
                                Log.i(TAG, "GCM Registration Token: " + token);

                                // Notify registered token
                                setNotificationToken(token);
                            } else {
                                String tokenError = "Invalid sender ID for push enabled. Is " + SENDER_ID_KEY + " meta-data set?";
                                Log.e(TAG, tokenError);
                                setNotificationTokenError(tokenError);
                            }

                        } catch (Exception e) {
                            String tokenError = "Failed to complete token refresh";
                            Log.e(TAG, tokenError, e);
                            setNotificationTokenError(tokenError);
                        }
                        return null;
                    }

                    @Override
                    protected void onPostExecute(Void result) {
                        mRegisterTask = null;
                    }
                }.execute(null, null, null);
            }
        });

        if(!postSuccess) {
            String tokenError = "Failed to dispatch token petition";
            Log.e(TAG, tokenError);
            setNotificationTokenError(tokenError);
        }
    }

    private static synchronized void setNotificationToken(String token) {
        mPushNotificationToken = token;
    }

    public static synchronized String getNotificationToken() {
        return mPushNotificationToken;
    }

    private static synchronized void setNotificationTokenError(String tokenError) {
        mPushNotificationTokenError = tokenError;
    }

    public static synchronized String getNotificationTokenError() {
        return mPushNotificationTokenError;
    }

    private static NotificationChannel createChannelConfiguration(String channelId, String name, String description)
    {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O)
        {
            return null;
        }

        NotificationChannel channel = new NotificationChannel(channelId, name, NotificationManager.IMPORTANCE_HIGH);
        if (description != null)
        {
            channel.setDescription(description);
        }
        return channel;
    }

    private static void addChannelConfiguration(List<NotificationChannel> channels, Set<String> channelIds,
                                                String channelId, String name, String description)
    {
        channels.add(createChannelConfiguration(channelId, name, description));
        channelIds.add(channelId);
    }

    private static void addConfiguredChannels(List<NotificationChannel> channels, Set<String> channelIds,
                                              String[] ids, String[] names, String[] descriptions)
    {
        addChannelConfiguration(channels, channelIds, DEFAULT_CHANNEL_ID, "Default", null);

        for (int i = 0; i < ids.length; i++)
        {
            addChannelConfiguration(channels, channelIds, ids[i], names[i], descriptions[i]);
        }
    }

    public static synchronized void setupChannels(String[] ids, String[] names, String[] descriptions)
    {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O)
        {
            return;
        }

        List<NotificationChannel> channels = new ArrayList<>();
        Set<String> channelIds = new HashSet<>();
        addConfiguredChannels(channels, channelIds, ids, names, descriptions);

        Activity currentActivity = UnityPlayer.currentActivity;
        NotificationManager notificationManager = (NotificationManager) currentActivity.getSystemService(Context.NOTIFICATION_SERVICE);

        // Remove the obsolete channels
        for (NotificationChannel channel : notificationManager.getNotificationChannels())
        {
            String channelId = channel.getId();
            if (!channelIds.contains(channelId))
            {
                notificationManager.deleteNotificationChannel(channelId);
            }
        }

        // Create and update the configured channels
        notificationManager.createNotificationChannels(channels);
    }
}
