using SocialPoint.IO;

namespace SocialPoint.BehaviorTree
{
    public class BehaviorWriterWrapper : WriterWrapper, BehaviorDesigner.Runtime.IWriter
    {
        public BehaviorWriterWrapper(IWriter writer): base(writer)
        {
        }
    }
}
