using System.Collections;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Lifecycle;
using SocialPoint.Network;
using SocialPoint.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Examples.Multiplayer.Lockstep
{
    public class GameMonoBehaviour : MonoBehaviour, IPointerClickHandler, IErrorHandler, ICleanupComponent, ITeardownComponent
    {
        const string DefaultPlugin = "LockstepPlugin";
        
        [SerializeField]
        NetworkTechInstaller _networkTechInstaller;
        
        [SerializeField]
        Text _fullscreenText;

        [SerializeField]
        ClientConfig _gameConfig;

        [SerializeField]
        GameObject _gameUiContainer;

        IGameModeController _lifecycle;

        [SerializeField]
        Slider _manaSlider;

        float _resultTime;

        [SerializeField]
        GameObject _setupUiContainer;

        [SerializeField]
        GameObject _clientUiContainer;

        [SerializeField]
        GameObject _serverUiContainer;

        [SerializeField]
        GameObject _serverAndClientUiContainer;

        [SerializeField]
        GameObject _hostUiContainer;

        [SerializeField]
        GameObject _matchmakingUiContainer;

        [SerializeField]
        GameObject _serverMode;

        [SerializeField]
        Text _timeText;

        ServerSettings.HostingOption _previousOption;
        string _previousAddress;
        IUpdateScheduler _updateScheduler;

        void ICleanupComponent.Cleanup()
        {
            ShowMenuScreen();
        }

        void IErrorHandler.OnError(Error err)
        {
            Log.e("IErrorHandler.OnError " + err);
            Reset();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if(_lifecycle != null && _lifecycle.Events != null)
            {
                _lifecycle.Events.Process(new ClickInputEvent { Position = eventData.pointerPressRaycast.worldPosition });
            }
        }

        IEnumerator ITeardownComponent.Teardown()
        {
            if(_fullscreenText != null && _lifecycle != null && _lifecycle.SceneController != null)
            {
                _resultTime = 0.0f;
                var results = _lifecycle.SceneController.GetResultsAttr();
                _fullscreenText.text = string.Format("match result {0}", results);

                while(_resultTime < 1.0f)
                {
                    _resultTime += Time.deltaTime;
                    yield return null;
                }
            }
        }

        void Update()
        {
            if(_lifecycle != null && _lifecycle.SceneController != null)
            {
                var sceneCtrl = _lifecycle.SceneController;
                UpdateView(sceneCtrl.LocalPlayerManaPercent, sceneCtrl.TimeLeft);
            }
        }

        void OnDestroy()
        {
            if(_lifecycle != null)
            {
                _lifecycle.Dispose();
                _lifecycle = null;
            }
        }

        void StartGame(IGameModeController controller)
        {
            _lifecycle = controller;
            _lifecycle.RegisterComponent(this);
            OnGameStarted();
            _lifecycle.Start();
        }

        void OnGameStarted()
        {
            SetupGameScreen();
            _fullscreenText.text = string.Empty;

            if(_timeText != null)
            {
                _timeText.text = FormatTime(0);
            }
        }

        void Reset()
        {
            if(_lifecycle != null)
            {
                _lifecycle.Dispose();
                _lifecycle = null;
            }

            ShowMenuScreen();
        }

        void Start()
        {
            _updateScheduler = Services.Instance.Resolve<IUpdateScheduler>();
            if(_gameUiContainer != null)
            {
                _gameUiContainer.SetActive(false);
            }

            // If the network tech selected uses photon we cannot use the ServerAndClient mode.
            // Also, as the LocalBridge also uses photon, we want to change the values of the correct
            // config.
            // If it does not use photon, it should disable the server config controls that will be shown
            // at the bottom of the screen.
            
            bool usesPhoton = _networkTechInstaller.Settings.Tech == NetworkTechInstaller.NetworkTech.Photon;

            bool selfHosted = usesPhoton && PhotonNetwork.PhotonServerSettings.HostType == ServerSettings.HostingOption.SelfHosted;
            
            _previousOption = selfHosted ? ServerSettings.HostingOption.PhotonCloud : PhotonNetwork.PhotonServerSettings.HostType;
            _serverAndClientUiContainer.SetActive(!usesPhoton);
            _serverMode.SetActive(usesPhoton);
            OnSelfHostChanged(selfHosted);

            ShowMenuScreen();
        }

        void ShowMenuScreen()
        {
            if(_gameUiContainer != null)
            {
                _gameUiContainer.SetActive(false);
            }

            if(_setupUiContainer != null)
            {
                _setupUiContainer.SetActive(true);
            }

            _fullscreenText.text = string.Empty;
            UpdateView(0, 0);
        }

        void SetupGameScreen()
        {
            if(_gameUiContainer != null)
            {
                _gameUiContainer.SetActive(true);
            }

            if(_setupUiContainer != null)
            {
                _setupUiContainer.SetActive(false);
            }
        }

        void UpdateView(float mana, long time)
        {
            if(_manaSlider != null)
            {
                _manaSlider.value = mana;
            }

            if(_timeText != null)
            {
                _timeText.text = FormatTime(time);
            }
        }

        string FormatTime(long time)
        {
            time /= 1000;
            return string.Format("{0:D2}:{1:D2}", time / 60, time % 60);
        }

        public void OnLocalClicked()
        {
            StartGame(new LocalGameModeController(_updateScheduler, _gameConfig));
        }

        public void OnReplayClicked()
        {
            StartGame(new ReplayGameModeController(_updateScheduler, _gameConfig));
        }

        public void OnClientClicked()
        {
            StartGame(new ClientGameModeController(_updateScheduler, _gameConfig));
        }

        public void OnServerClicked()
        {
            _gameConfig.General.NumPlayers = 1;
            StartGame(new ServerGameModeController(_updateScheduler, _gameConfig));
        }

        public void OnServerAndClientClicked()
        {
            StartGame(new ServerAndClientGameModeController(_updateScheduler, _gameConfig));
        }

        public void OnHostClicked()
        {
            StartGame(new HostGameModeController(_updateScheduler, _gameConfig));
        }

        public void OnMatchClicked()
        {
            StartGame(new MatchmakingGameModeController(_updateScheduler, _gameConfig));
        }

        public void OnCloseClicked()
        {
            Reset();
        }

        public void OnSelfHostChanged(bool enabledSelfHost)
        {
            PhotonNetwork.PhotonServerSettings.HostType = enabledSelfHost ? ServerSettings.HostingOption.SelfHosted : _previousOption;

            _networkTechInstaller.Settings.Photon.Config.RoomOptions.ForcedPlugins = enabledSelfHost ? new[] {DefaultPlugin} : new string[0];
            _networkTechInstaller.Settings.Photon.Config.ForceServer = enabledSelfHost ? _previousAddress : string.Empty;

            _clientUiContainer.SetActive(!enabledSelfHost);
            _serverUiContainer.SetActive(!enabledSelfHost);
            _hostUiContainer.SetActive(!enabledSelfHost);
            _matchmakingUiContainer.SetActive(enabledSelfHost);
        }

        public void OnServerAddressChanged(string address)
        {
            PhotonNetwork.PhotonServerSettings.ServerAddress = address;
            _networkTechInstaller.Settings.Photon.Config.ForceServer = address;
            _previousAddress = address;
        }
    }
}