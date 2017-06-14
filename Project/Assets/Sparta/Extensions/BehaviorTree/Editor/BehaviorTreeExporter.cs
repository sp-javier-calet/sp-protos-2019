using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Standalone;
using SocialPoint.Exporter;
using SocialPoint.IO;
using SocialPoint.Base;

namespace SocialPoint.BehaviorTree
{
    public class BehaviorTreeExporter : BaseExporter
    {
        public string ExportName = "BehaviorTree";
        public ExternalBehaviorTree BehaviorTree;

        public override void Export(IFileManager files, Log.ILogger log)
        {            
            var fh = files.Write(ExportName);
            var bwriter = new BehaviorWriterWrapper(fh.Writer);
            BehaviorSourceSerializer.Instance.Serialize(BehaviorTree.BehaviorSource, bwriter);
            fh.CloseStream();
        }

        public override string ToString()
        {
            return "BehaviorTree exporter" + (BehaviorTree != null ? (" [" + BehaviorTree.name + "]") : string.Empty);
        }
    }
}
