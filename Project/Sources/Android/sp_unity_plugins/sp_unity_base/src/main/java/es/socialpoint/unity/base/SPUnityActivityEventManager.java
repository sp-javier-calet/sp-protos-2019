package es.socialpoint.unity.base;

import android.content.Intent;

import java.util.ArrayList;
import java.util.List;

import es.socialpoint.unity.base.SPUnityActivityEventListener;

public class SPUnityActivityEventManager
{
    private static List<SPUnityActivityEventListener> _listeners = new ArrayList<SPUnityActivityEventListener>();

    public static void Register(SPUnityActivityEventListener listener)
    {
        if(!_listeners.contains(listener))
        {
            _listeners.add(listener);
        }
    }

    public static void Unregister(SPUnityActivityEventListener listener)
    {
        _listeners.remove(listener);
    }

    public static void HandleActivityResult(int requestCode, int resultCode, Intent data)
    {
        for (SPUnityActivityEventListener listener : _listeners)
        {
            if(listener != null)
            {
                listener.HandleActivityResult(requestCode, resultCode, data);
            }
        }
    }
}
