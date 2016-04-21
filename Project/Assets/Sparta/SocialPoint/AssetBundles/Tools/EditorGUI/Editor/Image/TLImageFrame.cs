using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Class used by TLImage classes to hold the Texture2D references and additional information.
    /// </summary>
	public struct TLImageFrame
	{
        /// <summary>
        /// Time this frame has to be displayed(useful for animated frames).
        /// </summary>
        /// <value>The wait time in secs.</value>
		public double?		WaitTimeSecs { get; set; }
        /// <summary>
        /// The reference to the Texture2D that will be displayed.
        /// </summary>
        /// <value>The texture reference.</value>
		public Texture2D 	Image { get; set; }
		
		public TLImageFrame( Texture2D image ) : this()
		{
			WaitTimeSecs = null;
			Image = image;
		}
		
		public TLImageFrame( Texture2D image, double? waitTimeSecs ) : this()
		{
			WaitTimeSecs = waitTimeSecs;
			Image = image;
		}
	}
}
