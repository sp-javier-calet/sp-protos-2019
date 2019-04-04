//-----------------------------------------------------------------------
// ClientConfig.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using System.IO;
using UnityEngine;

namespace Examples.Multiplayer.Lockstep
{
    [Serializable]
    public class ClientConfig
    {
        const int DefaultLocalSimDelay = 500;
        const string DefaultReplayName = "last_replay.rpl";

        public Transform Container;
        public GameObject CubePrefab;

        public Config General;
        public GameObject LoadingPrefab;

        public int LocalSimDelay = DefaultLocalSimDelay;
        public string ReplayName = DefaultReplayName;

        public string ReplayPath { get { return Path.Combine(Application.persistentDataPath, ReplayName); } }
    }
}