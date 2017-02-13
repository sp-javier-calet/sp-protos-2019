// -----------------------------------------------------------------------
// <copyright file="LiteEnums.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Photon.Stardust.S2S.Server.Enums
{
    public class LiteOpCode
    {
        public const byte Join = 255;

        public const byte Leave = 254;

        public const byte RaiseEvent = 253;

        public const byte SetProperties = 252;

        public const byte GetProperties = 251;

        public const byte Ping = 249;

        public const byte ChangeGroups = 248;
    }


    public class LiteEventCode
    {
        public const byte NoCodeSet = 0;

        public const byte Join = 255;

        public const byte Leave = 254;

        public const byte PropertiesChanged = 253;
    }

    public class LiteOpKey
    {
         public const byte GameId = 255; 

        public const byte ActorNr = 254; 

        public const byte TargetActorNr = 253; 

        public const byte Actors = 252; 

        public const byte Properties = 251; 

        public const byte Broadcast = 250; 

        public const byte ActorProperties = 249; 

        public const byte GameProperties = 248; 

        public const byte Cache = 247;

        public const byte ReceiverGroup = 246;

        public const byte Data = 245; 

        public const byte Code = 244; 

        public const byte Flush = 243;

        public const byte DeleteCacheOnLeave = 241;

        public const byte Group = 240;

        public const byte GroupsForRemove = 239;

        public const byte GroupsForAdd = 238;

        public const byte SuppressRoomEvents = 237;

        public const byte JoinMode = 215;

        public const byte Plugin = 204;
    }
}
