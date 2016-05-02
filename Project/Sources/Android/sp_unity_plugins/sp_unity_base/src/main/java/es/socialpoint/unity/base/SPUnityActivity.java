package es.socialpoint.unity.base;

import java.io.UnsupportedEncodingException;
import java.net.URLEncoder;
import java.util.Set;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import android.os.Looper;
import android.os.Handler;

import es.socialpoint.unity.permissions.PermissionsManager;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class SPUnityActivity extends UnityPlayerActivity {

	private static final String TAG = "SPUnityActivity";

	/* Provides global access to the current acivity without the unity3d package dependency */
	public static Activity getCurrentActivity() {
		return UnityPlayer.currentActivity;
	}

	private String mApplicationSource;
	private Intent mLastIntent;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		PermissionsManager.instance.checkPermissionsOrRestart(this);
		super.onCreate(savedInstanceState);
		storeSourceFromIntent(getIntent());
		UnityGameObject.Init(this);
	}

	// SocialPoint code
	@Override
	protected void onNewIntent(Intent intent) {
		super.onNewIntent(intent);
		setIntent(intent);
		storeSourceFromIntent(intent);
	}

	public String collectApplicationSource() {
		/* Application source is cleaned after collect.
		 * Otherwise, the source will be repeated when the app comes to foreground
		 * from task manager or screen block */
		String currentSource = mApplicationSource;
		mApplicationSource = "";
		return currentSource;
	}

	private String urlEncode(String s)
	{
		String encodedUrl = s;
		try
		{
			encodedUrl = URLEncoder.encode(s, "utf-8");
		}
		catch(UnsupportedEncodingException e)
		{
			Log.e(TAG, "Source parameters encoding error", e);
		}
		return encodedUrl;
	}

	public static void UnitySendMessage(final String object, final String method, final String parameter)
	{
		Log.e(TAG, "UnitySendMessage "+object+" "+method+" "+parameter);
		new Handler(getCurrentActivity().getMainLooper()).post(new Runnable(){
			public void run(){
				UnityPlayer.UnitySendMessage(object, method, parameter);
			}
		});
	}

	private void storeSourceFromIntent(Intent intent)
	{
		if(intent != mLastIntent)
		{
			mApplicationSource = "";

			String uri = intent.getDataString();
            if(uri != null && uri != "")
            {
            	mApplicationSource = uri;
			}
			else
			{
				Bundle extras = null;

				if(intent != null)
				{
				    extras = intent.getExtras();
				}
				if(extras != null)
				{
				    String extrasStr = "";
				    Set<String> keys = extras.keySet();
				    for(String key : keys)
				    {
				        extrasStr += urlEncode(key) + "=" + urlEncode(extras.get(key).toString()) + "&";
				    }

				    mApplicationSource = extrasStr;
				}
			}

			mLastIntent = intent;
		}
	}
}
