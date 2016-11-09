using UnityEngine;
using System.Collections;
using SocialPoint.Lockstep;
using UnityEngine.UI;

public class LockstepOptimizationView : MonoBehaviour 
{
    [SerializeField]
    Text _allowClientSendTurn;

    [SerializeField]
    Text _allowServerSendTurn;

    void Awake()
    {
        RefreshUI();
    }

    public void OnEnableClientSendTurn()
    {
        ClientLockstepController.AllowSendTurn = !ClientLockstepController.AllowSendTurn;
    }

    public void OnEnableServerSendTurn()
    {
        ServerLockstepController.AllowSendTurn = !ServerLockstepController.AllowSendTurn;
    }


    void RefreshUI()
    {
        _allowClientSendTurn.text = ClientLockstepController.AllowSendTurn.ToString();
        _allowServerSendTurn.text = ServerLockstepController.AllowSendTurn.ToString();
    }
}
