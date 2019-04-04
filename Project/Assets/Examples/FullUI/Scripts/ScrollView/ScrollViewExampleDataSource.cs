//-----------------------------------------------------------------------
// ScrollViewExampleDataSource.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using SocialPoint.GUIControl;
using UnityEngine;

public class ScrollViewExampleDataSource : UIScrollRectBaseDataSource<ScrollViewExampleCellData>
{
    [SerializeField]
    int _numberOfCells = 50;

    List<ScrollViewExampleCellData> _data;
    GameObject[] _prefabs;

    public void Init(GameObject[] prefabs)
    { 
        _data = new List<ScrollViewExampleCellData>();

        _prefabs = prefabs;
    }        
        
    public override IEnumerator Load()
    {
        _data.Clear();
        for (int i = 0; i < _numberOfCells; ++i)
        {
            _data.Add(new ScrollViewExampleCellData("test item small name " + i, "test item small description for item with index " + i, GetPrefabIndexFromArray()));
        }

        Data = _data;
            
        // we wait 2 seconds only for testing loading spinner, this can be removed in normal cases
        yield return new WaitForSeconds(2.0f);
    }
        
    public override ScrollViewExampleCellData CreateCellData()
    {
        return new ScrollViewExampleCellData("test NEW item small name ", "test NEW item small description for item with index ", GetPrefabIndexFromArray());
    }
  
    int GetPrefabIndexFromArray()
    {
        return Random.Range(0, _prefabs.Length - 1);
    }
}