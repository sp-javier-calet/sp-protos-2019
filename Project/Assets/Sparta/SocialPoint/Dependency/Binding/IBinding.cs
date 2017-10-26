using System.Collections.Generic;

namespace SocialPoint.Dependency
{
    public interface IBinding
    {
        BindingKey Key { get; }

        bool Resolved { get; }

        int Priority{ get; }

        object Resolve();

        void OnResolved();

    }

    public class BindingComparer : IComparer<IBinding>
    {
        public int Compare(IBinding x, IBinding y)
        {
            return y.Priority - x.Priority;
        }
    }
}