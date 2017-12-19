﻿using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;
using System.IO;
using System;
using System.Net.Sockets;
using SocialPoint.Network;

public class SimpleSocketClientData
{
    byte Type;
    int Length;
    byte _clientId;
    TcpClient _client;
    NetworkStream _netStream;
    MemoryStream _memStream;
    SystemBinaryReader _reader;

    public TcpClient Client
    {
        get
        {
            return _client;
        }
    }

    public NetworkStream Stream
    {
        get
        {
            return _netStream;
        }
    }

    public byte ClientId
    {
        get
        {
            return _clientId;
        }
    }

    public SimpleSocketClientData(byte clientId, TcpClient client)
    {
        _client = client;
        _clientId = clientId;
        _netStream = client.GetStream();
        _memStream = new MemoryStream();
        _reader = new SystemBinaryReader(_memStream);
    }

    public event Action<NetworkMessageData, IReader> MessageReceived;

    public void Receive()
    {

        if(!_netStream.CanRead)
        {
            return;
        }

        // leer de net stream en mem stream
//        byte[] bytes = new byte[bytesAvailable];
//        _netStream.Read(bytes, 0, bytesAvailable);
//        _memStream.Write(bytes, 0, bytesAvailable);

//        _memStream.Seek(0, SeekOrigin.Begin);
//
//        Type = _reader.ReadByte();
//
//        Length = _reader.ReadInt32();
//        UnityEngine.Debug.Log("LENGHT "+Length);
//
//        if(MessageReceived != null)
//        {
//            MessageReceived(new NetworkMessageData {
//                MessageType = Type,
//                ClientIds = { _clientId }
//            }, _reader);
//        }
//        _memStream.Seek(0, SeekOrigin.Begin);


        byte[] bytes = new byte[1];
        _netStream.Read(bytes, 0, sizeof(byte));
        _memStream.Write(bytes, 0, sizeof(byte));

        if(_memStream.Position == sizeof(byte))
        {
            _memStream.Seek(0, SeekOrigin.Begin);
            Type = _reader.ReadByte();
        }
        if(_memStream.Position == sizeof(byte) + sizeof(Int32))
        {
            _memStream.Seek(1, SeekOrigin.Begin);
            Length = _reader.ReadInt32();
        }
        if(_memStream.Position == sizeof(byte) + sizeof(Int32) + Length)
        {
            _memStream.Seek(5, SeekOrigin.Begin);
            if(MessageReceived != null)
            {
                MessageReceived(new NetworkMessageData {
                    MessageType = Type,
                    ClientIds = new List<byte>(){_clientId},
                }, _reader);
            }
            _memStream.Seek(0, SeekOrigin.Begin);
        }
    }
}
