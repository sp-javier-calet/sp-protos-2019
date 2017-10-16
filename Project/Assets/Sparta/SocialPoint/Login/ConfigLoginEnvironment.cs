using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocialPoint.Login
{
    public class ConfigLoginEnvironment
    {
        public const string DefaultConfigEndpoint = "http://backend.pro.configmanager.sp.laicosp.net/products/{0}/envs/{1}/download";

        public string Environment;
        public string GameId;
        public string EntryScene;

        public string Endpoint
        {
            get
            {
                return string.Format(DefaultConfigEndpoint, GameId, Environment);
            }
        }

    }
}
