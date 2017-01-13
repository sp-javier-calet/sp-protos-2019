using SocialPoint.Social;
using SocialPoint.Attributes;
using System;
using System.Collections.Generic;

namespace SocialPoint.Social
{
    public class SocialPlayerFactory
    {
        public interface IFactory
        {
            SocialPlayer.IComponent CreateElement(AttrDic dic);
        }

        const string MemberUidKey = "id";
        const string MemberNameKey = "name";
        const string MemberLevelKey = "level";
        const string MemberScoreKey = "power";

        readonly List<IFactory> _factories;

        public SocialPlayerFactory()
        {
            _factories = new List<IFactory>();
        }

        public SocialPlayer CreateSocialPlayer(AttrDic dic)
        {
            var player = new SocialPlayer();
            ParseSocialPlayer(player, dic);
            return player;
        }

        void ParseSocialPlayer(SocialPlayer player, AttrDic dic)
        {
            player.Uid = dic.GetValue(MemberUidKey).ToString();
            player.Name = dic.GetValue(MemberNameKey).ToString();
            player.Level = dic.GetValue(MemberLevelKey).ToInt();
            player.Score = dic.GetValue(MemberScoreKey).ToInt();

            for(var i = 0; i < _factories.Count; i++)
            {
                var component = _factories[i].CreateElement(dic);
                if(component != null)
                {
                    player.AddComponent(component);
                }
            }
        }

        public void AddFactory(IFactory factory)
        {
            _factories.Add(factory);
        }
    }
}
