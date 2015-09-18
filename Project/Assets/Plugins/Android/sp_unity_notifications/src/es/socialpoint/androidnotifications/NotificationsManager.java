package es.socialpoint.androidnotifications;

import android.content.Context;
import android.content.Intent;

public class NotificationsManager
{   
    public static void CreateLocalNotification(Context context, String title, String message, long time, int id, long repeating) 
    {   
        Intent intent = new Intent(context, Notification.class);
        intent.setAction("send");
        intent.putExtra("title", title);
        intent.putExtra("message", message);
        intent.putExtra("time", time);
        intent.putExtra("id", id);
        intent.putExtra("repeating", repeating);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        context.startService(intent);
    }
    
    public static void ClearLocalNotifications(Context context) 
    {
        Intent intent = new Intent(context,Notification.class);
        intent.setAction("clear");
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        context.startService(intent);
    }
    
    public static void CancelLocalNotification(Context context, int id) 
    {
        int[] idList = {id};
        CancelAllLocalNotifications(context,idList);
    }
    
    public static void CancelAllLocalNotifications(Context context, int[] idList) 
    {
        Intent intent = new Intent(context,Notification.class);
        intent.setAction("cancel");
        intent.putExtra("idlist", idList);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        context.startService(intent);
    }

}