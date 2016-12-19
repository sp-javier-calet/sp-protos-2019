using UnityEditor;
using System.Collections;
using System;

[InitializeOnLoad]
public class MainThreadQueue{
    // We need this to be singleton to guarantee initialization when accessed.
    private static Object lockObj = new object();
    private static MainThreadQueue instance;
    public static MainThreadQueue Instance
    {
        get
        {
            //Just to be thread safe
            lock(lockObj)
            {
                if(instance == null)
                {
                    instance = new MainThreadQueue();
                }
            }
            return instance;
        }
    }



    private MainThreadQueue()
    {
        EditorApplication.update += WatcherUpdate;
    }

    private Queue responseQueue = Queue.Synchronized(new Queue());

    public event Action<object> OnItemQueued;

    public void AddQueueItem(object obj)
    {
        responseQueue.Enqueue(obj);
    }

    void WatcherUpdate () {
        if(responseQueue.Count > 0)
        {
            if(OnItemQueued != null)
            {
                OnItemQueued(responseQueue.Dequeue());
            }
        }
	}
}
