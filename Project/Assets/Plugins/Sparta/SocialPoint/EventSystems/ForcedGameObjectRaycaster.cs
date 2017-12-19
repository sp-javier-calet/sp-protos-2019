using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    public sealed class ForcedGameObjectRaycaster : BaseRaycaster
    {
        public GameObject RaycastResultGameObject { get; set; }

        public PointerEventData LastEventData { get; private set; }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            LastEventData = eventData;
            if(RaycastResultGameObject != null)
            {
                resultAppendList.Add(new RaycastResult {
                    gameObject = RaycastResultGameObject,
                    module = this,
                    distance = 0,
                    index = resultAppendList.Count
                });
            }
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