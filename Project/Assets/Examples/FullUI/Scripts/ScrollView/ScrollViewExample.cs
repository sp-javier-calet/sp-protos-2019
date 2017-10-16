using SocialPoint.GUIControl;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

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

        int totalNumber = 10;
        for (int i = 0; i < totalNumber; ++i)
        {
            string prefabName = "GUI_StoreItem";

            if(!_prefabs.ContainsKey(prefabName))
            {
                var prefab = Resources.Load(prefabName);
                if(prefab != null)
                {
                    var go = GameObject.Instantiate(prefab) as GameObject;
                    if(go != null)
                    {
                        _prefabs.Add(prefabName, go);
                    }
                }
            }

            myData.Add(new MyData(i, "test item name " + i, "test item description for item with index " + i, prefabName));
        }

        totalNumber = 2;
        for (int i = 0; i < totalNumber; ++i)
        {
            string prefabName = "GUI_StoreItemSmall";

            if(!_prefabs.ContainsKey(prefabName))
            {
                var prefab = Resources.Load(prefabName);
                if(prefab != null)
                {
                    var go = GameObject.Instantiate(prefab) as GameObject;
                    if(go != null)
                    {
                        _prefabs.Add(prefabName, go);
                    }
                }
            }

            myData.Add(new MyData(i, "test item small name " + i, "test item small description for item with index " + i, prefabName));
        }

        return myData;
    }
}
