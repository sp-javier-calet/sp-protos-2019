using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Multiplayer;
using System;

public class TestMultiplayerServerBehaviour : INetworkServerSceneBehaviour, INetworkMessageReceiver, IDisposable
{
    NetworkServerSceneController _controller;
    IParser<ClickAction> _clickParser;

    public TestMultiplayerServerBehaviour(NetworkServerSceneController ctrl)
    {
        _clickParser = new ClickActionParser();
        _controller = ctrl;
        _controller.AddBehaviour(this);
        _controller.RegisterReceiver(this);
    }

    public void Dispose()
    {
        _controller.RemoveBehaviour(this);
        _controller.RegisterReceiver(null);
    }

    public void Update(float dt, NetworkScene scene, NetworkScene oldScene)
    {
        var itr = _controller.Scene.GetObjectEnumerator();
        while(itr.MoveNext())
        {
            var t = itr.Current.Transform;
            t.Position.x += RandomUtils.Range(-0.5f, +0.5f);
            t.Position.z += RandomUtils.Range(-0.5f, +0.5f);
        }
        itr.Dispose();
    }

    public void OnMessageReceived(NetworkMessageData data, IReader reader)
    {
        if(data.MessageType == GameMsgType.ClickAction)
        {
            var ac = _clickParser.Parse(reader);
            _controller.Instantiate("Cube", new Transform(ac.Position));
        }
    }

    public void OnClientConnected(byte clientId)
    {
    }

    public void OnClientDisconnected(byte clientId)
    {
    }
}