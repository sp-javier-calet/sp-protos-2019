using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SocialPoint.Lockstep;
using SocialPoint.Dependency;
using SocialPoint.Utils;
using FixMath.NET;

public class GameLockstepClientBehaviour : MonoBehaviour, IPointerClickHandler
{
    ClientLockstepController _lockstep;
    LockstepModel _model;

    [SerializeField]
    Slider _manaSlider;

    [SerializeField]
    GameObject _unitPrefab;

    [SerializeField]
    GameObject _loadingPrefab;

    void Start()
    {
        _lockstep = ServiceLocator.Instance.Resolve<ClientLockstepController>();
        _lockstep.Simulate += Simulate;
        _model = ServiceLocator.Instance.Resolve<LockstepModel>();
        _model.OnInstantiate += OnInstantiate;
        _lockstep.RegisterCommandLogic<ClickCommand>(new ClickCommandLogic(_model));

        _lockstep.Start(TimeUtils.TimestampMilliseconds);
    }

    void OnDestroy()
    {
        _lockstep.Simulate -= Simulate;
        _model.OnInstantiate -= OnInstantiate;
    }

    void Simulate(long tsmillis)
    {
        _model.Simulate(tsmillis);
    }

    void OnInstantiate(Fix64 x, Fix64 y, Fix64 z)
    {
        SocialPoint.ObjectPool.ObjectPool.Spawn(_unitPrefab, transform, 
            new Vector3((float)x, (float)y, (float)z), Quaternion.identity);
    }

    void Update()
    {
        _manaSlider.value = _model.ManaView;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var p = eventData.pointerPressRaycast.worldPosition;
        var cmd = new ClickCommand(
                      (Fix64)p.x, (Fix64)p.y, (Fix64)p.z);

        var loading = SocialPoint.ObjectPool.ObjectPool.Spawn(
                          _loadingPrefab, transform, p, Quaternion.identity);
        _lockstep.AddPendingCommand(cmd, (c) => FinishLoading(loading));
    }

    public void FinishLoading(GameObject loading)
    {
        SocialPoint.ObjectPool.ObjectPool.Recycle(loading);
    }



}