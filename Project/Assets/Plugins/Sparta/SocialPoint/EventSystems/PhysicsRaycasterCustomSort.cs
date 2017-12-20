using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.EventSystems
{
    [RequireComponent(typeof(Camera))]
    public sealed class PhysicsRaycasterCustomSort : PhysicsRaycaster
    {
        enum SortType
        {
            Priority,
            DistanceToCenter,
            Distance
        }

        [SerializeField]
        SortType _sortType;

        Ray _ray;

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if(eventCamera == null)
                return;

            _ray = eventCamera.ScreenPointToRay(eventData.position);
            float dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;

            // in PhysicsRaycaster from 5.3 they are still using RaycastAll instead of RaycastNonAlloc
            // https://bitbucket.org/Unity-Technologies/ui/src/b5f9aae6ff7c2c63a521a1cb8b3e3da6939b191b/UnityEngine.UI/EventSystem/Raycasters/PhysicsRaycaster.cs?at=5.3&fileviewer=file-view-default
            var hits = UnityEngine.Physics.RaycastAll(_ray, dist, finalEventMask);

            if(hits.Length > 1)
            {
                Array.Sort(hits, GetComparer());
            }

            if(hits.Length != 0)
            {
                // just appending the first hit
                RaycastHit hit = hits[0];
                var result = new RaycastResult {
                    gameObject = hit.collider.gameObject,
                    module = this,
                    distance = hit.distance,
                    worldPosition = hit.point,
                    worldNormal = hit.normal,
                    screenPosition = eventData.position,
                    index = resultAppendList.Count,
                    sortingLayer = 0,
                    sortingOrder = 0
                };
                resultAppendList.Add(result);
            }
        }

        Comparison<RaycastHit> GetComparer()
        {
            switch(_sortType)
            {
            case SortType.Priority:
                return RaycastHitPriorityComparer;
            case SortType.DistanceToCenter:
                return RaycastHitDistanceToCenterComparer;
            default:
                return RaycastHitDistanceComparer;
            }
        }

        static int RaycastPriority(RaycastHit raycastHit)
        {
            int rayPriority = -1;
            var go = raycastHit.collider.gameObject;
            var priorizable = go.GetComponent<ITouchPrioritizable>();
            if(priorizable != null)
            {
                rayPriority = priorizable.TouchPriority;
            }
            return rayPriority;
        }

        float RaycastDistance(RaycastHit raycastHit)
        {
            var go = raycastHit.collider.gameObject;
            var point = go.transform.position;
            var distance = Vector3.Cross(_ray.direction, point - _ray.origin).magnitude;
            return distance;
        }

        static int RaycastHitPriorityComparer(RaycastHit lhs, RaycastHit rhs)
        {
            int lhsPriority = RaycastPriority(lhs);
            int rhsPriority = RaycastPriority(rhs);
            return rhsPriority.CompareTo(lhsPriority); //from high to low
        }

        int RaycastHitDistanceToCenterComparer(RaycastHit lhs, RaycastHit rhs)
        {
            float lhsPriority = RaycastDistance(lhs);
            float rhsPriority = RaycastDistance(rhs);
            return lhsPriority.CompareTo(rhsPriority); //from low to high
        }

        static int RaycastHitDistanceComparer(RaycastHit lhs, RaycastHit rhs)
        {
            return lhs.distance.CompareTo(rhs.distance); //from low to high
        }

    }
}
