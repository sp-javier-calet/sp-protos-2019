namespace SocialPoint.Dependency
{
    public interface IListener
    {
        void OnResolved(IBinding binding, object instance);
    }

    public abstract class BaseListener
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

        protected void Trigger()
        {
            if(_resolve.Type != null)
            {
                _container.Resolve(_resolve.Type, _resolve.Tag);
            }
        }
    }
}