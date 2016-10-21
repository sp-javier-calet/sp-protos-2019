using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Utils
{
    public interface ISimulateable
    {
        void Simulate(long timestamp);

        long KeyTimestamp{ get; }
    }

    static class SimulateableUtils
    {
        static public long KeyTimestamp<T>(List<T> sims) where T : ISimulateable
        {
            if(sims == null || sims.Count == 0)
            {
                return 0;
            }
            var keySimulateable = sims.Aggregate((earlierTileNode, nextTileNode) => {
                if(earlierTileNode.KeyTimestamp == 0)
                {
                    return nextTileNode;
                }
                if(nextTileNode.KeyTimestamp == 0)
                {
                    return earlierTileNode;
                }
                return earlierTileNode.KeyTimestamp < nextTileNode.KeyTimestamp ? earlierTileNode : nextTileNode;
            });
            return keySimulateable.KeyTimestamp;
        }
    }
}
