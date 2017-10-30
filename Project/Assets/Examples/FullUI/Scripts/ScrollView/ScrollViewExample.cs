using SocialPoint.GUIControl;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewExample : UIScrollRectExtension<MyData, MyCell> 
{
    [SerializeField]
    int _numberOfCells = 50;

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
      
    List<MyData> GetData()
    {
        var myData = new List<MyData>();
        for (int i = 0; i < _numberOfCells; ++i)
        {
            myData.Add(new MyData("test item small name " + i, "test item small description for item with index " + i, GetPrefabIndexFromArray()));
        }

        return myData;
    }

    MyData AddData()
    {
        return new MyData("test NEW item small name ", "test NEW item small description for item with index ", GetPrefabIndexFromArray());
    }
        
    int GetPrefabIndexFromArray()
    {
        return Random.Range(0, _prefabs.Length - 1);
    }
}
