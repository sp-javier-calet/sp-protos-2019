using System.Collections;
using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEngine;

public class CP_Semaphore : MonoBehaviour
{
    enum SemaphoreState
    {
        E_NONE,
        E_INVISIBLE,
        E_WAITING,
        E_SHOWING,
        E_P1,
        E_P2,
        E_P3,
        E_P4
    }

    public List<CP_SemaphoreLight> Lights;

    CP_SceneManager _sceneManager;
    SemaphoreState _semaphoreState;
    long _stateStartTime = 0;
    long _stateRandomTime = 0;

    void Awake()
    {
        ProceedState(SemaphoreState.E_INVISIBLE);
    }

    void ProceedState(SemaphoreState state)
    {
        switch(state)
        {
            case SemaphoreState.E_INVISIBLE:
            {
                for(var i = 0; i < Lights.Count; ++i)
                {
                    if(Lights[i] != null)
                    {
                        Lights[i].SetLightState(CP_SemaphoreLight.LightState.E_DISABLED);
                    }
                }

                break;
            }

            case SemaphoreState.E_WAITING:
            {
                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = 1000;

                break;
            }

            case SemaphoreState.E_SHOWING:
            {
                for(var i = 0; i < Lights.Count; ++i)
                {
                    if(Lights[i] != null)
                    {
                        Lights[i].SetLightState(CP_SemaphoreLight.LightState.E_OFF);
                    }
                }

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = 1000;

                break;
            }

            case SemaphoreState.E_P1:
            {
                Lights[0].SetLightState(CP_SemaphoreLight.LightState.E_RED);

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = 1250;

                break;
            }

            case SemaphoreState.E_P2:
            {
                Lights[1].SetLightState(CP_SemaphoreLight.LightState.E_RED);

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = 1250;

                break;
            }

            case SemaphoreState.E_P3:
            {
                Lights[2].SetLightState(CP_SemaphoreLight.LightState.E_RED);

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = 1250;

                break;
            }

            case SemaphoreState.E_P4:
            {
                Lights[0].SetLightState(CP_SemaphoreLight.LightState.E_GREEN);
                Lights[1].SetLightState(CP_SemaphoreLight.LightState.E_GREEN);
                Lights[2].SetLightState(CP_SemaphoreLight.LightState.E_GREEN);
                Lights[3].SetLightState(CP_SemaphoreLight.LightState.E_GREEN);

                _stateStartTime = TimeUtils.TimestampMilliseconds;
                _stateRandomTime = 1500;

                if(_sceneManager != null)
                {
                    _sceneManager.SetCurrentGameState(CP_SceneManager.GameState.E_PLAYING);
                }

                break;
            }
        }

        _semaphoreState = state;
    }

    public void StartSemaphore(CP_SceneManager sceneManager)
    {
        _sceneManager = sceneManager;

        ProceedState(SemaphoreState.E_WAITING);
    }

    void Update()
    {
        var passedStateTime = (TimeUtils.TimestampMilliseconds > _stateStartTime + _stateRandomTime);

        switch(_semaphoreState)
        {
            case SemaphoreState.E_WAITING:
            {
                if(passedStateTime)
                {
                    ProceedState(SemaphoreState.E_SHOWING);
                }

                break;
            }
            case SemaphoreState.E_SHOWING:
            {
                if(passedStateTime)
                {
                    ProceedState(SemaphoreState.E_P1);
                }

                break;
            }
            case SemaphoreState.E_P1:
            {
                if(passedStateTime)
                {
                    ProceedState(SemaphoreState.E_P2);
                }

                break;
            }
            case SemaphoreState.E_P2:
            {
                if(passedStateTime)
                {
                    ProceedState(SemaphoreState.E_P3);
                }

                break;
            }
            case SemaphoreState.E_P3:
            {
                if(passedStateTime)
                {
                    ProceedState(SemaphoreState.E_P4);
                }

                break;
            }
            case SemaphoreState.E_P4:
            {
                if(passedStateTime)
                {
                    ProceedState(SemaphoreState.E_INVISIBLE);
                }

                break;
            }
        }
    }
}
