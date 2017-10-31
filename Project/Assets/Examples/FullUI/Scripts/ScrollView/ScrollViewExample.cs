using SocialPoint.GUIControl;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewExample : UIScrollRectExtension<ScrollViewExampleCellData, ScrollViewExampleCellItem> 
{
    [SerializeField]
    int _numberOfCells = 50;

    List<ScrollViewExampleCellData> _myData = new List<ScrollViewExampleCellData>();

    public void Init()
    {
        if(_prefabs.Length == 0)
        {
            throw new UnityException("Missing prefabs to instantiate");
        }

        DefineGetData(GetData);
        DefineAddCellData(AddData);

        FetchData();
    }
      
    List<ScrollViewExampleCellData> GetData()
    {
        _myData.Clear();
        for (int i = 0; i < _numberOfCells; ++i)
        {
            _myData.Add(new ScrollViewExampleCellData("test item small name " + i, "test item small description for item with index " + i, GetPrefabIndexFromArray()));
        }

        return _myData;
    }

    ScrollViewExampleCellData AddData()
    {
        return new ScrollViewExampleCellData("test NEW item small name ", "test NEW item small description for item with index ", GetPrefabIndexFromArray());
    }
        
    int GetPrefabIndexFromArray()
    {
        return Random.Range(0, _prefabs.Length - 1);
    }
}
