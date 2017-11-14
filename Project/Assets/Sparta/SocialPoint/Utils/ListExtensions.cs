using System.Collections.Generic;

public static class ListExtensions
{
    public static void NewElements<T>(this List<T> newItems, List<T> oldItems, List<T> result)
    {
        result.Clear();

        for(int i = 0; i < newItems.Count; ++i)
        {
            if(!oldItems.Contains(newItems[i]))
            {
                if(!result.Contains(newItems[i]))
                {
                    result.Add(newItems[i]);
                }
            }
        }
    }

    public static void RemovedElements<T>(this List<T> newItems, List<T> oldItems, List<T> result)
    {
        NewElements(oldItems, newItems, result);
    }

    public static void IntersectedElements<T>(this List<T> newItems, List<T> oldItems, List<T> result)
    {
        result.Clear();

        for(int i = 0; i < newItems.Count; ++i)
        {
            if(oldItems.Contains(newItems[i]))
            {
                if(!result.Contains(newItems[i]))
                {
                    result.Add(newItems[i]);
                }
            }
        }
    }

    public static T First<T>(this List<T> items, T defaultValue = default(T))
    {
        return items.Count > 0 ? items[0] : defaultValue;
    }

    public static List<T> CreateListWithSize<T>(int size)
    {
        var list = new List<T>(size);
        list.Resize(size);
        return list;
    }

    public static void Resize<T>(this List<T> list, int size, T item = default(T))
    {
        int currSize = list.Count;

        if(size < currSize)
        {
            list.RemoveRange(size, currSize - size);
        }

        else if(size > currSize)
        {
            if(size > list.Capacity)
            {
                //this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                list.Capacity = size;
            }
            var newItemsCount = size - currSize;
            for(int i = 0; i < newItemsCount; ++i)
            {
                list.Add(item);
            }
        }
    }
}
