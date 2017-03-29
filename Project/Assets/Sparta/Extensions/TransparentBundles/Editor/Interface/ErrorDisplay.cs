using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class ErrorDisplay
    {
        private static Dictionary<string, string> ErrorLog = new Dictionary<string, string>();

        private static Dictionary<ErrorType, string> ErrorMessages = new Dictionary<ErrorType, string>
        {
            { ErrorType.assetNotFoundInBundle, "Transparent Bundles - Error - The bundle '{0}' have an asset with the following GUID: '{1}' that was not found in the project. Please, make sure your project is up-to-date and that the asset has not been removed. If the issue persists, please, contact the transparent bundles team: " + Config.ContactMail+"\n"},
            { ErrorType.assetNotFound, "Transparent Bundles - Error - The asset '{0}' with the following GUID: '{1}' was not found in the project. Please, make sure that the asset was not deleted from the project. \n\nVisit the following link for more info: \n" + Config.HelpUrl+"\n"},
            { ErrorType.parentBundleNotFound, "Transparent Bundles - Error - The parent bundle '{0}' was not found in the bundle list. Please, contact the transparent bundles team: " + Config.ContactMail + "\n"},
            { ErrorType.assetPendingToCommit, "Transparent Bundles - Error - Some asset are pending to be commited and pushed to GIT:\n\n\n{0}\nPlease, make sure that you updload all the pending assets before creating or updating bundles. \n\nVisit the following link for more info: \n" + Config.HelpUrl+"\n"},
            { ErrorType.bundleNotDownloadable, "Transparent Bundles - Error - The bundle '{0}' doesn't have a proper URL or Name assigned. Please, contact the transparent bundles team: " + Config.ContactMail + "\n"}
        };

        public static void FlushErrorCache()
        {
            ErrorLog = new Dictionary<string, string>();
        }

        public static string DisplayError(ErrorType errorType, bool showPopup = false, bool showOnce = false, bool warning = false, string itemName = "", string GUID = "")
        {
            string errorText = "";

            if(GUID.Length == 0)
            {
                errorText = string.Format(ErrorMessages[errorType], itemName);
            }
            else
            {
                errorText = string.Format(ErrorMessages[errorType], itemName, GUID);
            }

            if(showOnce)
            {
                string errorCode = errorType.ToString() + itemName + GUID;
                if(!ErrorLog.ContainsKey(errorCode))
                {
                    ErrorLog.Add(errorCode, errorText);
                    if(showPopup)
                    {
                        DisplayPopupError(errorText, warning);
                    }
                    else
                    {
                        DisplayConsoleError(errorText, warning);
                    }
                }
            }
            else
            {
                if(showPopup)
                {
                    DisplayPopupError(errorText, warning);
                }
                else
                {
                    DisplayConsoleError(errorText, warning);
                }
            }

            return errorText;
        }

        private static void DisplayConsoleError(string errorText, bool warning = false)
        {
            if(warning)
            {
                UnityEngine.Debug.LogWarning(errorText);
            }
            else
            {
                UnityEngine.Debug.LogError(errorText);
            }
        }

        private static void DisplayPopupError(string errorText, bool warning = false)
        {
            DisplayConsoleError(errorText, warning);
            EditorUtility.DisplayDialog("Bundle issue", errorText, "Close");
        }
    }

    public enum ErrorType
    {
        assetNotFoundInBundle,
        assetNotFound,
        parentBundleNotFound,
        assetPendingToCommit,
        bundleNotDownloadable
    }
}
