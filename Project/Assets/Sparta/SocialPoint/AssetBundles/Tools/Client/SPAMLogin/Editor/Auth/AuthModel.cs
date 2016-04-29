using System;
using SocialPoint.Tool.Shared.TLGUI;
using SocialPoint.Attributes;

namespace SocialPoint.Editor.SPAMGui
{
	public class AuthModel : TLModel
	{
		public int			numLoadingDots { get; set; }
		public DateTime		lastSecondTime { get; set; }
		//public AttrDic		loginResponse { get; set; }
		
		public AuthModel() : base ()
		{
            Init();
		}

        void Init()
        {
            lastSecondTime = DateTime.Now;
            numLoadingDots = 0;
            //loginResponse = null;
        }

        public void OnEnable()
        {
            //Reset
            Init();
        }
	}
}
