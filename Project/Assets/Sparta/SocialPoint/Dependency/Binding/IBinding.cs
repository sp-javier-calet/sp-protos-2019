
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
}