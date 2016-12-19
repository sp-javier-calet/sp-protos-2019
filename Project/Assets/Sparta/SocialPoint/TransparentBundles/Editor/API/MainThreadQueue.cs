using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[InitializeOnLoad]
public class MainThreadQueue{    
	
    static MainThreadQueue()
    {
        EditorApplication.update += WatcherUpdate;
    }

    public static Queue responseQueue = Queue.Synchronized(new Queue());

    public static void AddQueueItem(object obj)
    {
        responseQueue.Enqueue(obj);
    }

    static void WatcherUpdate () {
        if(responseQueue.Count > 0)
        {
            var requestFinished = (HttpAsyncRequest.RequestState)responseQueue.Dequeue();

            requestFinished.RaiseCallback();
        }
	}
}
