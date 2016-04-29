package es.socialpoint.unity.permissions;

import android.Manifest;
import android.annotation.SuppressLint;
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

import java.util.ArrayList;
import java.util.HashSet;

import es.socialpoint.unity.base.R;

public enum PermissionsManager {
    instance;

    protected static final int PERMISSIONS_REQUEST_ONE = 0;

    private AlertDialog currentDialog = null;

    HashSet<String> blockedPermissionsHashSet = new HashSet<>();

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
        ArrayList<String> permissionsToRequest = getPermissionsToRequest(activity);

        if(!permissionsToRequest.isEmpty())
        {
            // we request the first permission not granted
            String[] permissionToRequest = {permissionsToRequest.get(0)};
            ActivityCompat.requestPermissions(activity, permissionToRequest, PERMISSIONS_REQUEST_ONE);
        }
    }

    @SuppressLint("NewApi") // to suppress error due to requestedPermissionsFlags requires API level 16
    ArrayList<String> getPermissionsToRequest(Activity activity)
    {
        ArrayList<String> permissionsToRequest = new ArrayList<>();

        try
        {
            PackageInfo packageInfo = activity.getPackageManager().getPackageInfo(activity.getPackageName(), PackageManager.GET_PERMISSIONS);
            for(int i = 0, rpfLength = packageInfo.requestedPermissionsFlags.length; i < rpfLength; ++i)
            {
                if(packageInfo.requestedPermissionsFlags[i] == 1) // 1 not granted, 3 granted
                {
                    String permission = packageInfo.requestedPermissions[i];
                    if(blockedPermissionsHashSet.contains(permission))
                    {
                        continue;
                    }
                    permissionsToRequest.add(permission);
                }
            }
        }
        catch (NameNotFoundException ignored)
        {

        }

        return permissionsToRequest;
    }

    public boolean onRequestPermissionsResult(Activity activity, int requestCode, String permissions[], int[] grantResults)
    {
        switch (requestCode)
        {
            case PERMISSIONS_REQUEST_ONE:
            {
                if(grantResults.length > 0)
                {
                    boolean granted = grantResults[0] == PackageManager.PERMISSION_GRANTED;
                    if(!granted)
                    {
                        String permission = permissions[0];
                        boolean shouldShowRequestPermissionsRationale = ActivityCompat.shouldShowRequestPermissionRationale(activity, permission);
                        if(shouldShowRequestPermissionsRationale)
                        {
                            showPermissionRationaleDialog(activity, permission);
                            return false;
                        }
                        else
                        {
                            blockedPermissionsHashSet.add(permission);
                            int permissionsToRequestSize = getPermissionsToRequest(activity).size();
                            if(permissionsToRequestSize == 0 && !hasPermissions(activity))
                            {
                                showEnablePermissionsInSettingsDialog(activity);
                                return false;
                            }
                            else
                            {
                                askPermissions(activity);
                                return false;
                            }
                        }
                    }
                    if(hasPermissions(activity))
                    {
                        return true;
                    }

                    askPermissions(activity);
                    return false;
                }
            }
        }
        return false;
    }

    void showPermissionRationaleDialog(final Activity activity, String permission)
    {
        int title;
        int message;
        switch(permission)
        {
            case Manifest.permission.WRITE_EXTERNAL_STORAGE:
            case Manifest.permission.READ_EXTERNAL_STORAGE:
                title = R.string.accept_permission_read_write_title;
                message = R.string.accept_permission_read_write_message;
                break;
            case Manifest.permission.GET_ACCOUNTS:
                title = R.string.accept_permission_get_accounts_title;
                message = R.string.accept_permission_get_accounts_message;
                break;
            default:
                title = R.string.accept_permissions_title;
                message = R.string.accept_permissions_message;
                break;
        }
        currentDialog = new AlertDialog.Builder(activity)
                .setTitle(title)
                .setMessage(message)
                .setCancelable(false)
                .setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
                    public void onClick(DialogInterface arg0, int arg1) {
                        if(!hasPermissions(activity))
                        {
                            askPermissions(activity);
                        }
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
