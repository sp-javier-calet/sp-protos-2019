using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using SocialPoint.Base;

public class UIScrollRectPagination : MonoBehaviour 
{
    [Header("Groups")]
    [SerializeField]
    GameObject _navigationGroup;

    [SerializeField]
    GameObject _paginationGroup;

    [Header("Pagination Buttons")]
    [SerializeField]
    GameObject _paginationButtonPrefab;

    [SerializeField]
    Sprite _imageSelected;

    [SerializeField]
    Sprite _imageUnSelected;

    [Header("Functionallity")]
    [SerializeField]
    bool _useNavigationButtons;
    public bool UseNavigationButtons
    {
        set
        {
            _navigationGroup.SetActive(value);
            _useNavigationButtons = value;
        }
    }

    [SerializeField]
    bool _usePaginationButtons;
    public bool UsePaginationButtons
    {
        set
        {
            _paginationGroup.SetActive(value);
            _usePaginationButtons = value;
        }
    }

    Transform _parent;
    Action _scrollToPreviousCell;
    Action _scrollToNextCell;
    Action<int> _scrollToSelectedCell;

    List<Image> _paginationButtonImages = new List<Image>();

    void Awake()
    {
        _parent = _paginationGroup.transform;

        if(_useNavigationButtons && _navigationGroup == null)
        {
            throw new UnityException("You are trying to 'Use Navigation Buttons' but 'Navigation Group' is not assigned");
        }

        if(_usePaginationButtons && _paginationGroup == null)
        {
            throw new UnityException("You are trying to 'Use Pagination Buttons' but 'Pagination Group' is not assigned");
        }
    }
        
    public void Init(int count, int _selectedIndex, Action scrollToPreviousCell, Action scrollToNextCell, Action<int> scrollToSelectedCell)
    {
        _scrollToPreviousCell = scrollToPreviousCell;
        _scrollToNextCell = scrollToNextCell;
        _scrollToSelectedCell = scrollToSelectedCell;

        if(_usePaginationButtons)
        {
            // Check if we need to create too much pagination buttons
            for(int i = 0; i < count; ++i)
            {
                InstantiateNewPaginationButton(_paginationButtonPrefab, i);
            }

            SetSelectedButton(_selectedIndex);
        }
    }

    public void Reload(int count, int _selectedIndex)
    {
        if(_usePaginationButtons)
        {
            for(int i = _parent.childCount; i > count; --i)
            {
                RemovePaginationButton(_parent.GetChild(i).gameObject);
            }

            for(int i = _parent.childCount + 1; i <= count; ++i)
            {
                InstantiateNewPaginationButton(_paginationButtonPrefab, i);
            }

            SetSelectedButton(_selectedIndex);
        }
    }

    void InstantiateNewPaginationButton(GameObject prefab, int index)
    {
        var go = GameObject.Instantiate(prefab);
        if(go != null)
        {
            var trans = go.transform;
            trans.SetParent(_parent);
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;

            go.SetActive(true);

            AddPaginationButton(go);
            SetPaginationButtonListener(go, index);
        }
    }

    void RemovePaginationButton(GameObject go)
    {
        go.DestroyAnyway();
    }

    void AddPaginationButton(GameObject go)
    {
        if(go != null)
        {
            var image = go.GetComponent<Image>();
            if(image != null)
            {
                _paginationButtonImages.Add(image);
            }
        }
    }

    void SetPaginationButtonListener(GameObject go, int index)
    {
        var button = go.GetComponent<Button>();
        if(button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnPaginationButtonClicked(index));
        }
    }

    public virtual void SetSelectedButton(int index)
    {
        if(_usePaginationButtons)
        {
            for(int i = 0; i < _paginationButtonImages.Count; ++i)
            {
                _paginationButtonImages[i].sprite = (i == index ? _imageSelected : _imageUnSelected);
            }
        }
    }

    #region Unity UI Button Click Events

    public void OnPreviousButtonClicked()
    {
        if(_scrollToPreviousCell != null && _useNavigationButtons)
        {
            _scrollToPreviousCell();
        }
    }

    public void OnNextButtonClicked()
    {
        if(_scrollToNextCell != null && _useNavigationButtons)
        {
            _scrollToNextCell();
        }
    }

    public void OnPaginationButtonClicked(int index)
    {
        if(_scrollToSelectedCell != null && _usePaginationButtons)
        {
            _scrollToSelectedCell(index);
        }
    }

    #endregion
}
