#pragma warning disable 414

using UnityEngine;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

public class GameAudioManager : MonoBehaviour
{

#region SINGLETON

    static GameAudioManager s_pInstance = null;

    public static GameAudioManager SharedInstance
    {
        get
        {
            if(!s_bInitialised)
            {
                if(s_pInstance == null)
                {
                    GameObject kGameLoop = GameObject.Find("GameLoop/Audio");

                    if(kGameLoop != null)
                    {
                        s_pInstance = GameObject.Find("GameLoop/Audio").GetComponent<GameAudioManager>();
                    }
                }
            }

            return s_pInstance;
        }
    }

#endregion SINGLETON

#region PRIVATE ATTRIBUTES

    public AudioMixer[] m_kMusicMixers;

    const int c_iMaxInstancesOfSameSound = 1;
    const float c_iMaxInstancesOfSameSoundByTimeInterval = 0.05f;

    AudioSource[] m_kAudioSources = null;
    const int c_iAudioChannels = 15;

    Dictionary<string, AudioClip> m_kAudioClips;

    static bool s_bInitialised = false;

#endregion PRIVATE ATTRIBUTES


#region PUBLIC METHODS

    public void Initialise()
    {
        if(!s_bInitialised)
        {
            m_kAudioClips = new Dictionary<string, AudioClip>();

            GameObject pGameObj = transform.gameObject;
            if(pGameObj)
            {
                m_kAudioSources = new AudioSource[c_iAudioChannels];
                for(int i = 0; i < c_iAudioChannels; ++i)
                {
                    m_kAudioSources[i] = pGameObj.AddComponent<AudioSource>();
                    m_kAudioSources[i].playOnAwake = false;
                    m_kAudioSources[i].Stop();
                }
            }

            s_bInitialised = true;
        }
    }

    public void AddAudioClip(string strAudioID)
    {
        if(!m_kAudioClips.ContainsKey(strAudioID))
        {
            AudioClip pAudioClip = Resources.Load<AudioClip>(strAudioID);
            if(pAudioClip != null)
            {
                m_kAudioClips[strAudioID] = pAudioClip;
            }
        }
    }

    public AudioClip GetAudioClip(string strAudioClipID)
    {
        if(m_kAudioClips != null)
        {
            if(!m_kAudioClips.ContainsKey(strAudioClipID))
            {
                AddAudioClip(strAudioClipID);
            }

            if(m_kAudioClips.ContainsKey(strAudioClipID))
            {
                return m_kAudioClips[strAudioClipID];
            }
        }

        return null;
    }

    public void RemoveAudioClip(string strAudioClipID)
    {
        if(m_kAudioClips != null)
        {
            if(m_kAudioClips.ContainsKey(strAudioClipID))
            {
                if(m_kAudioClips[strAudioClipID] != null)
                {
                    m_kAudioClips[strAudioClipID].UnloadAudioData();
                }

                m_kAudioClips.Remove(strAudioClipID);
            }
        }
    }

    public AudioMixer GetSFXAudioMixer()
    {
        if(m_kMusicMixers.Length > 0)
        {
            return m_kMusicMixers[0];
        }

        return null;
    }

    public int PlaySound(string strSoundID, bool loop = false, float volume = 1.0f)
    {
        if(strSoundID.Length > 0)
        {
            AudioClip pAudioClip = GetAudioClip(strSoundID);
            if(pAudioClip != null)
            {
                if(IsAudioClipEligibleForPlay(pAudioClip))
                {
                    if(CheckMaxInstancesOfSameSound(ref pAudioClip))
                    {
                        int iSoundChannel = GetAvailableSoundChannel();
                        if(m_kAudioSources != null && iSoundChannel != -1 && m_kAudioSources[iSoundChannel] != null)
                        {
                            m_kAudioSources[iSoundChannel].clip = pAudioClip;
                            m_kAudioSources[iSoundChannel].loop = loop;

                            m_kAudioSources[iSoundChannel].Play();
                            m_kAudioSources[iSoundChannel].volume = volume;

                            return iSoundChannel;
                        }
                    }
                }
            }
        }

        return -1;
    }

