using NUnit.Framework;
using SocialPoint.Utils;
using System.Collections.Generic;
using System.Threading;

namespace SocialPoint.Network
{
    [TestFixture]
    [Category("SocialPoint.Network")]
    public abstract class BaseSocketNetworkTests : BaseNetworkTests, INetworkClientDelegate, INetworkServerDelegate
    {
        const int SleepThread = 10;
        protected UpdateScheduler _scheduler;

        List<NetworkDelegateType> _currentEvents = new List<NetworkDelegateType>();

        virtual protected void SetUp()
        {
            _scheduler = new UpdateScheduler();
        }

        override protected void WaitForEvents(params NetworkDelegateType[] typeEvent)
        {
            if(typeEvent.Length == 0)
            {
                var i = 0;
                while(i++ < 10)
                {
                    _scheduler.Update(1.0f, 1.0f);
                    Thread.Sleep(SleepThread);
                }
            }
            else
            {
                var i = 0;

                bool delegateCalled = false;
                List<NetworkDelegateType> eventsList = new List<NetworkDelegateType>(typeEvent);
                List<NetworkDelegateType> result = new List<NetworkDelegateType>();
                while(!delegateCalled && i++ < 100)
                {
                    _currentEvents.IntersectedElements(eventsList, result);
                    delegateCalled = result.Count > 0 && result.Count == typeEvent.Length ? true : false;
                    _scheduler.Update(1.0f, 1.0f);
                    Thread.Sleep(SleepThread);
                }
            }
            _currentEvents.Clear();
        }

        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
            if(!_currentEvents.Contains(NetworkDelegateType.ClientConnected1))
            {
                _currentEvents.Add(NetworkDelegateType.ClientConnected1);
            }
            if(!_currentEvents.Contains(NetworkDelegateType.ClientConnected2))
            {
                _currentEvents.Add(NetworkDelegateType.ClientConnected2);
            }
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            if(!_currentEvents.Contains(NetworkDelegateType.ClientDisconnected1))
            {
                _currentEvents.Add(NetworkDelegateType.ClientDisconnected1);
            }
            if(!_currentEvents.Contains(NetworkDelegateType.ClientDisconnected2))
            {
                _currentEvents.Add(NetworkDelegateType.ClientDisconnected2);
            }
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {

            if(!_currentEvents.Contains(NetworkDelegateType.MessageClientReceived1))
            {
                _currentEvents.Add(NetworkDelegateType.MessageClientReceived1);
            }
            if(!_currentEvents.Contains(NetworkDelegateType.MessageClientReceived2))
            {
                _currentEvents.Add(NetworkDelegateType.MessageClientReceived2);
            }
        }

        void INetworkClientDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
        }

        #endregion

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnServerStarted()
        {
            if(!_currentEvents.Contains(NetworkDelegateType.ServerStarted))
            {
                _currentEvents.Add(NetworkDelegateType.ServerStarted);
            }
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            if(!_currentEvents.Contains(NetworkDelegateType.ServerStopped))
            {
                _currentEvents.Add(NetworkDelegateType.ServerStopped);
            }
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            if(!_currentEvents.Contains(NetworkDelegateType.ClientConnectedInServer))
            {
                _currentEvents.Add(NetworkDelegateType.ClientConnectedInServer);
            }
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            if(!_currentEvents.Contains(NetworkDelegateType.ClientDisconectedInServer))
            {
                _currentEvents.Add(NetworkDelegateType.ClientDisconectedInServer);
            }
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
            if(!_currentEvents.Contains(NetworkDelegateType.MessageServerReceived))
            {
                _currentEvents.Add(NetworkDelegateType.MessageServerReceived);
            }
        }

        void INetworkServerDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
        }

        #endregion
    }
}
