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
            _currentEvents.Clear();
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

        void AddCurrentEvent(NetworkDelegateType ev)
        {
            if(!_currentEvents.Contains(ev))
            {
                _currentEvents.Add(ev);
            }
        }


        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
            AddCurrentEvent(NetworkDelegateType.ClientConnected1);
            AddCurrentEvent(NetworkDelegateType.ClientConnected2);
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            AddCurrentEvent(NetworkDelegateType.ClientDisconnected1);
            AddCurrentEvent(NetworkDelegateType.ClientDisconnected2);
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
            AddCurrentEvent(NetworkDelegateType.MessageClientReceived1);
            AddCurrentEvent(NetworkDelegateType.MessageClientReceived2);
        }

        void INetworkClientDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
        }

        #endregion

        #region INetworkServerDelegate implementation

        void INetworkServerDelegate.OnServerStarted()
        {
            AddCurrentEvent(NetworkDelegateType.ServerStarted);
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            AddCurrentEvent(NetworkDelegateType.ServerStopped);
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            AddCurrentEvent(NetworkDelegateType.ClientConnectedInServer);
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            AddCurrentEvent(NetworkDelegateType.ClientDisconectedInServer);
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
            AddCurrentEvent(NetworkDelegateType.MessageServerReceived);
        }

        void INetworkServerDelegate.OnNetworkError(SocialPoint.Base.Error err)
        {
        }

        #endregion
    }
}
