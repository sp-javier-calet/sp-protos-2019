using System.Collections;

namespace SocialPoint.Tool.Shared.TLGUI
{
	public abstract class TLTask
	{
		protected string _result;

		public string result { get { return _result; } }

		public TLTask()
		{
			_result = "";
		}

		/**
		 * return the result as a string
		 */
		public abstract string Perform();

		public virtual void CleanUp() {}
	}
}
