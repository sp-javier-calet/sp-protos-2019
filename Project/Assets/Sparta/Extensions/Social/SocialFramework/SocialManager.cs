using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public sealed class SocialManager
    {
        public SocialPlayerFactory PlayerFactory{ get; private set; }

        public SocialPlayer LocalPlayer{ get; private set; }

        public SocialManager()
        {
            PlayerFactory = new SocialPlayerFactory();
        }

        public void SetLocalPlayerData(AttrDic data)
        {
            LocalPlayer = PlayerFactory.CreateSocialPlayer(data);
        }
    }
}