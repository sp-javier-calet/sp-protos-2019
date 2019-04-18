//-----------------------------------------------------------------------
// GameTypes.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.Lockstep;
using SocialPoint.NetworkModel;

namespace Examples.Multiplayer.Lockstep
{
    public static class GameCommandType
    {
        public static byte Click = 1;

        public static void Setup(NetworkScene scene, LockstepCommandFactory factory, LockstepClient client)
        {
            factory.Register<ClickCommand>(Click);
            client.RegisterCommandLogic(new ClickCommandLogic(scene));
        }
    }

    public static class GameObjectType
    {
        public static byte Cube = 1;
    }
}