//-----------------------------------------------------------------------
// ScreenControllerScrollView.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.GUIControl;
using UnityEngine;

public class ScreenControllerScrollView : UIViewController
{
    public ScrollViewExampleDataSource ScrollRectExtensionDataSource;
    public ScrollViewExampleRectExtension ScrollRectExtension;

    public ScreenControllerScrollView()
    {
        IsFullScreen = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if(ScrollRectExtension == null)
        {
            throw new MissingComponentException("Missing UIScrollRectExtension component reference");
        }

        if(ScrollRectExtension.BasePrefabs.Length == 0)
        {
            throw new UnityException("Missing prefabs to use as cells for UIScrollRectExtension component");
        }

        if(ScrollRectExtensionDataSource != null)
        {
            ScrollRectExtensionDataSource.Init(ScrollRectExtension.BasePrefabs);
            ScrollRectExtension.Init(ScrollRectExtensionDataSource);
        }
    }
}
