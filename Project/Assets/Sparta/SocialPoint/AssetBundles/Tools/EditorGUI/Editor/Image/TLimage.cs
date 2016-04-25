using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
	public enum TLImageType
	{
		TLImage,
		TLAnimatedImage
	}

    /// <summary>
    /// Base image class used by the library.
    /// </summary>
    /// Represents a static image. Uses a single TLImageFrame to display a Texture2D.
	public class TLImage
	{
		protected TLImageFrame 	_currentFrame;
		public TLImageType 		Type { get; protected set; }

		public TLImage () 
		{
			this.Type = TLImageType.TLImage;
		}

		public TLImage ( Texture2D baseTex )
		{
			this.Type = TLImageType.TLImage;
			_currentFrame = new TLImageFrame (baseTex);
		}

		public TLImage ( TLImage other )
		{
			this.Type = TLImageType.TLImage;
			_currentFrame = new TLImageFrame (other.GetTexture());
		}

		public Texture2D GetTexture()
		{
			return _currentFrame.Image;
		}

        public void SetTexture( Texture2D baseTex )
        {
            _currentFrame.Image = baseTex;
        }
	}
}
