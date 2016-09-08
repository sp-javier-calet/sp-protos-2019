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
        _lockstep.AddPendingCommand(new ClickCommand(
            (Fix64)p.x, (Fix64)p.y, (Fix64)p.z,
            _lockstep.ExecutionTurn, _model));
    }
}