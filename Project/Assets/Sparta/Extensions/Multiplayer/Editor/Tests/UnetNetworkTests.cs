using System;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using SocialPoint.Utils;
using UnityEngine.Networking;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class UnetNetworkTests : BaseNetworkTests
    {
        IDisposable[] _disposables;

        [SetUp]
        public void SetUp()
        {
            var go = new UnityEngine.GameObject();
            var runner = go.AddComponent<UnityUpdateRunner>();
            var userver = new UnetNetworkServer(runner);
            var uclient = new UnetNetworkClient();
            var uclient2 = new UnetNetworkClient();
            _server = userver;
            _client = uclient;
            _client2 = uclient2;
            _disposables = new IDisposable[] {
                userver, uclient, uclient2
            };
        }

        override protected void WaitForEvents()
        {
        }

        [TearDown]
        public void TearDown()
        {
            if(_disposables != null)
            {
                for(var i = 0; i < _disposables.Length; i++)
                {
                    var d = _disposables[i];
                    if(d != null)
                    {
                        d.Dispose();
                    }
                }
            }
        }
    }
}
