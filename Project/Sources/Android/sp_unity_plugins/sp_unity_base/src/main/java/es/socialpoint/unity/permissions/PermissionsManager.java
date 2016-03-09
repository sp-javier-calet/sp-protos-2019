package es.socialpoint.unity.permissions;

import es.socialpoint.unity.base.R;
import android.app.Activity;
import android.app.AlarmManager;
import android.app.AlertDialog;
import android.app.PendingIntent;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.net.Uri;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.util.Log;

public enum PermissionsManager {
	instance;

	private static final String TAG = "PermissionsServices";
	protected static final int PERMISSIONS_REQUEST_ALL = 0;

	AlertDialog currentDialog = null;

	public boolean checkPermissions(Activity activity)
	{
        if(currentDialog != null)
		{
			currentDialog.dismiss();
			currentDialog = null;
		}

		if(hasPermissions(activity))
		{
			return true;
		}

		askPermissions(activity);
		return false;
	}

	boolean hasPermissions(Activity activity)
	{
		if(activity == null)
		{
			return false;
		}

		/* This shouldn't be needed as ContextCompat takes cares of all versions, but if for
		 * some reason a permission is added to the manifest with api level higher than the
		 * minimum one, ContextCompat.checkSelfPermission will return false. This happened with
		 * the permission READ_EXTERNAL_STORAGE that has api level 16 requirements while we had
		 * minimum api level 14.
		 */
		if(android.os.Build.VERSION.SDK_INT < 23)
		{
			return true;
		}

		boolean allGranted = true;
		try
		{
			PackageInfo packageInfo = activity.getPackageManager().getPackageInfo(activity.getPackageName(), PackageManager.GET_PERMISSIONS);
			for (String permission : packageInfo.requestedPermissions)
			{
				allGranted &= ContextCompat.checkSelfPermission(activity, permission) == PackageManager.PERMISSION_GRANTED;
			}
		}
		catch (NameNotFoundException e)
		{
			allGranted = false;
		}

		return allGranted;
	}

	void askPermissions(Activity activity)
	{
		try 
		{
			PackageInfo packageInfo = activity.getPackageManager().getPackageInfo(activity.getPackageName(), PackageManager.GET_PERMISSIONS);
			ActivityCompat.requestPermissions(activity, packageInfo.requestedPermissions, PERMISSIONS_REQUEST_ALL);
		}
		catch (NameNotFoundException e) 
		{

		}
	}

	public boolean onRequestPermissionsResult(Activity activity, int requestCode, String permissions[], int[] grantResults)
	{
		switch (requestCode)
		{
		case PERMISSIONS_REQUEST_ALL:
		{
			if(grantResults.length > 0)
			{
				boolean allGranted = true;
				boolean permissionBlocked = true; 
				for(int i = 0; i < grantResults.length; ++i)
				{
                    try
                    {
                        permissionBlocked &= !ActivityCompat.shouldShowRequestPermissionRationale(activity, permissions[i]);
                        allGranted &= grantResults[i] == PackageManager.PERMISSION_GRANTED;
                    }
                    catch (IllegalArgumentException e)
                    {

                    }
				}

				if(allGranted)
				{
					return true;
				}
				else if(permissionBlocked)
				{
					showEnablePermissionsInSettingsDialog(activity);
					return false;
				}
				else
				{
					showPermissionsNeededDialog(activity);
					return false;
				}
			}
		}
		}
		return false;
	}

	void showPermissionsNeededDialog(final Activity activity)
	{
		currentDialog = new AlertDialog.Builder(activity)
		.setTitle(R.string.accept_permissions_title)
		.setMessage(R.string.accept_permissions_message)
		.setCancelable(false)
		.setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() { 
			public void onClick(DialogInterface arg0, int arg1) {
				askPermissions(activity);
			} 
		})
		.show();
	}

	void showEnablePermissionsInSettingsDialog(final Activity activity)
	{
		currentDialog = new AlertDialog.Builder(activity)
		.setTitle(R.string.accept_permissions_settings_title)
		.setMessage(R.string.accept_permissions_settings_message)
		.setCancelable(false)
		.setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() { 
			public void onClick(DialogInterface arg0, int arg1) {
				Intent intent = new Intent(android.provider.Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
				intent.setData(Uri.parse("package:" + activity.getPackageName()));
				activity.startActivity(intent);
			} 
		})
		.show();
	}

	public void checkPermissionsOrRestart(Activity activity)
	{
		if(!hasPermissions(activity))
		{
			Intent permissionsIntent = new Intent(activity, PermissionsActivity.class);
			PendingIntent pendingIntent = PendingIntent.getActivity(activity, 0, permissionsIntent, permissionsIntent.getFlags());
			AlarmManager manager = (AlarmManager)activity.getSystemService(Context.ALARM_SERVICE);
			manager.set(AlarmManager.RTC, System.currentTimeMillis() + 100, pendingIntent);
			android.os.Process.killProcess(android.os.Process.myPid());
		}
	}
}
