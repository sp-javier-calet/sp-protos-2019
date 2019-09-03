//-----------------------------------------------------------------------
// TimelineControl.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class TimelineControl : MonoBehaviour
{
    public PlayableDirector Director;
    public Slider TimeControlSlider;
    public TextMeshProUGUI MaxTimeLabel;
    public TextMeshProUGUI CurrentTimeLabel;

    void Start()
    {
        TimeControlSlider.maxValue = (float)Director.duration;
        MaxTimeLabel.text = Director.duration.ToString("F");
    }

    void LateUpdate()
    {
        double time;
        CurrentTimeLabel.text = (time = Director.time).ToString("F");
        TimeControlSlider.value = (float)time;
    }

    public void PlayTimeline()
    {
        StopCoroutines();
        Director.Play();
    }

    public void StopTimeline()
    {
        PauseTimeline();
        Director.time = 0f;
        Director.Evaluate();
    }

    public void PauseTimeline()
    {
        StopCoroutines();
        Director.Pause();
    }

    public void Rewind()
    {
        PauseTimeline();
        StartCoroutine(DoRewindStep());
    }

    public void FastForward()
    {
        PauseTimeline();
        StartCoroutine(DoFastForwardStep());
    }

    public void OnSliderValueChanged()
    {
        Director.time = TimeControlSlider.value;
        Director.Evaluate();
    }

    void StopCoroutines()
    {
        StopCoroutine(DoRewindStep());
        StopCoroutine(DoFastForwardStep());
    }

    private IEnumerator DoRewindStep()
    {
        yield return new WaitForSeconds(0.001f);

        Director.time -= 0.1f;

        if(Director.time < 0f)
        {
            Director.time = 0f;
            Director.Evaluate();
        }
        else
        {
            Director.Evaluate();
            StartCoroutine(DoRewindStep());
        }
    }

    private IEnumerator DoFastForwardStep()
    {
        yield return new WaitForSeconds(0.001f);

        Director.time += 0.1f;

        if(Director.time > Director.duration)
        {
            Director.time = Director.duration;
        }
        else
        {
            Director.Evaluate();
            StartCoroutine(DoFastForwardStep());
        }
    }
}
