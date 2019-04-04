//-----------------------------------------------------------------------
// Config.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using SocialPoint.Lockstep;

namespace Examples.Multiplayer.Lockstep
{
    [Serializable]
    public class Config
    {
        public const byte DefaultNumPlayers = 2;
        const long DefaultManaSpeed = 2;
        const long DefaultMaxMana = 10000;
        const long DefaultGameDuration = 30000;
        const long DefaultUnitCost = 2000;
        const int DefaultSimStep = 33;
        const int DefaultCmdStep = 198;
        const bool DefaultRealtimeUpdate = true;

        public int CmdStep = DefaultCmdStep;
        public long Duration = DefaultGameDuration;
        public long ManaSpeed = DefaultManaSpeed;
        public long MaxMana = DefaultMaxMana;

        public byte NumPlayers = DefaultNumPlayers;
        public bool RealtimeUpdate = DefaultRealtimeUpdate;
        public int SimStep = DefaultSimStep;
        public long UnitCost = DefaultUnitCost;

        public LockstepConfig Lockstep { get { return new LockstepConfig {SimulationStepDuration = SimStep, CommandStepDuration = CmdStep}; } }

        public LockstepServerConfig LockstepServer { get { return new LockstepServerConfig {MaxPlayers = NumPlayers}; } }
    }
}