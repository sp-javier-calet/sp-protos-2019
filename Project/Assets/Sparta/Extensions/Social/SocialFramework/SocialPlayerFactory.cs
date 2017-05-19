using SocialPoint.Social;
using SocialPoint.Attributes;
using System;
using System.Collections.Generic;

namespace SocialPoint.Social
{
    public sealed class SocialPlayerFactory
    {
        const string MemberUidKey = "id";
        const string MemberNameKey = "name";
        const string MemberLevelKey = "level";
        const string MemberScoreKey = "score";

        public interface IFactory
        {
            SocialPlayer.IComponent CreateElement(AttrDic dic);
        }

        sealed class BasicDataFactory : IFactory
        {
            public SocialPlayer.IComponent CreateElement(AttrDic dic)
            {
                var component = new SocialPlayer.BasicData();
                component.Uid = dic.GetValue(MemberUidKey).ToString();
                component.Name = dic.GetValue(MemberNameKey).ToString();
                component.Level = dic.GetValue(MemberLevelKey).ToInt();
                component.Score = dic.GetValue(MemberScoreKey).ToInt();
                return component;
            }
        }

        readonly List<IFactory> _factories;

        public SocialPlayerFactory()
        {
            _factories = new List<IFactory>();
            _factories.Add(new BasicDataFactory());
        }

        public SocialPlayer CreateSocialPlayer(AttrDic dic)
        {
            var player = new SocialPlayer();
            ParseSocialPlayer(player, dic);
            return player;
        }

        void ParseSocialPlayer(SocialPlayer player, AttrDic dic)
        {
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
