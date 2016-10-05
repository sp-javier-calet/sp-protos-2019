using SocialPoint.GUIControl;
using SocialPoint.Utils;
using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.AdminPanel
{
    public interface IPanelController
    {
        void RefreshPanel();

        void OpenPanel(IAdminPanelGUI panel);

        void ReplacePanel(IAdminPanelGUI panel);

        void ClosePanel();

        void RegisterUpdateable(IUpdateable updateable);

        void UnregisterUpdateable(IUpdateable updateable);
    }

    public abstract class BasePanelController : UIViewController, IPanelController
    {
        void OnLevelWasLoaded(int i)
        {
            Hide();
        }

        void Update()
        {
            for(var i = 0; i < _updateables.Count; i++)
            {
                _updateables[i].Update();
            }
        }

        public abstract void RefreshPanel();

        public abstract void OpenPanel(IAdminPanelGUI panel);

        public abstract void ReplacePanel(IAdminPanelGUI panel);

        public abstract void ClosePanel();

        List<IUpdateable> _updateables = new List<IUpdateable>();

        public void RegisterUpdateable(IUpdateable updateable)
        {
            if(updateable != null && !_updateables.Contains(updateable))
            {
                _updateables.Add(updateable);
            }
        }

        public void UnregisterUpdateable(IUpdateable updateable)
        {
            _updateables.Remove(updateable);
        }
    }
}
