using UnityEngine;

namespace SocialPoint.Base
{
    public static class CanvasExtensions
    {
        public static Camera GetCamera(this Canvas canvas)
        {
            switch(canvas.renderMode)
            {
            case RenderMode.ScreenSpaceOverlay:
                //If we send back a camera when set to screen space overlay, the coordinates will not be accurate. If we return null, they will be.
                return null;

            case RenderMode.ScreenSpaceCamera:
                //If it's set to screen space we use the world Camera that the Canvas is using.
                //If it doesn't have one set, however, we have to send back the current camera. otherwise the coordinates will not be accurate.
                return (canvas.worldCamera) ? canvas.worldCamera : Camera.main;

            default:
            case RenderMode.WorldSpace:
                //World space always uses the current camera.
                return Camera.main;
            }
        }
    }
}