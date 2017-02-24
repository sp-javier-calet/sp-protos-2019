using System;
using System.Collections;
using UnityEditor;

namespace SocialPoint.TransparentBundles
{
    public class MainThreadQueue
    {
        // We need this to be singleton to guarantee initialization when accessed.
        static Object _lockObj = new object();
        static MainThreadQueue _instance;

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

        MainThreadQueue()
        {
            EditorApplication.update += WatcherUpdate;
        }

        /// <summary>
        /// Queue that synchronizes objects from other threads to the main thread.
        /// </summary>
        readonly Queue _syncObjectQueue = Queue.Synchronized(new Queue());

        /// <summary>
        /// You can attach to this event to have your class handle the synced items.
        /// Be careful and check that the object belongs to your pipeline before using it since other processes may use this queue.
        /// You should call once to MainThreadQueue.Instance to be sure it is initialized. [InitializeOnLoad] for your class is recommended. 
        /// </summary>
        public event Action<object> OnItemDequeued;

        /// <summary>
        /// Adds an item to the main thread queue
        /// </summary>
        /// <param name="obj">object to add to the queue</param>
        public void AddQueueItem(object obj)
        {
            _syncObjectQueue.Enqueue(obj);
        }

        /// <summary>
        /// Update attached to UnityEditor Update that checks the Queue for items and process them.
        /// </summary>
        void WatcherUpdate()
        {
            if(_syncObjectQueue.Count > 0)
            {
                if(OnItemDequeued != null)
                {
                    OnItemDequeued(_syncObjectQueue.Dequeue());
                }
            }
        }
    }
}