    public void StopSound(int iChannel)
    {
        if(iChannel >= 0 && iChannel < m_kAudioSources.Length)
        {
            if(m_kAudioSources[iChannel] && m_kAudioSources[iChannel].isPlaying)
            {
                m_kAudioSources[iChannel].Stop();
                m_kAudioSources[iChannel].volume = 1.0f;
                m_kAudioSources[iChannel].pitch = 1.0f;
            }
        }
    }

    public void StopAllSounds()
    {
        for(int i = 0; i < m_kAudioSources.Length; ++i)
        {
            if(m_kAudioSources[i])
            {
                if(m_kAudioSources[i].isPlaying)
                {
                    m_kAudioSources[i].Stop();
                    m_kAudioSources[i].volume = 1.0f;
                    m_kAudioSources[i].pitch = 1.0f;
                }

                m_kAudioSources[i].clip = null;
            }
        }

        if(m_kAudioClips != null)
        {
            List<string> kAudioClipKeys = new List<string>(m_kAudioClips.Keys);
            for(int i = 0; i < kAudioClipKeys.Count; ++i)
            {
                RemoveAudioClip(kAudioClipKeys[i]);
            }
        }
    }

    public void StopAllMusic()
    {
        AudioSource[] kAllAudioSources = FindObjectsOfType<AudioSource>();
        if(kAllAudioSources != null && kAllAudioSources.Length > 0)
        {
            for(int j = 0; j < kAllAudioSources.Length; ++j)
            {
                bool bAudioSourceFoundInSounds = false;
                for(int i = 0; i < m_kAudioSources.Length; ++i)
                {
                    if(m_kAudioSources[i] && m_kAudioSources[i] == kAllAudioSources[j])
                    {
                        bAudioSourceFoundInSounds = true;
                        break;
                    }
                }

                if(!bAudioSourceFoundInSounds)
                {
                    if(kAllAudioSources[j].isPlaying)
                    {
                        kAllAudioSources[j].Stop();
                        kAllAudioSources[j].volume = 1.0f;
                        kAllAudioSources[j].pitch = 1.0f;
                    }

                    kAllAudioSources[j].clip = null;
                }
            }
        }
    }

#endregion PUBLIC METHODS

#region PRIVATE METHODS

    bool IsAudioClipEligibleForPlay(AudioClip clip, float maxDelay = 0.005f)
    {
        if(clip == null)
            return false;

        for(int i = 0; i < m_kAudioSources.Length; ++i)
        {
            if(m_kAudioSources[i]
                && m_kAudioSources[i].clip == clip
                && m_kAudioSources[i].isPlaying
                && m_kAudioSources[i].time < maxDelay)
            {
                return false;
            }
        }

        return true;
    }

    int GetAvailableSoundChannel()
    {
        for(int i = 0; i < m_kAudioSources.Length; ++i)
        {
            if(m_kAudioSources[i] && !m_kAudioSources[i].isPlaying)
            {
                return i;
            }
        }

        return -1;
    }

    bool CheckMaxInstancesOfSameSound(ref AudioClip kAudioClip)
    {
        int iCurrentInstances = 0;

        for(int i = 0; i < m_kAudioSources.Length; ++i)
        {
            if(m_kAudioSources[i] && m_kAudioSources[i].isPlaying)
            {
                if(m_kAudioSources[i].clip == kAudioClip && m_kAudioSources[i].time < c_iMaxInstancesOfSameSoundByTimeInterval)
                {
                    iCurrentInstances++;
                }
            }
        }

        return (iCurrentInstances < c_iMaxInstancesOfSameSound);
    }

#endregion PRIVATE METHODS

#region UNITY METHODS

    void Awake()
    {
        s_pInstance = this;
        s_pInstance.Initialise();

        DontDestroyOnLoad(this);
    }

#endregion UNITY METHODS

}
