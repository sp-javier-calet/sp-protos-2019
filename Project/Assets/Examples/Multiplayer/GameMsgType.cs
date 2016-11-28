using SocialPoint.Multiplayer;

public static class GameMsgType
{
    public const byte ClickAction = SceneMsgType.Highest + 1;
    public const byte ExplosionEvent = SceneMsgType.Highest + 2;
    public const byte MovementAction = SceneMsgType.Highest + 3;
    public const byte PathEvent = SceneMsgType.Highest + 4;
}
