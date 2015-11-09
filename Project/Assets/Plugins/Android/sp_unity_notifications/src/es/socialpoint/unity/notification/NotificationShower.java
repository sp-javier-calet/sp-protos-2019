package es.socialpoint.unity.notification;

import android.os.Bundle;
import android.app.Notification;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.support.v4.app.NotificationCompat;
import android.util.Log;
import es.socialpoint.unity.configuration.Metadata;
import es.socialpoint.unity.notification.IntentParameters.Origin;

public class NotificationShower {

    private static final String TAG = "NotificationShower";
    private static final String RES_TYPE_DRAWABLE = "drawable";
    
    private static final String SMALL_ICON = "notify_icon_small";
    private static final String DEFAULT_SMALL_ICON = "default_notify_icon_small";
    private static final String LARGE_ICON = "notify_icon_large";
    private static final String DEFAULT_LARGE_ICON = "default_notify_icon_large";
    
    private static int DEFAULT_COLOR = 0xff704b92;
    
    private Context mContext;
    private int mAlarmId;
    private String mText;
    private String mTitle;
    private Origin mOrigin;
    private Bundle mExtras;

    private NotificationShower(Context context, Bundle extras) {
        mContext = context;
        mAlarmId = 0;
        mExtras = extras;
    }
    
    public static NotificationShower create(Context context, Bundle extras) {
        return new NotificationShower(context, extras);
    }
    
    public NotificationShower setOrigin(Origin origin) {
        mOrigin = origin;
        return this;
    }
    
    public NotificationShower setTitle(String title) {
        mTitle = title;
        return this;
    }
    
    public NotificationShower setText(String text) {
        mText = text;
        return this;
    }
    
    public NotificationShower setAlarmId(int alarmId) {
        mAlarmId = alarmId;
        return this;
    }
    
    private int loadIcon(String icon, String defaultIcon) {
        Resources resources = mContext.getResources();
        int iconRes = resources.getIdentifier(icon, RES_TYPE_DRAWABLE, mContext.getPackageName());
        if(iconRes == 0) {
            iconRes = resources.getIdentifier(defaultIcon, RES_TYPE_DRAWABLE, mContext.getPackageName());
        }
        
        return iconRes;
    }
    
    public void show() {
        // Create notification builder and set default parameters and configuration
        NotificationCompat.Builder mBuilder = new NotificationCompat.Builder(mContext)
            .setColor(DEFAULT_COLOR)
            .setDefaults(Notification.DEFAULT_SOUND | Notification.DEFAULT_VIBRATE);
    
        // Set notification parameters
        if(mTitle != null) {
            mBuilder.setContentTitle(mTitle);
        }
        
        if(mText != null) {
            mBuilder.setContentText(mText);
        }
        
        int largeIconRes = loadIcon(LARGE_ICON, DEFAULT_LARGE_ICON);
        if(largeIconRes != 0) {
            Bitmap largeIcon = BitmapFactory.decodeResource(mContext.getResources(), largeIconRes);
            if(largeIcon != null) {
                mBuilder.setLargeIcon(largeIcon);
            }
        }
        
        int smallIconRes = loadIcon(SMALL_ICON, DEFAULT_SMALL_ICON);
        if(smallIconRes != 0) {
            mBuilder.setSmallIcon(smallIconRes);
        }
        
        Intent launchIntent = mContext.getPackageManager().getLaunchIntentForPackage(mContext.getPackageName());
        String gameClass = launchIntent.getComponent().getClassName();
        if(gameClass != null) {
            Intent resultIntent;
            try {
                resultIntent = new Intent(mContext, Class.forName(gameClass));
                resultIntent.putExtras(mExtras);
                resultIntent.putExtra(IntentParameters.EXTRA_ORIGIN, mOrigin.getName());
                
                PendingIntent contentIntent = PendingIntent.getActivity(mContext, 0, resultIntent, PendingIntent.FLAG_UPDATE_CURRENT);
                mBuilder.setContentIntent(contentIntent);
            } catch (ClassNotFoundException e) {
                Log.e(TAG, "Could not resolve game class name for: " + gameClass);
            }
        }
        
        NotificationManager nm = (NotificationManager) mContext.getSystemService(Context.NOTIFICATION_SERVICE);
        nm.notify(mAlarmId, mBuilder.build());
    }
}
