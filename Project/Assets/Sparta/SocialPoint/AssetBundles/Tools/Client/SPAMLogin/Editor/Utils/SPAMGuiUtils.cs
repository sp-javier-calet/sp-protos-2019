using UnityEditor;
using UnityEngine;
using SocialPoint.Attributes;

namespace SocialPoint.Editor.SPAMGui
{
	public static class SPAMGuiUtils
	{
		public static bool EvalResponse( AttrDic response )
		{
			if (response.ContainsKey ("error")) {

				EditorUtility.DisplayDialog ("Exception", response.ToString(), "Close");
				return false;
			} else if (response.ContainsKey ("result") && response ["result"].AsValue.ToString() == "ERROR") {

				EditorUtility.DisplayDialog ("Exception", response.ToString(), "Close");
				return false;
			} else if (response.ContainsKey ("response")) {

				if (response["response"].AsDic.ContainsKey("error")) {

					EditorUtility.DisplayDialog("Exception", response.ToString(), "Close");
					return false;
				} else if (response["response"].AsDic.ContainsKey ("result") && response["response"].AsDic["result"].AsValue.ToString() == "ERROR") {
					
					EditorUtility.DisplayDialog ("Exception", response.ToString(), "Close");
					return false;
				}
			}

			return true;
		}
	}
}
