namespace SocialPoint.Dependency
{
    public interface IListener
    {
        void OnResolved(IBinding binding, object instance);
    }
}