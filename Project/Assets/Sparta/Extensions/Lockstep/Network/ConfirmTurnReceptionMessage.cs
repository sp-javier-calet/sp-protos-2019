﻿using System;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public class ConfirmTurnsReceptionMessage : INetworkMessage
    {
        public int[] ConfirmedTurns { get; private set; }

        public int Client;

        public ConfirmTurnsReceptionMessage(int[] confirmedTurns = null)
        {
            ConfirmedTurns = confirmedTurns;
        }

        public void Deserialize(IReader reader)
        {
            int length = (int)reader.ReadByte();
            ConfirmedTurns = new int[length];
            for(int i = 0; i < length; ++i)
            {
                ConfirmedTurns[i] = reader.ReadInt32();
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write((byte)ConfirmedTurns.Length);
            for(int i = 0; i < ConfirmedTurns.Length; ++i)
            {
                writer.Write(ConfirmedTurns[i]);
            }
        }

        public bool RequiresSync
        {
            get
            {
                return false;
            }
        }
    }
}