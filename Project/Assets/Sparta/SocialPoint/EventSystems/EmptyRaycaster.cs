using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public class EmptyRaycaster : BaseRaycaster
    {
        public override void Raycast(PointerEventData eventData, System.Collections.Generic.List<RaycastResult> resultAppendList)
        {
        }

        public override UnityEngine.Camera eventCamera
        {
            get
            {
                return null;
            }
        }
    }
}
