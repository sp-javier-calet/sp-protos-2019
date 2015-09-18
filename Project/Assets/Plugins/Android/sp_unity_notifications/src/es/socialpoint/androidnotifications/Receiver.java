package es.socialpoint.androidnotifications;

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
import android.os.Build;
import android.support.v4.app.NotificationCompat;

//import com.unity3d.player.UnityPlayer;
//import com.unity3d.player.UnityPlayerNativeActivity;

public class Receiver extends BroadcastReceiver 
{   
    @Override
    public void onReceive(Context context, Intent intent) 
    {       
        // sending a notification
        if (intent.getAction().equals("SEND_NOTIFICATION"))
        {
            String unityClass = intent.getStringExtra("unityClass");
            String title = intent.getStringExtra("title");
            String message = intent.getStringExtra("message");
            String ticker = intent.getStringExtra("ticker");
            String s_icon = intent.getStringExtra("s_icon");
            String l_icon = intent.getStringExtra("l_icon");
            // int color = intent.getIntExtra("color", 0);
            // Boolean sound = Boolean.valueOf(intent.getBooleanExtra("sound", false));
            // Boolean vibrate = Boolean.valueOf(intent.getBooleanExtra("vibrate", false));
            // Boolean lights = Boolean.valueOf(intent.getBooleanExtra("lights", false));
            int id = intent.getIntExtra("id", 0);

            if(unityClass == null)
                unityClass = "es.socialpoint.unity.base.SPUnityActivity";

            if(ticker == null)
                ticker = message;
            
            if(s_icon == null)
                s_icon = "notification";

            if(l_icon == null)
                l_icon = "app_icon";
            
            Resources res = context.getResources();
            Class<?> unityClassActivity = null;
            try {
                unityClassActivity = Class.forName(unityClass);
            } catch (ClassNotFoundException e) {
                e.printStackTrace();
            }
            Intent notificationIntent = new Intent(context, unityClassActivity);
            notificationIntent.putExtra("sp_notification", title);
            notificationIntent.putExtra("alarmId", id);
            PendingIntent contentIntent = PendingIntent.getActivity(context, 0, notificationIntent, PendingIntent.FLAG_UPDATE_CURRENT);
            
            NotificationCompat.InboxStyle notificationInbox = new NotificationCompat.InboxStyle()
            .setSummaryText(title)
            .setBigContentTitle(title);

            NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
            .setContentTitle(title)
            .setContentText(message)         
            .setContentIntent(contentIntent)
            .setVisibility(Notification.VISIBILITY_PUBLIC)
            .setPriority(Notification.PRIORITY_HIGH)
            .setDefaults(Notification.DEFAULT_ALL);

            // Notification.Builder builder = new Notification.Builder(context);  
            // builder.setContentIntent(contentIntent)
            //     .setWhen(System.currentTimeMillis())
            //     .setAutoCancel(true)
            //     .setContentTitle(title)
            //     .setContentText(message);
            
            // if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP)
            //     builder.setColor(color);
            
            // if(ticker != null && ticker.length() > 0)
            //     builder.setTicker(ticker);
                   
            if (s_icon != null && s_icon.length() > 0)
                builder.setSmallIcon(res.getIdentifier(s_icon, "drawable", context.getPackageName()));
            
            if (l_icon != null && l_icon.length() > 0)
                builder.setLargeIcon(BitmapFactory.decodeResource(res, res.getIdentifier(l_icon, "drawable", context.getPackageName())));
            
            // if(sound.booleanValue())
            //     builder.setSound(RingtoneManager.getDefaultUri(2));
            
            // if(vibrate.booleanValue())
            //     builder.setVibrate(new long[] {
            //         1000L, 1000L
            //     });
            
            // if(lights.booleanValue())
            //     builder.setLights(Color.GREEN, 3000, 3000);
            
            NotificationManager notificationManager = (NotificationManager)context.getSystemService(Context.NOTIFICATION_SERVICE);
            Notification notification = builder.build();
            notificationManager.notify(id, notification);
        }
    }
}