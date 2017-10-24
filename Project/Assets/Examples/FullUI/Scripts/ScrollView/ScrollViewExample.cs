using SocialPoint.GUIControl;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewExample : UIScrollRectExtension<MyData, MyCell> 
{
    string[] prefabs = {"GUI_StoreItem", "GUI_StoreItemSmall", "GUI_StoreItem2"};

    public void Init()
    {
        DefineGetData(GetData);
        DefineAddCellData(AddData);

        FetchData();
    }
      
    List<MyData> GetData()
    {
        List<MyData> myData = new List<MyData>();
        for (int i = 0; i < 1; ++i)
        {
            myData.Add(new MyData("test item small name " + i, "test item small description for item with index " + i, prefabs[UnityEngine.Random.Range(0,3)]));
        }

        return myData;
    }

    MyData AddData()
    {
        return new MyData("test NEW item small name ", "test NEW item small description for item with index ", prefabs[UnityEngine.Random.Range(0, 3)]);
    }
}
