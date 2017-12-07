package es.socialpoint.unity.base;

import android.app.Activity;
import android.app.ActivityManager;
import android.content.Context;
import android.os.Build;

import java.io.IOException;

/**
 * Created by abarrera on 30/11/2017.
 */

public class NativeUtils {

    private static Context context;

    public static void Init(Context context)
    {
        NativeUtils.context = context;
    }

    public static boolean ClearDataAndKillApp()
    {
        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT)
        {
            return ((ActivityManager) context.getSystemService(Activity.ACTIVITY_SERVICE)).clearApplicationUserData();
        }
        else
        {
            try
            {
                Runtime.getRuntime().exec("pm clear "+ context.getPackageName());
            }
            catch (IOException e)
            {
                e.printStackTrace();
                return false;
            }
            return true;
        }
    }
}
