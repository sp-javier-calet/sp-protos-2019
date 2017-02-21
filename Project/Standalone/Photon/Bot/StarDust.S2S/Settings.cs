// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The settings.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server
{
    using System;
    using System.Configuration;

    using Photon.SocketServer;

    /// <summary>
    ///   The settings.
    /// </summary>
    public static class Settings
    {
        #region Constants and Fields

        public static readonly bool ActiveDisconnect = bool.Parse(ConfigurationManager.AppSettings["ActiveDisconnect"]);

        public static readonly byte FlushChannel = byte.Parse(ConfigurationManager.AppSettings["FlushChannel"]);

        public static readonly bool FlushReliable = bool.Parse(ConfigurationManager.AppSettings["FlushReliable"]);

        public static readonly int LogCounterInterval = 1000;
        
        public static readonly NetworkProtocolType Protocol =
            (NetworkProtocolType)
            Enum.Parse(typeof(NetworkProtocolType), ConfigurationManager.AppSettings["Protocol"], true);

        public static readonly byte ReliableDataChannel =
            byte.Parse(ConfigurationManager.AppSettings["ReliableDataChannel"]);

        public static readonly int ReliableDataSize = int.Parse(ConfigurationManager.AppSettings["ReliableDataSize"]);

        public static readonly int TimeInGame = int.Parse(ConfigurationManager.AppSettings["TimeInGame"]);

        public static readonly byte UnreliableDataChannel =
            byte.Parse(ConfigurationManager.AppSettings["UnreliableDataChannel"]);

        public static readonly int UnreliableDataSize = int.Parse(
            ConfigurationManager.AppSettings["UnreliableDataSize"]);

        public static int FlushInterval = int.Parse(ConfigurationManager.AppSettings["FlushInterval"]);

        public static byte NumClientsPerGame = byte.Parse(ConfigurationManager.AppSettings["NumClientsPerGame"]);

        public static int NumGamesPerLobby = int.Parse(ConfigurationManager.AppSettings["NumGamesPerLobby"]);

        public static int NumLobbies = int.Parse(ConfigurationManager.AppSettings["NumLobbies"]);

        public static int NumClientsPerLobby = int.Parse(ConfigurationManager.AppSettings["NumClientsPerLobby"]);

        public static int PingInterval = int.Parse(ConfigurationManager.AppSettings["PingInterval"]);

        public static int ReliableDataSendInterval =
            int.Parse(ConfigurationManager.AppSettings["ReliableDataSendInterval"]);

        public static bool SendReliableData = bool.Parse(ConfigurationManager.AppSettings["SendReliableData"]);

        public static bool SendUnreliableData = bool.Parse(ConfigurationManager.AppSettings["SendUnreliableData"]);

        public static string ServerAddress = ConfigurationManager.AppSettings["ServerAddress"];

        public static string ApplicationId = ConfigurationManager.AppSettings["ApplicationId"];

        public static int StartupInterval = int.Parse(ConfigurationManager.AppSettings["StartupInterval"]);
        
        public static int UnreliableDataSendInterval =
            int.Parse(ConfigurationManager.AppSettings["UnreliableDataSendInterval"]);
        public static bool UseEncryption = bool.Parse(ConfigurationManager.AppSettings["UseEncryption"]);

        public static byte LobbyType = byte.Parse(ConfigurationManager.AppSettings["LobbyType"]);

        #endregion
    }
}