using System.Collections.Generic;

namespace ObserverPattern
{
    public class Observable : IObservable
    {
        IList<IObserver> _observers = new List<IObserver>();

        public void NotifyToObservers()
        {
            for (int i = 0; i < _observers.Count; ++i)
            {
                _observers[i].OnNotify();
            }
        }
            
        public void AddObserver(IObserver observer)
        {
            _observers.Add(observer);
        }
            
        public void RemoveObserver(IObserver observer)
        {
            if(_observers.Contains(observer))
            {
                _observers.Remove(observer);
            }
        }
    }
}