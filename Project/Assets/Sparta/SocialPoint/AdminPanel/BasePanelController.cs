#if ADMIN_PANEL

using SocialPoint.GUIControl;
using SocialPoint.Utils;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Hide();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected override void OnDisappeared()
        {
            base.OnDisappeared();
            SceneManager.sceneLoaded -= OnSceneLoaded;
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

        readonly List<IUpdateable> _updateables = new List<IUpdateable>();

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

        protected void NotifyOpenedPanel(IAdminPanelGUI gui)
        {
            var managed = gui as IAdminPanelManagedGUI;
            if(managed != null)
            {
                managed.OnOpened();
            }
        }

        protected void NotifyClosedPanel(IAdminPanelGUI gui)
        {
            var managed = gui as IAdminPanelManagedGUI;
            if(managed != null)
            {
                managed.OnClosed();
            }
        }
    }
}

#endif