using UnityEngine;
using SocialPoint.GUIControl;
using UnityEngine.UI;
using System;

public class ScrollViewTestingHelper : MonoBehaviour 
{
    [SerializeField]
    InputField _inputText;

    [SerializeField]
    ScrollViewExample _scrollViewExtension;

    public void OnScrollToTop()
    {
        if(_scrollViewExtension != null)
        {
            _scrollViewExtension.ScrollToTop();
        }
    }

    public void OnScrollToBottom()
    {
        if(_scrollViewExtension != null)
        {
            _scrollViewExtension.ScrollToBottom();
        }
    }

    public void OnScrollToInput()
    {
        if(_scrollViewExtension != null)
        {
            int parsedValue;
            if(Int32.TryParse(_inputText.text, out parsedValue))
            {
                _scrollViewExtension.ScrollToElement(parsedValue);
            }
            else
            {
                Debug.Log("Input field value cannot be converted to an Integer");
            }
        }
    }
}
