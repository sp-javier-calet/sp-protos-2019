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
        public List<S3ListObject> S3Items;
        ValidatorLog _localValidator = null;
        Vector2 _scrollPos;
        ValidatorLog _validatorInViewWindow;

        public ValidatorSelectView(ValidatorLogWindow parent) : base(parent)
        {
        }

        public override void OnEnableMethod()
        {
            S3Items = ValidatorController.ListFromS3();

            if(ValidatorLog.IsValidatorLogAvailableAtDisk())
            {
                _localValidator = ValidatorLog.LoadFromDisk();
            }

            _validatorInViewWindow = parentWindow.GetView<ValidatorView>().CurrentLogInWindow;
        }

        public override void OnFocusMethod()
        {
        }

        public override void OnGUIMethod()
        {
            using(var scroll = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                EditorGUILayout.Space();
                _scrollPos = scroll.scrollPosition;

                bool localPainted = false;

                for(int i = 0; i < S3Items.Count; i++)
                {
                    var obj = S3Items[i];
                    var date = ValidatorController.GetS3ValidationDate(obj.Key);

                    if(_localValidator != null && !localPainted)
                    {
                        if(_localValidator.lastExecuted > date)
                        {
                            DrawRow("Local", _localValidator.FormatedDate, _validatorInViewWindow.lastExecuted == _localValidator.lastExecuted, OnClickLocalValidator);
                            localPainted = true;
                        }
                    }

                    var dateStr = date.ToLocalTime().ToString(ValidatorLog.VIEW_DATE_FORMAT, CultureInfo.InvariantCulture);
                    DrawRow("Remote", dateStr, _validatorInViewWindow.lastExecuted == date, () => OnClickRemoteValidator(obj.Key));
                }

                if(!localPainted)
                {
                    DrawRow("Local", _localValidator.FormatedDate, _validatorInViewWindow.lastExecuted == _localValidator.lastExecuted, OnClickLocalValidator);
                }

                if(S3Items.Count == 0)
                {
                    GUILayout.Label("No remote info could be found", EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        void OnClickLocalValidator()
        {
            parentWindow.ChangeView<ValidatorView>().LoadValidatorLog(_localValidator);

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
