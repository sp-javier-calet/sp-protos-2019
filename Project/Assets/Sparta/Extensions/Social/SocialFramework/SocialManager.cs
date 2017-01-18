using SocialPoint.Attributes;
using System;

namespace SocialPoint.Social
{
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
        /// It expects a dictionary with the data of the different componenets
        /// in subdictionaries 
        /// (For example: {"alliances":{...}, "game_feature":{...}})
        /// </summary>
        public void SetLocalPlayerData(AttrDic data, IPlayerData basicData)
        {
            LocalPlayer = PlayerFactory.CreateSocialPlayer(data);

            var basicDataComponenet = new SocialPlayer.BasicData();
            basicDataComponenet.Uid = basicData.Id;
            basicDataComponenet.Level = (int)basicData.Level;
            basicDataComponenet.Name = basicData.Name;

            LocalPlayer.AddComponent(basicDataComponenet);

            OnLocalPlayerLoaded(data);
        }

        /// <summary>
        /// Method to parse the local player info about the SocialFeature
        /// It expects a dictionary with the basic player data and the data
        /// of the different componenets in subdictionaries
        /// (For example: {"id":123, "name":"Pepito", "alliances":{...}, "game_feature":{...}})
        /// </summary>
        public void SetLocalPlayerData(AttrDic data)
        {
            LocalPlayer = PlayerFactory.CreateSocialPlayer(data);

            OnLocalPlayerLoaded(data);
        }
    }
}