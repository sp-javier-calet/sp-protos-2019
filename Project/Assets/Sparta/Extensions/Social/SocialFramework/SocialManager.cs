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

            if(OnLocalPlayerLoaded != null)
            { 
                OnLocalPlayerLoaded(data);
            }
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