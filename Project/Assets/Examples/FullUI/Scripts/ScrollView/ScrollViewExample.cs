using SocialPoint.GUIControl;
using System.Collections.Generic;
using UnityEngine;

public class ScrollViewExample : UIScrollRectExtension<MyData, MyCell> 
{
    public void Init()
    {
        FetchData(GetData);
    }

//    public void GetData()
//    {
//        StartCoroutine(GetData);//FetchDataFromServer(OnReceivedData)); 
//    }

//    IEnumerator FetchDataFromServer(Action callback)
//    {
//        // Simulating server delay
//        yield return new WaitForSeconds(2f);
//
//        if(callback != null)
//        {
//            callback();
//        }
//    }
//
    List<MyData> GetData()
    {
        List<MyData> myData = new List<MyData>();
        string[] prefabs = {"GUI_StoreItem", "GUI_StoreItemSmall", "GUI_StoreItem2"};

        for (int i = 0; i < 10; ++i)
        {
            myData.Add(new MyData(i, "test item small name " + i, "test item small description for item with index " + i, prefabs[UnityEngine.Random.Range(0,3)]));
        }

        return myData;
    }
}
