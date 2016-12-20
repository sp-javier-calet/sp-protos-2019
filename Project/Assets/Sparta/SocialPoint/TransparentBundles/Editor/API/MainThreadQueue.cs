using UnityEditor;
using System.Collections;
using System;

[InitializeOnLoad]
public class MainThreadQueue
{
    // We need this to be singleton to guarantee initialization when accessed.
    private static Object _lockObj = new object();
    private static MainThreadQueue _instance;
    public static MainThreadQueue Instance
    {
        get
        {
            //Just to be thread safe
            lock(_lockObj)
            {
                if(_instance == null)
                {
                    _instance = new MainThreadQueue();
                }
            }
            return _instance;
        }
    }

    private MainThreadQueue()
    {
        EditorApplication.update += WatcherUpdate;
    }

    private Queue _responseQueue = Queue.Synchronized(new Queue());

    public event Action<object> OnItemQueued;

    public void AddQueueItem(object obj)
    {
        _responseQueue.Enqueue(obj);
    }

    void WatcherUpdate()
    {
        if(_responseQueue.Count > 0)
        {
            if(OnItemQueued != null)
            {
                OnItemQueued(_responseQueue.Dequeue());
            }
        }
    }
}
