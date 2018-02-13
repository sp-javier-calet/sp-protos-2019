namespace SocialPoint.Dependency
{
    public interface IListener
    {
        void OnResolved(IBinding binding, object instance);
    }

    public class BaseListener : IListener
    {
        readonly DependencyContainer _container;
        BindingKey _resolve;

        public BaseListener(DependencyContainer container)
        {
            _container = container;
        }

        public void ThenResolve<K>(string tag = null)
        {
            _resolve = new BindingKey(typeof(K), tag);
        }

        void IListener.OnResolved(IBinding binding, object instance)
        {
            OnResolved(binding, instance);
        }

        protected virtual void OnResolved(IBinding binding, object instance)
        {
            if(_resolve.Type != null)
            {
                _container.Resolve(_resolve.Type, _resolve.Tag);
            }
        }
    }
}