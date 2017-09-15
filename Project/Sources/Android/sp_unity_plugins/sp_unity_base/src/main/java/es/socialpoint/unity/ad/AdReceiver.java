package es.socialpoint.unity.ad;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import com.appsflyer.SingleInstallBroadcastReceiver;

public class AdReceiver extends BroadcastReceiver {

    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d("AdReceiver", "" + intent);

        // AppsFlyer
        SingleInstallBroadcastReceiver singleInstallBroadcastReceiver = new SingleInstallBroadcastReceiver();
        singleInstallBroadcastReceiver.onReceive(context, intent);
    }
}
