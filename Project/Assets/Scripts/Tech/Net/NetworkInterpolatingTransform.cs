using UnityEngine;
using UnityEngine.Networking;
 
public class NetworkInterpolatingTransform : NetworkBehaviour
{
    [SerializeField]
    private float _posLerpRate = 15;
    [SerializeField]
    private float _rotLerpRate = 15;
    [SerializeField]
    private float _posThreshold = 0.1f;
    [SerializeField]
    private float _rotThreshold = 1f;
    [SerializeField]
    private float _networkSendInterval = 0.01f;

    [SyncVar]
    private Vector3 _lastPosition;

    [SyncVar]
    private Vector3 _lastRotation;

    private void Start()
    {
        _lastPosition = transform.position;
        _lastRotation = transform.eulerAngles;
    } 

    void Update()
    {
        if (IsMaster)
            return;

        InterpolatePosition();
        InterpolateRotation();
    }

    bool IsMaster => isLocalPlayer || hasAuthority;

    void FixedUpdate()
    {
        if (!IsMaster)
            return;

        var posChanged = IsPositionChanged();

        if (posChanged)
        {
            CmdSendPosition(transform.position);
            _lastPosition = transform.position;
        }

        var rotChanged = IsRotationChanged();

        if (rotChanged)
        {
            CmdSendRotation(transform.localEulerAngles);
            _lastRotation = transform.localEulerAngles;
        }
    }

    private void InterpolatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, _lastPosition, Time.deltaTime * _posLerpRate);
    }

    private void InterpolateRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(_lastRotation), Time.deltaTime * _rotLerpRate);
    }

    [Command(channel = Channels.DefaultUnreliable)]
    private void CmdSendPosition(Vector3 pos)
    {
        _lastPosition = pos;
    }

    [Command(channel = Channels.DefaultUnreliable)]
    private void CmdSendRotation(Vector3 rot)
    {
        _lastRotation = rot;
    }

    private bool IsPositionChanged()
    {
        return Vector3.Distance(transform.position, _lastPosition) > _posThreshold;
    }

    private bool IsRotationChanged()
    {
        return Vector3.Distance(transform.localEulerAngles, _lastRotation) > _rotThreshold;
    }

    public override int GetNetworkChannel()
    {
        return Channels.DefaultUnreliable;
    }

    public override float GetNetworkSendInterval()
    {
        return _networkSendInterval;
    }
}