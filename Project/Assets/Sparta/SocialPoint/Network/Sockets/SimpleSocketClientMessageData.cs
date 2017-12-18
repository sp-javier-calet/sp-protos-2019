using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;
using System.IO;
using System;
using System.Net.Sockets;
using SocialPoint.Network;

public class SimpleSocketClientMessageData
{

    SystemBinaryReader Reader;
    MemoryStream Stream;
    byte Type;
    int Length;
    byte _clientId;

    public SimpleSocketClientMessageData(byte clientId)
    {
        _clientId = clientId;
        Stream = new MemoryStream();
        Reader = new SystemBinaryReader(Stream);
    }

    public event Action<NetworkMessageData, IReader> MessageReceived;

    public void Receive(Socket socket)
    {
        var nextByte = new byte[1];
        socket.Receive(nextByte, 0, 1, SocketFlags.None);
        Stream.Write(nextByte, 0, 1);
        if(Stream.Position == 1)
        {
            Type = Reader.ReadByte();
        }
        if(Stream.Position == 3)
        {
            Length = Reader.ReadInt32();
        }
        if(Stream.Position == 3 + Length)
        {
            var data = Reader.ReadBytes(Length);
            var reader = new SystemBinaryReader(new MemoryStream(data));
            if(MessageReceived != null)
            {
                MessageReceived(new NetworkMessageData {
                    MessageType = Type,
                    ClientIds = { _clientId }
                }, reader);
            }
            Stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
