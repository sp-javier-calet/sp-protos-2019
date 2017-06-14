using SocialPoint.IO;

namespace SocialPoint.BehaviorTree
{
    public class BehaviorReaderWrapper : ReaderWrapper, BehaviorDesigner.Runtime.IReader
    {
        public BehaviorReaderWrapper(IReader reader): base(reader)
        {
        }
    }
}
