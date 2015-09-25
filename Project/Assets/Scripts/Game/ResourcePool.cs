using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Events;
using SocialPoint.Attributes;

public class ResourceException :Exception
{
    public ResourceException()
    {
    }

    public ResourceException(string message) : base(message)
    {
    }

    public ResourceException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class ResourcePool : IEnumerable<KeyValuePair<string,long>>
{
    public delegate void ResourceModifiedDelegate(ResourceOperation op);

    /// <summary>
    /// Occurs when a resource ammount is modified.
    /// </summary>
    public event ResourceModifiedDelegate ResourceModified = delegate {};


    Dictionary<string,long> _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePool"/> class.
    /// </summary>
    public ResourcePool()
    {
        _data = new Dictionary<string,long>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePool"/> class.
    ///  {
    ///     "food": 1000,
    ///     "gold": 20000
    ///  }
    /// </summary>
    public ResourcePool(AttrDic data) : this()
    {
        Init(data);
    }

    public IEnumerator<KeyValuePair<string, long>> GetEnumerator()
    {
        return _data.GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool ContainsKey(string name)
    {
        return _data.ContainsKey(name);
    }

    /// <summary>
    /// Init the resources ammount dictionary.
    /// </summary>
    /// <param name="data">Data.</param>
    void Init(AttrDic data)
    {
        foreach(var kvp in data)
        {
            if(!ContainsKey(kvp.Key))
            {
                this[kvp.Key] = kvp.Value.AsValue.ToInt();
            }
        }
    }

    /// <summary>
    /// Gets or sets the amount of the specified resource.
    /// </summary>
    /// <param name="resource">Resource.</param>
    public long this[string resource]
    {
        get
        {
            if(!ContainsKey(resource))
            {
                _data.Add(resource, 0);
            }
            return _data[resource];
        }

        set
        {
            if(value < 0)
            {
                throw new ResourceException("Value can't be negative");
            }
            _data[resource] = value;
            //TODO: fill with more data
            var op = new ResourceOperation();
            op.Resource = resource;
            op.Amount = (int)this[resource];
            ResourceModified(op);
        }
    }

    public static ResourcePool operator+(ResourcePool a, ResourcePool b)
    {
        foreach(var kvp in b)
        {
            a[kvp.Key] += kvp.Value;
        }
        return a;
    }

    public static ResourcePool operator-(ResourcePool a, ResourcePool b)
    {
        foreach(var kvp in b)
        {
            a[kvp.Key] -= kvp.Value;
        }
        return a;
    }

    /// <summary>
    /// Returns the dictionary of resources amounts.
    /// </summary>
    /// <returns>AttrDic<string,int></returns>
    public Attr ToAttr()
    {
        var data = new AttrDic();
        foreach(var kvp in this)
        {
            data.Set(kvp.Key, new AttrLong(kvp.Value));
        }
        return data;
    }
}
