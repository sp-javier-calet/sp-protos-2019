package es.socialpoint.unity.configuration;

import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.os.Bundle;
import android.util.Log;

public class Metadata {
    private static final String TAG = "Metadata";
    private Bundle metadata;
    
    public Metadata(Context context) {
        try {
            PackageManager manager = context.getPackageManager();
            ApplicationInfo info = manager.getApplicationInfo(context.getPackageName(), PackageManager.GET_META_DATA);
            metadata = info.metaData;
        } catch (NameNotFoundException e) {
            metadata = null;
            Log.e(TAG, "Impossible to read application info");
        }
    }
    
    @SuppressWarnings("unchecked")
    public <T> T get(String key, T defaultValue){
        if(metadata != null) {
            try {
                Object r = metadata.get(key);
                if(r != null) {
                    return (T)r;
                }
            } catch (Exception e) {
                e.printStackTrace();
                Log.e(TAG, "Error retrieving app metadata for " + key + ". Default value: " + defaultValue);
            }
        }
        return defaultValue;
    }
}
