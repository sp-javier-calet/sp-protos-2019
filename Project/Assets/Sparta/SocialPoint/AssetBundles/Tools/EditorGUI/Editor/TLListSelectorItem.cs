namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Base class for TLListSelectorItem.
    /// </summary>
    /// TLListSelectorItem and its subclasses are used in TLWListSelector widgets.
	public class TLListSelectorItem
	{	
        /// <summary>
        /// Gets or sets the text content for this item.
        /// </summary>
        /// <value>The content.</value>
		public string							Content { get; set; }

		public TLListSelectorItem()
		{
			Content = "";
		}

		public TLListSelectorItem(string text)
		{
			Content = text;
		}
	}
}