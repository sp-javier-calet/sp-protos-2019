﻿using SocialPoint.Multiplayer;
using SocialPoint.IO;

public class ClickAction : INetworkShareable
{
    public Vector3 Position;

    public void Deserialize(IReader reader)
    {
        Position = Vector3Parser.Instance.Parse(reader);
    }

    public void Serialize(IWriter writer)
    {
        Vector3Serializer.Instance.Serialize(Position, writer);
    }
}