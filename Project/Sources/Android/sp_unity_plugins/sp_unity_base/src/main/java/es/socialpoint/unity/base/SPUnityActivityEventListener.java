package es.socialpoint.unity.base;

import android.content.Intent;

public interface SPUnityActivityEventListener {
    void HandleActivityResult(int requestCode, int resultCode, Intent data);
}
