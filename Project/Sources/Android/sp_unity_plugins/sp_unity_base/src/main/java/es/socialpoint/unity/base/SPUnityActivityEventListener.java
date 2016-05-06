package es.socialpoint.unity.base;

import android.content.Intent;

public interface SPUnityActivityEventListener {
    void handleActivityResult(int requestCode, int resultCode, Intent data);
}
