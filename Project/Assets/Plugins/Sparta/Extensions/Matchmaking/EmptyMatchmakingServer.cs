using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Matchmaking
{
    public class EmptyMatchmakingServer : IMatchmakingServer, IDisposable
    {
        // If matchmaking is enabled in the LockstepNetworkServer instance, it needs a proper response.
        const string JSONResponse = "{\"player1_token\" : \"aaa\", \"player2_token\" : \"bbb\", \"player_1\" : {\"id\" : 2147483646}, \"player_2\" : {\"id\" : 2147483647}}";

        readonly List<IMatchmakingServerDelegate> _delegates;

        public EmptyMatchmakingServer()
        {
            _delegates = new List<IMatchmakingServerDelegate>();
        }

        #region IMatchmakingServer implementation

        void IMatchmakingServer.AddDelegate(IMatchmakingServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        void IMatchmakingServer.RemoveDelegate(IMatchmakingServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        void IMatchmakingServer.LoadInfo(string matchId, List<string> playerIds)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMatchInfoReceived(System.Text.Encoding.UTF8.GetBytes(JSONResponse));
            }
        }

        void IMatchmakingServer.NotifyResults(string matchId, AttrDic results, AttrDic customData)
        {

        }

        bool IMatchmakingServer.Enabled
        {
            get
            {
                return true;
            }
        }

        string IMatchmakingServer.Version
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        AttrDic IMatchmakingServer.ClientsVersions
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        SocialPoint.Network.HttpRequest IMatchmakingServer.InfoRequest
        {
            get
            {
                return null;
            }
        }

        SocialPoint.Network.HttpRequest IMatchmakingServer.NotifyRequest
        {
            get
            {
                return null;
            }
        }

        SocialPoint.Network.HttpResponse IMatchmakingServer.InfoResponse
        {
            get
            {
                return null;
            }
        }

        SocialPoint.Network.HttpResponse IMatchmakingServer.NotifyResponse
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            _delegates.Clear();
        }

        #endregion
    }
}

