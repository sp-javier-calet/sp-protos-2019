using UnityEngine;
using System.Collections;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;


namespace SocialPoint.CrossPromotion
{
    public class SocialPointCrossPromoManager : CrossPromotionManager
    {
        public SocialPointCrossPromoManager(ICoroutineRunner coroutineRunner, IAppEvents appEvent, IEventTracker eventTracker) : base(coroutineRunner)
        {
            //Initialize CrossPromotionManager
            AppEvents = appEvent;
            TrackSystemEvent = eventTracker.TrackSystemEvent;
            TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
            CreateIcon = CreateButtonCrossPromo;
            CreatePopup = CreatePopupCrossPromo;
        }

        private static void CreateButtonCrossPromo()
        {
            //TODO
        }

        private static void CreatePopupCrossPromo()
        {
            //TODO: Use a UIController??
            GameObject prefab = Resources.Load("PopupCrossPromo") as GameObject;
            /*GameObject obj = */
            GameObject.Instantiate(prefab)/* as GameObject*/;
            //DLPopupCrossPromoController vc = obj.AddComponent<DLPopupCrossPromoController>();
            //vc.Init();
            //return vc;
        }
    }
}