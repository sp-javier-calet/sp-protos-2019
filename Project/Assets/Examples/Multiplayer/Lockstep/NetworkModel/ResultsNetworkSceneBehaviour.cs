using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Lockstep;
using SocialPoint.NetworkModel;
using SocialPoint.Utils;

namespace Examples.Multiplayer.Lockstep
{
    public class ResultsNetworkSceneBehaviour : INetworkSceneDelegate
    {
        readonly Dictionary<byte, int> _objectCount = new Dictionary<byte, int>();

        void INetworkSceneDelegate.OnObjectInstantiated(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectAdded(NetworkGameObject ngo)
        {
            var owner = ngo.Behaviours.Get<OwnerNetworkBehavior>();
            if(owner == null)
            {
                return;
            }

            if(_objectCount.ContainsKey(owner.PlayerNumber))
            {
                _objectCount[owner.PlayerNumber]++;
            }
            else
            {
                _objectCount[owner.PlayerNumber] = 1;
            }
        }

        void INetworkSceneDelegate.OnObjectRemoved(NetworkGameObject ngo)
        {
        }

        void INetworkSceneDelegate.OnObjectDestroyed(NetworkGameObject ngo)
        {
        }

        void IDeltaUpdateable<int>.Update(int elapsed)
        {
        }

        public Attr ToAttr()
        {
            var attr = new AttrDic();
            var itr = _objectCount.GetEnumerator();
            while(itr.MoveNext())
            {
                attr.SetValue(itr.Current.Key.ToString(), itr.Current.Value);
            }

            itr.Dispose();
            return attr;
        }

        public void Send(LockstepNetworkClient netClient)
        {
            netClient.SendPlayerFinish(GetPlayerResult(netClient.PlayerNumber));
        }

        public Attr GetPlayerResult(byte playerNumber)
        {
            var objectCount = 0;
            _objectCount.TryGetValue(playerNumber, out objectCount);

            return new AttrInt(objectCount);
        }
    }
}