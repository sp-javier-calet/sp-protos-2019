using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A list of TLWCheck Widgets.
    /// </summary>
    /// This Widget is undocumented and it's behaviour has not been tested properly.
	public class TLWCheckBox: TLWidget
	{
		private List<TLWCheck> _checkList;

		public TLWCheckBox( TLView view, string name, string[] items ): base ( view, name )
		{
			_checkList = new List<TLWCheck>();
			SetCheckList( items );
		}

		public override void Perform ()
		{
			for ( int i = 0; i < _checkList.Count; i++ ) {
				_checkList[i].Draw();
			}
		}

		public void SetCheckList( string[] items )
		{
			_checkList.Clear();
			for ( int i = 0; i < items.Length; i++ ) {
				_checkList.Add( new TLWCheck( View, Name+"_"+i, items[i] ) );
			}
		}

		public TLWCheck[] GetSelection( bool isSelected = true )
		{
			List<TLWCheck> selection = new List<TLWCheck>();

			for ( int i = 0; i < _checkList.Count; i++ ) {
				if ( _checkList[i].isChecked == isSelected ) {
					selection.Add( _checkList[i] );
				}
			}

			return selection.ToArray();
		}

		public string[] GetSelectionNames( bool isSelected = true )
		{
			List<string> selection = new List<string>();

			TLWCheck[] checkSelection = GetSelection( isSelected );
			for ( int i = 0; i < checkSelection.Length; i++ ) {
				selection.Add( checkSelection[i].text );
			}

			return selection.ToArray();
		}

		public TLWCheck[] GetCheckList()
		{
			return _checkList.ToArray();
		}

		public void SetCheckItemByName( string name, bool value )
		{
			TLWCheck check = GetCheckByName( name );
			if ( check == null ) return;

			check.SetCheck( value );
		}

		public TLWCheck GetCheckByName( string name )
		{
			for ( int i = 0; i < _checkList.Count; i++ ) {
				if ( _checkList[i].text == name )
					return _checkList[i];
			}

			return null;
		}
	}
}
