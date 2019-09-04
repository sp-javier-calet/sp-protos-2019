using UnityEngine;

[ExecuteInEditMode]
public class FaceToCamera : MonoBehaviour
{
    void Update()
    {
        Camera usingCamera = Camera.main;
        /*
        if(Application.isPlaying && CameraManager.Instance != null)
        {
            usingCamera = CameraManager.Instance.GameCamera;
        }
        */

        if(usingCamera != null)
        {
            transform.localRotation = Quaternion.LookRotation(usingCamera.transform.forward);
        }
    }
}
