using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public class UIScrollRectPagination : MonoBehaviour 
{
    [SerializeField]
    GameObject _paginationButton;

    [SerializeField]
    Sprite _imageSelected;

    [SerializeField]
    Sprite _imageUnSelected;

    [SerializeField]
    bool _usePaginationButtons = true;

    //    LayoutGroup _layoutGroup;
    Transform _parent;
    Action _scrollToPreviousCell;
    Action _scrollToNextCell;
    Action<int> _scrollToSelectedCell;

    List<Image> _paginationButtonImages = new List<Image>();

    void Awake()
    {
//        _layoutGroup = GetComponent<LayoutGroup>();
        _parent = transform;
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
                InstantiateNewPaginationButton(_paginationButton, i);
            }

            SetSelectedButton(_selectedIndex);
        }
    }

    void InstantiateNewPaginationButton(GameObject prefab, int index)
    {
        GameObject go = _paginationButton;
        if(index == 0)
        {
            go.SetActive(true);
        }
        else
        {
            go = Instantiate(prefab);
            if(go != null)
            {
                var trans = go.transform;
                trans.SetParent(_parent);
                trans.localPosition = Vector3.zero;
                trans.localRotation = Quaternion.identity;
                trans.localScale = Vector3.one;

                go.SetActive(true);
            }
        }

        if(go != null)
        {
            AddPaginationButton(go);
            SetPaginationButtonListener(go, index);
        }
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
        if(_scrollToPreviousCell != null)
        {
            _scrollToPreviousCell();
        }
    }

    public void OnNextButtonClicked()
    {
        if(_scrollToNextCell != null)
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
