
namespace SocialPoint.Dependency
{
    public interface IBinding
    {
        BindingKey Key { get; }

        bool Resolved { get; }

        object Resolve();

        void OnResolved();
    }
}