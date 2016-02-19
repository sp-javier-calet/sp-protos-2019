using UnityEngine;

namespace SocialPoint.Base
{
    public static class CameraExtension
    {
        static Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);

        public static Vector3 WorldPosFromScreenPos(this Camera camera, Vector2 screenPos)
        {
            var worldPos = new Vector3(0, 0, 0);

            Ray ray = camera.ScreenPointToRay(screenPos);
            float rayDistance;
            if(_groundPlane.Raycast(ray, out rayDistance))
            {
                worldPos = ray.GetPoint(rayDistance);
            }
            return worldPos;
        }

        public static Vector3 WorldPosFromScreenNormalized(this Camera camera, Vector2 screenPos)
        {
            float fovDegree = camera.fieldOfView;

            float fovRad = fovDegree * Mathf.Deg2Rad;
            float fovRadHalf = fovRad * 0.5f;

            float h = Mathf.Tan(fovRadHalf);

            float ar = (float)Screen.width / (float)Screen.height;
            float w = h * ar;

            Vector3 dir = camera.transform.forward.normalized + (camera.transform.right.normalized * w) * screenPos.x + (camera.transform.up.normalized * h) * screenPos.y;
            dir.Normalize();

            var worldPos = new Vector3(0, 0, 0);
            var ray = new Ray(camera.transform.position, dir);
            float rayDistance;

            if(_groundPlane.Raycast(ray, out rayDistance))
            {
                worldPos = ray.GetPoint(rayDistance);
            }
            return worldPos;
        }
    }
}