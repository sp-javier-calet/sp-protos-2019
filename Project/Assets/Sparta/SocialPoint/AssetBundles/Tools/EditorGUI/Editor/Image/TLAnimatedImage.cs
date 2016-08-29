using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Animated TLImage to use as GIFs.
    /// </summary>
    /// The TLAnimatedImage uses TLImageFrame and a custom framerate to play the animation at a given
    /// speed. Can be loopable or used a single shot.
	public sealed class TLAnimatedImage : TLImage
	{
        private TLImageFrame[] 	_frames;
        /// <summary>
        /// Gets the frames.
        /// </summary>
        /// <value>The structure that holds a reference to the Texture drawn and the time it has to be displayed.</value>
		public TLImageFrame[] 	frames { get { return _frames; } }
		private int 			_numFrames;
        /// <summary>
        /// Gets the number of frames.
        /// </summary>
        /// <value>The number of frames the animation has.</value>
		public int 				numFrames { get { return _numFrames; } }
        private double          _fps;
        /// <summary>
        /// Gets the fps.
        /// </summary>
        /// <value>The frames per second the animation has to be reproduced to.</value>
        public double           fps { get { return _fps; } }
		private int 			_currentIndex;
		private double			_accumTime;

		private bool			_isLoopable;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is loopable.
        /// </summary>
        /// <value><c>true</c> if this instance is loopable; otherwise, <c>false</c>.</value>
		public bool				IsLoopable
		{
			get
			{
				return _isLoopable;
			}
			set
			{
				ResetLoop();
				_isLoopable = value;
			}
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.Tool.Shared.TLGUI.TLAnimatedImage"/> class.
        /// </summary>
        /// <param name="texArr">A Texture2D array reference describing the animation in order of frame execution.</param>
		public TLAnimatedImage ( Texture2D[] texArr ) : base( )
		{
			this.Type = TLImageType.TLAnimatedImage;
            _fps = 1.0;
            SetFrames(texArr, _fps);
			IsLoopable = false;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.Tool.Shared.TLGUI.TLAnimatedImage"/> class.
        /// </summary>
        /// <param name="other">Other TLAnimatedImage. Will ise the same Texture2D array reference but with its own timeline.</param>
		public TLAnimatedImage ( TLAnimatedImage other ) : base( )
		{
			this.Type = TLImageType.TLAnimatedImage;
			_frames = new TLImageFrame[other.numFrames];
			
			for (int i=0; i < other.numFrames; ++i) {
				_frames[i].Image = other.frames[i].Image;
				_frames[i].WaitTimeSecs = other.frames[i].WaitTimeSecs;
			}
			
			_numFrames = other.numFrames;
			IsLoopable = other.IsLoopable;
		}

		public void SetFrameRate( double fpsec )
		{
            _fps = fpsec;
			for (int i=0; i < _numFrames; ++i) {
				_frames[i].WaitTimeSecs = 1.0/_fps;
			}
		}

        public void SetFrames( Texture2D[] texArr, double fpsec=1.0 )
        {
            _frames = new TLImageFrame[texArr.Length];
            _fps = fpsec;
            for (int i=0; i < texArr.Length; ++i) {
                _frames[i].Image = texArr[i];
                _frames[i].WaitTimeSecs = 1.0/_fps;
            }

            _numFrames = _frames.Length;
        }

        /// <summary>
        /// Sets the animation loop delay.
        /// </summary>
        /// <param name="loopDelay">How many time(in seconds) will this animation wait before executing again.</param>
		public void SetAnimationLoopDelay( double loopDelay )
		{
			_frames [_numFrames - 1].WaitTimeSecs = loopDelay;
		}

		private void ResetLoop()
		{
			_currentIndex = 0;
			_accumTime = 0;
			_currentFrame = _frames [_currentIndex];
		}

		/// <summary>
        /// The Update method for a TLAnimatedImage.
        /// </summary>
        /// This method must be explicitely called in every Update function on every TLWidget the TLAnimatedImage is used into.
        /// <param name="elapsed">The elapsed time comming from the parent widget Update method it was called from.</param>
        /// <returns><c>true</c> if the frame has changed and requires a repaint; otherwise, <c>false</c>.</returns>
		public bool Update(double elapsed)
		{
			int oldIndex = _currentIndex;
			_accumTime += elapsed;
			if( _accumTime >= _currentFrame.WaitTimeSecs ) {
				if (IsLoopable)
					_currentIndex = (_currentIndex >= _numFrames - 1) ? 0 : _currentIndex + 1;
				else
					_currentIndex = (_currentIndex >= _numFrames - 1) ? _currentIndex : _currentIndex + 1;

				_accumTime = 0;
				_currentFrame = _frames[_currentIndex];
			}
			return oldIndex != _currentIndex;
		}
	}
}
