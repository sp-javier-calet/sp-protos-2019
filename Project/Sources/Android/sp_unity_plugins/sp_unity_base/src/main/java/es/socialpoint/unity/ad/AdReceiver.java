package es.socialpoint.unity.ad;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;

import com.appsflyer.AppsFlyerLib;
import com.mobileapptracker.Tracker;
public class AdReceiver extends BroadcastReceiver {

    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d("AdReceiver", "" + intent);
        AppsFlyerLib.getInstance().onReceive(context, intent);
        Tracker receiver = new Tracker();
        receiver.onReceive(context,intent);
    }
}
