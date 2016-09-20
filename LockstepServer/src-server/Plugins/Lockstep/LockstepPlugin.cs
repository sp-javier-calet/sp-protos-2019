using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Photon.Hive.Plugin.Lockstep
{
    public class LockstepPlugin : PluginBase
    {
        #region Public Properties

        public override bool IsPersistent
        {
            get
            {
                return true;
            }
        }

        public override string Name
        {
            get
            {
                return "Lockstep";
            }
        }

        #endregion

        public LockstepPlugin()
        {
            UseStrictMode = true;
        }
        
        public override void OnCloseGame(ICloseGameCallInfo info)
        {
            base.OnCloseGame(info); 
        }

        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            base.OnCreateGame(info);
        }

        public override void OnJoin(IJoinGameCallInfo info)
        {
            base.OnJoin(info);
        }

        public override void OnLeave(ILeaveGameCallInfo info)
        {
            base.OnLeave(info);
            
        }

        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            base.OnRaiseEvent(info);            
        }

        public override void OnSetProperties(ISetPropertiesCallInfo info)
        {
            base.OnSetProperties(info);
        }

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if (!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }
            return true;
        }
        
    }
}
