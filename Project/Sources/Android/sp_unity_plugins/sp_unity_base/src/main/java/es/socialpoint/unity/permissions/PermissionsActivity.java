package es.socialpoint.unity.permissions;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

import es.socialpoint.unity.configuration.Metadata;

public class PermissionsActivity extends Activity {
	Metadata metadata;

	@Override
	protected void onCreate(Bundle savedInstanceState) 
	{
		super.onCreate(savedInstanceState);
		metadata = new Metadata(this);
	}

	@Override
	public void onStart()
	{
		super.onStart();
		boolean permissionsGranted = PermissionsManager.instance.checkPermissions(this);
		if(permissionsGranted)
		{
			launchGame();
		}
	}

	@Override
	public void onRequestPermissionsResult(int requestCode, String permissions[], int[] grantResults)
	{
		boolean permissionsGranted = PermissionsManager.instance.onRequestPermissionsResult(this, requestCode, permissions, grantResults);
		if(permissionsGranted)
		{
			launchGame();
		}
	}

	void launchGame()
	{
		Intent intent = new Intent(this, es.socialpoint.unity.base.SPUnityActivity.class);
		Intent currentIntent = getIntent();
		if(currentIntent != null)
		{
			Bundle currentExtras = currentIntent.getExtras();
			if(currentExtras != null)
			{
				intent.putExtras(currentExtras);
			}
		}
		startActivity(intent);
		finish();
	}
}
