using System;
using UnityEngine.UI;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public sealed class AdminPanelAttributes : IAdminPanelGUI, IAdminPanelConfigurer, IDisposable
    {
        Text _textComponent;

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Attributes", this));
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            layout.CreateLabel("Attributes");
            layout.CreateButton("Parse Big File", OnParseBigFile);
            _textComponent = layout.CreateVerticalScrollLayout().CreateTextArea("");
        }

        public void Dispose()
        {
        }

        void OnParseBigFile()
        {
            var sb = StringUtils.StartBuilder();
            string jsonPath = UnityEngine.Application.streamingAssetsPath + "/BigJsonFile.json";
            string json = SocialPoint.IO.FileUtils.ReadAllText(jsonPath);
            long startTS = SocialPoint.Utils.TimeUtils.TimestampMilliseconds;
            UnityEngine.Profiler.BeginSample("LitJson Parsing");
            var litJsonParser = new LitJsonAttrParser();
            Attr resultLitJson = litJsonParser.ParseString(json);
            UnityEngine.Profiler.EndSample();
            long litJsonTS = SocialPoint.Utils.TimeUtils.TimestampMilliseconds;
            if(resultLitJson != null)
            {
                sb.AppendLine(string.Format("LitJson parsing time: {0}ms", litJsonTS - startTS));
            }
            else
            {
                sb.AppendLine("LitJson parsing failed");
            }


            UnityEngine.Profiler.BeginSample("FastJson Parsing");
            var fastJsonParser = new FastJsonAttrParser();
            Attr resultFastJson = fastJsonParser.ParseString(json);
            UnityEngine.Profiler.EndSample();
            long fastJsonTS = SocialPoint.Utils.TimeUtils.TimestampMilliseconds;
            if(resultFastJson != null)
            {
                sb.AppendLine(string.Format("FastJson parsing time: {0}ms", fastJsonTS - litJsonTS));
            }
            else
            {
                sb.AppendLine("FastJson parsing failed");
            }

            _textComponent.text = StringUtils.FinishBuilder(sb);
        }
    }
}
