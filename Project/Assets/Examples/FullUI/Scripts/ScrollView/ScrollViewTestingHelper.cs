//-----------------------------------------------------------------------
// ScrollViewTestingHelper.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System;

public class ScrollViewTestingHelper : MonoBehaviour
{
    [SerializeField]
    InputField _inputScrollText;

    [SerializeField]
    InputField _inputRemoveText;

    [SerializeField]
    ScrollViewExampleRectExtension _scrollViewExtension;

    public void OnScrollToTop()
    {
        if(_scrollViewExtension != null)
        {
            _scrollViewExtension.ScrollToStartPosition();
        }
    }

    public void OnScrollToBottom()
    {
        if(_scrollViewExtension != null)
        {
            _scrollViewExtension.ScrollToFinalPosition();
        }
    }

    public void OnScrollToInput()
    {
        if(_scrollViewExtension != null)
        {
            int parsedValue;
            if(Int32.TryParse(_inputScrollText.text, out parsedValue))
            {
                _scrollViewExtension.ScrollToCell(parsedValue);
            }
            else
            {
                Debug.Log("Input field value cannot be converted to an Integer");
            }
        }
    }

    public void OnAddCellAtStart()
    {
        if(_scrollViewExtension != null)
        {
            var data = _scrollViewExtension.DataSource.CreateCellData();
            _scrollViewExtension.AddData(data, false);
        }
    }

    public void OnAddCellAtEnd()
    {
        if(_scrollViewExtension != null)
        {
            var data = _scrollViewExtension.DataSource.CreateCellData();
            _scrollViewExtension.AddData(data, true);
        }
    }

    public void OnRemoveAtInput()
    {
        if(_scrollViewExtension != null)
        {
            int parsedValue;
            if(Int32.TryParse(_inputRemoveText.text, out parsedValue))
            {
                _scrollViewExtension.RemoveData(parsedValue);
            }
            else
            {
                Debug.Log("Input field value cannot be converted to an Integer");
            }
        }
    }
}
