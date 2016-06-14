package es.socialpoint.unity.base;

import android.content.Intent;

import java.util.ArrayList;
import java.util.List;

public class SPUnityActivityEventManager
{
    private static List<SPUnityActivityEventListener> _listeners = new ArrayList<SPUnityActivityEventListener>();

    public static void register(SPUnityActivityEventListener listener)
    {
        if(!_listeners.contains(listener))
        {
            _listeners.add(listener);
        }
    }

    public static void unregister(SPUnityActivityEventListener listener)
    {
        _listeners.remove(listener);
    }

    public static void handleActivityResult(int requestCode, int resultCode, Intent data)
    {
        for (SPUnityActivityEventListener listener : _listeners)
        {
            if(listener != null)
            {
                listener.handleActivityResult(requestCode, resultCode, data);
            }
        }
    }
}
