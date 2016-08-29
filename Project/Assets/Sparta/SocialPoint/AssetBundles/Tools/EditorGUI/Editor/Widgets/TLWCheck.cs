using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A Checkbox and Label Widgets.
    /// </summary>
    /// Can send check events.
    /// Can send uncheck events.
	public sealed class TLWCheck: TLWidget
	{
		private bool _isChecked;
		private string _text;
        private TLEvent<bool> _onCheckEvent;

        /// <summary>
        /// Gets a value indicating whether this <see cref="SocialPoint.Tool.Shared.TLGUI.TLWCheck"/> is checked.
        /// </summary>
        /// <value><c>true</c> if is checked; otherwise, <c>false</c>.</value>
		public bool isChecked { get { return _isChecked; } }
        /// <summary>
        /// Gets or sets the text label text.
        /// </summary>
        /// <value>The text for the label.</value>
        public string text { get { return _text; } set { _text = value; } }
        /// <summary>
        /// Gets the check event to connect to. Can have a value of true or false
        /// </summary>
        /// <value>The check event to connect to.</value>
        public TLEvent<bool> onCheckEvent { get { return _onCheckEvent; } }

		public TLWCheck( TLView view, string name, string text ): base ( view, name )
		{
			_isChecked = false;
			_text = text;
            _onCheckEvent = new TLEvent<bool>( "OnCheckEvent" );
		}

        public TLWCheck( TLView view, string name, string text, GUILayoutOption[] options ): base ( view, name, options )
        {
            _isChecked = false;
            _text = text;
            _onCheckEvent = new TLEvent<bool>( "OnCheckEvent" );
        }

		public override void Perform ()
		{
			bool prevIsChecked = _isChecked;
			_isChecked = GUILayout.Toggle( _isChecked, _text, Options );
			if ( _isChecked != prevIsChecked ) {
				if ( _isChecked ) {
                    onCheckEvent.Send(View.window, true);
				}
				else {
                    onCheckEvent.Send(View.window, false);
				}
			}
		}

        /// <summary>
        /// Programatically set the check status(and fire an event if needed).
        /// </summary>
        /// <param name="value">The new value of the checkbox.</param>
		public void SetCheck( bool value )
		{
            if (_isChecked != value) {
			    _isChecked = value;

                if (!_isChecked) {
                    onCheckEvent.Send(View.window, false);
                }
                else {
                    onCheckEvent.Send(View.window, true);
                }
            }
		}
	}
}
