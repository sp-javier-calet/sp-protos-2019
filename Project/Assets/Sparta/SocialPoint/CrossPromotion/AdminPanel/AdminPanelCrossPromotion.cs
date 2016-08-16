using UnityEngine;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;

namespace SocialPoint.CrossPromotion
{
    public class AdminPanelCrossPromotion : IAdminPanelConfigurer, IAdminPanelGUI
    {
        CrossPromotionManager _xpromo;
        bool _initialized = false;

        public AdminPanelCrossPromotion(CrossPromotionManager xpromo)
        {
            _xpromo = xpromo;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("CrossPromotion", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("CrossPromotion");
            layout.CreateButton("Start", () => {
                Start();
                layout.Refresh();
            }, !_initialized);
            layout.CreateButton("Reset", () => {
                Reset();
                layout.Refresh();
            }, _initialized);
            layout.CreateButton("Open Popup", () => {
                OpenPopup();
                layout.Refresh();
            }, _initialized);
        }

        void Start()
        {
            Reset();
            _initialized = true;
            #if UNITY_ANDROID
            TextAsset data = Resources.Load("xpromo_android") as TextAsset;
            #else
            TextAsset data = Resources.Load("xpromo_ios") as TextAsset;
            #endif
            AttrDic attr = new JsonAttrParser().Parse(data.bytes).AssertDic;
            _xpromo.Init(attr.Get("xpromo").AsDic);
            _xpromo.Start();
        }

        void Reset()
        {
            _initialized = false;
            Reflection.CallPrivateVoidMethod<CrossPromotionManager>(_xpromo, "Reset");
        }

        void OpenPopup()
        {
            _xpromo.TryOpenPopup();
        }
    }
}
