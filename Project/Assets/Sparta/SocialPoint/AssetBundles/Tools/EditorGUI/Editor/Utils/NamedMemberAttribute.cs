using System;

namespace SocialPoint.Tool.Shared.TLGUI.Utils
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class NamedMemberAttribute : Attribute 
	{
		public readonly string Name;
		
		public NamedMemberAttribute(string name)  // url is a positional parameter
		{
			this.Name = name;
		}
	}
}
