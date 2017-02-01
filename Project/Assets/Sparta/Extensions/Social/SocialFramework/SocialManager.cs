using SocialPoint.Attributes;
using System;

namespace SocialPoint.Social
{
    public static class NotificationType
    {
        public const int ChatWarning = 99;
        public const int TextMessage = 100;

        // Personal notifications
        public const int NotificationAllianceMemberAccept = 1;
        public const int NotificationAllianceMemberKickoff = 2;
        public const int NotificationAllianceMemberPromote = 3;
        public const int NotificationAllianceJoinRequest = 4;
        public const int NotificationAlliancePlayerAutoPromote = 109;
        public const int NotificationAlliancePlayerAutoDemote = 110;
        public const int NotificationUserChatBan = 307;

        // Alliance notifications
        public const int BroadcastAllianceMemberAccept = 101;
        public const int BroadcastAllianceJoin = 102;
        public const int BroadcastAllianceMemberKickoff = 103;
        public const int BroadcastAllianceMemberLeave = 104;
        public const int BroadcastAllianceEdit = 105;
        public const int BroadcastAllianceMemberPromote = 106;
        public const int BroadcastAllianceMemberRankChange = 111;
        public const int BroadcastAllianceOnlineMember = 308;
    }

    public sealed class SocialManager
    {
        public SocialPlayerFactory PlayerFactory{ get; private set; }

        public SocialPlayer LocalPlayer{ get; private set; }

        public event Action<AttrDic> OnLocalPlayerLoaded;

        public SocialManager()
        {
            PlayerFactory = new SocialPlayerFactory();
        }

        /// <summary>
        /// Method to parse the local player info about the SocialFeature
        /// It expects a dictionary with the data of the different components
        /// in subdictionaries 
        /// (For example: {"alliances":{...}, "game_feature":{...}})
        /// </summary>
        public void SetLocalPlayerData(AttrDic data, IPlayerData basicData)
        {
            LocalPlayer = PlayerFactory.CreateSocialPlayer(data);

            var basicDataComponent = new SocialPlayer.BasicData();
            basicDataComponent.Uid = basicData.Id;
            basicDataComponent.Level = basicData.Level;
            basicDataComponent.Name = basicData.Name;

            LocalPlayer.AddComponent(basicDataComponent);

            OnLocalPlayerLoaded(data);
        }

        /// <summary>
        /// Method to parse the local player info about the SocialFeature
        /// It expects a dictionary with the basic player data and the data
        /// of the different components in subdictionaries
        /// (For example: {"id":123, "name":"Pepito", "alliances":{...}, "game_feature":{...}})
        /// </summary>
        public void SetLocalPlayerData(AttrDic data)
        {
            LocalPlayer = PlayerFactory.CreateSocialPlayer(data);

            OnLocalPlayerLoaded(data);
        }
    }
}