using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// The base class for all TLView models.
    /// </summary>
    /// TLModel should hold the persistent data of the view and the controller.
    /// Derives ScriptableObject so that it could be possible to serialize its data for a view and
    /// retrieve this data 
	public class TLModel : ScriptableObject
	{
		public TLModel()
		{
		}
	}
}
