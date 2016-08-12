using SocialPoint.Utils;
using SocialPoint.Multiplayer;
using System;

public class TestMultiplayerServerBehaviour : INetworkServerSceneBehaviour, IDisposable
{
    NetworkServerSceneController _scene;
    IParser<ClickAction> _clickParser;

    public TestMultiplayerServerBehaviour(NetworkServerSceneController scene)
    {
        _clickParser = new ClickActionParser();
        _scene = scene;
        _scene.AddBehaviour(this);
    }

    public void Dispose()
    {
        _scene.RemoveBehaviour(this);
    }

    public void Update(float dt, NetworkGameScene scene, NetworkGameScene oldScene)
    {
    }

    public void OnMessageReceived(byte clientId, ReceivedNetworkMessage msg)
    {
        if(msg.MessageType == GameMsgType.ClickAction)
        {
            var ac = _clickParser.Parse(msg.Reader);
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