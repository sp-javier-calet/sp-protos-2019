using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SlideController : MonoBehaviour
{
    [SerializeField] protected GameObject _slidesContainer;
    [SerializeField] protected GameObject _arrowRight;
    [SerializeField] protected string _tutorialName;
    [SerializeField] protected string _nextSceneName;
    [SerializeField] protected bool _forceAlwaysShow = true;

    private const string TutorialDone = "done";

    private int _index = 0;

    // Use this for initialization
    private void Awake()
    {
        HideSlides();
        CheckTutorialFinished();
        UpdateArrows();

    }

    private void CheckTutorialFinished()
    {
        if(_forceAlwaysShow)
        {
            return;
        }

        var played = PlayerPrefs.GetString(_tutorialName);

        if(played == TutorialDone)
        {
            MoveToNextScene();
        }
    }

    private void MoveToNextScene()
    {
        SceneManager.LoadScene(_nextSceneName);
    }

    private void HideSlides()
    {
        var isFirst = true;
        foreach(Transform child in _slidesContainer.transform)
        {
            if(!isFirst)
            {
                child.gameObject.SetActive(false);
            }

            isFirst = false;
        }
    }

    public void ShowNextSlide()
    {
        if(_index < _slidesContainer.transform.childCount-1)
        {
            _slidesContainer.transform.GetChild(_index).gameObject.SetActive(false);

            _index++;

            _slidesContainer.transform.GetChild(_index).gameObject.SetActive(true);
        }
        else if(_index >= _slidesContainer.transform.childCount-1)
        {
            MoveToNextScene();
        }

        UpdateArrows();
        UpdateTutorialSeen();

    }

    private void UpdateTutorialSeen()
    {
        if(_index >= _slidesContainer.transform.childCount-1)
        {
            PlayerPrefs.SetString(_tutorialName,TutorialDone);
        }
    }

    public void ShowPreviousSlide()
    {
        if(_index > 0)
        {
            _slidesContainer.transform.GetChild(_index).gameObject.SetActive(false);

            _index--;

            _slidesContainer.transform.GetChild(_index).gameObject.SetActive(true);
        }

        UpdateArrows();

    }

    private void UpdateArrows()
    {
        _arrowRight.SetActive(_index < _slidesContainer.transform.childCount);
    }
}
