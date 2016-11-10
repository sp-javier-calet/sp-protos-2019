using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Lockstep;
using UnityEngine.UI;
using SocialPoint.Network;

public class LockstepOptimizationView : MonoBehaviour 
{
    public static readonly IntCircularBuffer TurnDataBuffer = new IntCircularBuffer();
    public static readonly IntCircularBuffer TotalPhotonDataBuffer = new IntCircularBuffer();

    FloatCircularBuffer turnDataBufferPerFrameAvg = new FloatCircularBuffer();
    FloatCircularBuffer photonDataBufferPerFrameAvg = new FloatCircularBuffer();

    int _turnDataBufferLastIdx = 0;
    float _turnDataBitsPerSecond = 0;
    [SerializeField]
    Text _turnDataBitsPerSecondText;

    int _turnDataBufferLastIdxPerFrame = 0;
    [SerializeField]
    Text _turnDataBitsPerFrameText;

    int _photonDataBufferLastIdx = 0;
    float _totalPhotonDataPerSecond = 0;
    [SerializeField]
    Text _totalPhotonDataPerSecondText;

    int _photonDataBufferLastIdxPerFrame = 0;
    [SerializeField]
    Text _totalPhotonDataPerFrameText;

    [SerializeField]
    Text _sendIsReliableTex;

    [SerializeField]
    Text _maxEmptyTurns;

    void Awake()
    {
        RefreshUI();

        StartCoroutine(ShowSendBytesCo(true));
        StartCoroutine(ShowSendBytesCo(false));
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
        _turnDataBitsPerSecondText.text = (8f * _turnDataBitsPerSecond).ToString();
        _totalPhotonDataPerSecondText.text = (8f * _totalPhotonDataPerSecond).ToString();

        _turnDataBitsPerFrameText.text = (8f * turnDataBufferPerFrameAvg.GetAvg(10)).ToString();
        _totalPhotonDataPerFrameText.text = (8f * photonDataBufferPerFrameAvg.GetAvg(10)).ToString();

        _sendIsReliableTex.text = PhotonNetworkBase.SendReliable.ToString();

        _maxEmptyTurns.text = LockStepNetworkCommon.MaxEmptyTurns.ToString();
    }

    IEnumerator ShowSendBytesCo(bool isRealTime)
    {
        float lastTime = Time.time;

        while(true)
        {
            if(isRealTime)
            {
                yield return null;

                turnDataBufferPerFrameAvg.Add(GetAveragePerSecond(TurnDataBuffer, ref _turnDataBufferLastIdxPerFrame, 1f));
                photonDataBufferPerFrameAvg.Add(GetAveragePerSecond(TotalPhotonDataBuffer, ref _photonDataBufferLastIdxPerFrame, 1f));

            }
            else
            {
                yield return new WaitForSeconds(1f);
                float time = Time.time;

                _turnDataBitsPerSecond = GetAveragePerSecond(TurnDataBuffer, ref _turnDataBufferLastIdx, time - lastTime);
                _totalPhotonDataPerSecond = GetAveragePerSecond(TotalPhotonDataBuffer, ref _photonDataBufferLastIdx, time - lastTime);
             
                lastTime = time;
            }

            RefreshUI();
        }
    }

    float GetAveragePerSecond(IntCircularBuffer buffer, ref int bufferStartIdx, float deltaTime)
    {
        int totalBytes = buffer.GetSum(bufferStartIdx, buffer.Count);
        float average = ((float)totalBytes) / (deltaTime);
        bufferStartIdx = buffer.Count;

        return average;
    }

    float GetAveragePerSecond<T>(FloatCircularBuffer buffer, ref int bufferStartIdx, float deltaTime)
    {
        float totalBytes = buffer.GetSum(bufferStartIdx, buffer.Count);
        float average = ((float)totalBytes) / (deltaTime);
        bufferStartIdx = buffer.Count;

        return average;
    }

    public void OnSendReliableClicked()
    {
        PhotonNetworkBase.SendReliable = !PhotonNetworkBase.SendReliable;
        RefreshUI();
    }

    public void OnSubstractTurnsClicked()
    {
        LockStepNetworkCommon.MaxEmptyTurns--;
        RefreshUI();
    }

    public void OnAddTurnsClicked()
    {
        LockStepNetworkCommon.MaxEmptyTurns++;
        RefreshUI();
    }
}
