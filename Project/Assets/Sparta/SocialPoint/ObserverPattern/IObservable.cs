using System.Collections.Generic;

namespace ObserverPattern
{
    public interface IObservable
    {
        void NotifyToObservers();
            
        void AddObserver(IObserver observer);
            
        void RemoveObserver(IObserver observer);
    }
}