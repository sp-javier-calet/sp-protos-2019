using Zenject;
using UnityEngine;

public class BaseInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<MonoBehaviour>().ToSingleMethod<MonoBehaviour>(CreateMonoBehaviour);
        Container.Rebind<Transform>().ToSingleInstance(Container.DefaultParent);
    }

    MonoBehaviour CreateMonoBehaviour(InjectContext ctx)
    {
        var go = ctx.Container.DefaultParent.gameObject;
        var behaviour = go.GetComponent<MonoBehaviour>();
        if(behaviour == null)
        {
            behaviour = go.AddComponent<MonoBehaviour>();
        }
        return behaviour;
    }
}
