using SocialPoint.AWS.S3;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace AssetBundleGraph
{
    public class ValidatorSelectView : WindowView<ValidatorLogWindow>
    {
        public List<S3ListObject> s3Items;
        public Vector2 scrollPos;
        public ValidatorLog localValidator = null;
        ValidatorLog validatorInViewWindow;

        public ValidatorSelectView(ValidatorLogWindow parent) : base(parent)
        {
        }

        public override void OnEnableMethod()
        {
            s3Items = ValidatorController.ListFromS3();
            if(ValidatorLog.IsValidatorLogAvailableAtDisk())
            {
                localValidator = ValidatorLog.LoadFromDisk();
            }

            validatorInViewWindow = parentWindow.GetView<ValidatorView>().currentLogInWindow;
        }

        public override void OnFocusMethod()
        {
        }

        public override void OnGUIMethod()
        {
            using(var scroll = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                EditorGUILayout.Space();
                scrollPos = scroll.scrollPosition;

                bool localPainted = false;

                for(int i = 0; i < s3Items.Count; i++)
                {
                    var obj = s3Items[i];
                    var date = ValidatorController.GetS3ValidationDate(obj.Key);

                    if(localValidator != null && !localPainted)
                    {
                        if(localValidator.lastExecuted > date)
                        {
                            DrawRow("Local", localValidator.FormatedDate, validatorInViewWindow.lastExecuted == localValidator.lastExecuted, OnClickLocalValidator);
                            localPainted = true;
                        }
                    }

                    var dateStr = date.ToLocalTime().ToString(ValidatorLog.VIEW_DATE_FORMAT, CultureInfo.InvariantCulture);
                    DrawRow("Remote", dateStr, validatorInViewWindow.lastExecuted == date, () => OnClickRemoteValidator(obj.Key));
                }

                if(!localPainted)
                {
                    DrawRow("Local", localValidator.FormatedDate, validatorInViewWindow.lastExecuted == localValidator.lastExecuted, OnClickLocalValidator);
                }
            }
        }

        void OnClickLocalValidator()
        {
            parentWindow.ChangeView<ValidatorView>().LoadValidatorLog(localValidator);

        }

        void OnClickRemoteValidator(string s3Key)
        {
            var text = ValidatorController.DownloadFromS3(s3Key);
            parentWindow.ChangeView<ValidatorView>().LoadValidatorLog(ValidatorLog.LoadFromText(text, s3Key));
        }

        void DrawRow(string text, string date, bool isAlreadySelected, Action OnClickAction)
        {
            GUIStyle box = new GUIStyle("box");
            GUI.color = isAlreadySelected ? Color.yellow : Color.white;
            using(var hScope = new EditorGUILayout.HorizontalScope(box))
            {
                EditorGUILayout.Space();

                GUILayout.Label(text);
                GUI.color = Color.white;
                GUILayout.Label(date, GUILayout.Width(150));

                GUI.enabled = !isAlreadySelected;
                if(GUILayout.Button("Load", GUILayout.MaxWidth(100)))
                {
                    OnClickAction();
                }
                GUI.enabled = true;
            }
        }
    }
}
