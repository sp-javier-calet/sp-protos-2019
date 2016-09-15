using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SocialPoint.Lockstep;
using SocialPoint.Dependency;
using SocialPoint.Utils;
using SocialPoint.Pooling;
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
        _lockstep.PendingCommandAdded += AutoConfirmCommand;
        _lockstep.NeedsTurnConfirmation = false;
        _model = ServiceLocator.Instance.Resolve<LockstepModel>();
        _model.OnInstantiate += OnInstantiate;
        _lockstep.Start(TimeUtils.TimestampMilliseconds);
    }

    void OnDestroy()
    {
        _lockstep.PendingCommandAdded -= AutoConfirmCommand;
        _model.OnInstantiate -= OnInstantiate;
    }

    void AutoConfirmCommand(ILockstepCommand c)
    {
        _lockstep.AddConfirmedCommand(c);
    }

    void OnInstantiate(Fix64 x, Fix64 y, Fix64 z)
    {
        ObjectPool.Spawn(_unitPrefab, transform,
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
                      (Fix64)p.x, (Fix64)p.y, (Fix64)p.z,
                      _lockstep.ExecutionTurn, _model);

        var loading = ObjectPool.Spawn(
                          _loadingPrefab, transform, p, Quaternion.identity);

        cmd.Applied += (arg1, arg2) => FinishLoading(loading);
        cmd.Discarded += (obj) => FinishLoading(loading);

        _lockstep.AddPendingCommand(cmd);
    }

    public void FinishLoading(GameObject loading)
    {
        ObjectPool.Recycle(loading);
    }



}