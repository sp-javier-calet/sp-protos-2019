using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Multiplayer;
using System;

public class TestMultiplayerServerBehaviour : INetworkServerSceneBehaviour, INetworkMessageReceiver, IDisposable
{
    NetworkServerSceneController _scene;
    IParser<ClickAction> _clickParser;

    public TestMultiplayerServerBehaviour(NetworkServerSceneController scene)
    {
        _clickParser = new ClickActionParser();
        _scene = scene;
        _scene.AddBehaviour(this);
        _scene.RegisterReceiver(this);
    }

    public void Dispose()
    {
        _scene.RemoveBehaviour(this);
        _scene.RegisterReceiver(null);
    }

    public void Update(float dt, NetworkScene scene, NetworkScene oldScene)
    {
    }

    public void OnMessageReceived(NetworkMessageData data, IReader reader)
    {
        if(data.MessageType == GameMsgType.ClickAction)
        {
            var ac = _clickParser.Parse(reader);
            _scene.Instantiate("Cube", new Transform(ac.Position));
        }
    }

    public void OnClientConnected(byte clientId)
    {
    }

    public void OnClientDisconnected(byte clientId)
    {
    }
}