package es.socialpoint.unity.notifications;

import android.app.Activity;
import android.app.AlarmManager;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.media.RingtoneManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.support.v4.app.NotificationCompat;
import android.util.Log;

import com.unity3d.player.UnityPlayerActivity;
import com.unity3d.player.UnityPlayer;

public class NotificationBridge extends BroadcastReceiver
{
	private static final String TAG = "NotificationBridge";

    public static void Schedule(int id, long delay, String title, String message, String largeIcon, String smallIcon, int color)
    {
		Activity currentActivity = UnityPlayer.currentActivity;
		Intent intent = new Intent(currentActivity, NotificationBridge.class);
        Bundle bundle = new Bundle();
		bundle.putInt("id", id);
		bundle.putString("title", title);
		bundle.putString("message", message);
		bundle.putString("largeIcon", largeIcon);
		bundle.putString("smallIcon", smallIcon);
		bundle.putInt("color", color);
        intent.putExtras(bundle);
		delay *= 1000;
        AlarmManager am = (AlarmManager)currentActivity.getSystemService(Context.ALARM_SERVICE);
        Log.d(TAG, "scheduling notification "+id+" for delay "+delay+" ...");
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, id, intent, 0);
		am.set(AlarmManager.RTC_WAKEUP, System.currentTimeMillis() + delay, pendingIntent);
    }

    public void onReceive(Context context, Intent intent)
    {
    	NotificationManager mng = (NotificationManager)context.getSystemService(Context.NOTIFICATION_SERVICE);

        String title = intent.getStringExtra("title");
        String message = intent.getStringExtra("message");
        String smallIcon = intent.getStringExtra("smallIcon");
        String largeIcon = intent.getStringExtra("largeIcon");
        int id = intent.getIntExtra("id", 0);
		int color = intent.getIntExtra("color", -1);
        Log.d(TAG, "showing notification "+id+" ...");

        Resources res = context.getResources();
        Intent notificationIntent = new Intent(context, UnityPlayer.currentActivity.getClass());
		Bundle bundle = new Bundle();
		bundle.putInt("id", id);
		notificationIntent.putExtras(bundle);
        PendingIntent contentIntent = PendingIntent.getActivity(context, 0, notificationIntent, 0);
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context);

        builder.setContentIntent(contentIntent)
        	.setWhen(System.currentTimeMillis())
        	.setAutoCancel(true)
        	.setContentTitle(title)
        	.setContentText(message)
			.setDefaults(Notification.DEFAULT_SOUND | Notification.DEFAULT_VIBRATE);

		if(smallIcon != null && smallIcon.length() > 0)
		{
			builder.setSmallIcon(res.getIdentifier(smallIcon, "drawable", context.getPackageName()));
		}
		if(largeIcon != null && largeIcon.length() > 0)
		{
			builder.setLargeIcon(BitmapFactory.decodeResource(res, res.getIdentifier(largeIcon, "drawable", context.getPackageName())));
		}
		if(color >= 0)
		{
			builder.setColor(color);
		}

        Notification notification = builder.build();
        mng.notify(id, notification);
    }

	public static void ClearReceived()
	{
        Activity currentActivity = UnityPlayer.currentActivity;
		NotificationManager mng = (NotificationManager)currentActivity.getSystemService(Context.NOTIFICATION_SERVICE);
		mng.cancelAll();
	}

	public static void CancelPending(int[] ids)
    {
		for(int i = 0; i < ids.length; i++)
		{
			CancelPending(ids[i]);
		}
	}

    public static void CancelPending(int id)
    {
        Activity currentActivity = UnityPlayer.currentActivity;
        AlarmManager am = (AlarmManager)currentActivity.getSystemService("alarm");
        Intent intent = new Intent(currentActivity, UnityPlayerActivity.class);
        PendingIntent pendingIntent = PendingIntent.getBroadcast(currentActivity, id, intent, 0);
        am.cancel(pendingIntent);
    }

    public static void RegisterForRemote()
    {
    }
}
