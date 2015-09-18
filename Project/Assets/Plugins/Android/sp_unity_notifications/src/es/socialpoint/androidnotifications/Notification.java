package es.socialpoint.androidnotifications;

import android.app.AlarmManager;
import android.app.IntentService;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;

public class Notification extends IntentService 
{
    private AlarmManager am = null;
    NotificationManager mNotificationManager = null;
    
    public Notification() 
    {
        super("Notification");
    }

    @Override
    protected void onHandleIntent(Intent i) 
    {   
        am = (AlarmManager) getSystemService(Context.ALARM_SERVICE);
        mNotificationManager = (NotificationManager) this.getSystemService(Context.NOTIFICATION_SERVICE);
        
        if (i.getAction().equals("send")) 
        {
            sendLocalNotification(i);
        }
        
        if (i.getAction().equals("clear")) 
        {
            clearLocalNotifications(i);
        }
        
        if (i.getAction().equals("cancel")) 
        {
            cancelLocalNotifications(i);
        }
    }
    
    public void sendLocalNotification(Intent i) 
    {
        long repeating = i.getLongExtra("repeating", 0);
        int id = i.getIntExtra("id", 0);
        long notifTime = System.currentTimeMillis() + (i.getLongExtra("time", 0)*1000);
        
        Intent intent = new Intent(this, Receiver.class);
        intent.setAction("SEND_NOTIFICATION");
        intent.putExtra("message", i.getStringExtra("message"));
        intent.putExtra("title", i.getStringExtra("title"));
        intent.putExtra("id", id);
        
        PendingIntent pendingIntent = null;
        if (repeating == 0)
        {
            pendingIntent = PendingIntent.getBroadcast(this, id, intent, 0);
            am.set(AlarmManager.RTC_WAKEUP, notifTime, pendingIntent);
        }
        else 
        {
            pendingIntent = PendingIntent.getBroadcast(this, id, intent, 0);
            am.setRepeating(AlarmManager.RTC_WAKEUP, notifTime, (repeating*1000), pendingIntent);
        }
    }
    
    public void clearLocalNotifications(Intent i)
    {
    }   
    
    public void cancelLocalNotifications(Intent i)
    {
        int[] idList = i.getIntArrayExtra("idlist");

        for (int x = 0; x < idList.length; x++)
        {
            Intent intent = new Intent(this, Receiver.class);
            PendingIntent pendingIntent = PendingIntent.getBroadcast(this, idList[x], intent, 0);
            am.cancel(pendingIntent);
        }
        
        // Cancel all previously shown notifications
        mNotificationManager.cancelAll();
    }    
}