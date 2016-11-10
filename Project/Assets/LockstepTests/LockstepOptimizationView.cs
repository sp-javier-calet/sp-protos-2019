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
        RefreshUI();
    }

    public void OnEnableServerSendTurn()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
    }
}
