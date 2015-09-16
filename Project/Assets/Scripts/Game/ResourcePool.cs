using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Assertions;
using SocialPoint.Events;
using SocialPoint.Attributes;

public class ResourcePool : Dictionary<string,long>
{

    public delegate void ResourceModifiedDelegate(ResourceOperation op);

    /// <summary>
    /// Occurs when a resource ammount is modified.
    /// </summary>
    public event ResourceModifiedDelegate ResourceModified = delegate {};

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePool"/> class.
    /// </summary>
    public ResourcePool():base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePool"/> class.
    ///  {
    ///     "food": 1000,
    ///     "gold": 20000
    ///  }
    /// </summary>
    public ResourcePool(AttrDic data):this()
    {
        Init(data);
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
                Add(kvp.Key, kvp.Value.AsValue.ToInt());
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
                Add(resource, 0);
            }
            return base[resource];
        }

        set
        {
            Assert.IsTrue(value >= 0, "value can't be negative");
            base[resource] = value;
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
            if(!a.ContainsKey(kvp.Key))
            {
                a.Add(kvp.Key, kvp.Value);
            }
            else
            {
                a[kvp.Key] += kvp.Value;
            }
        }
        return a;
    }

    public static ResourcePool operator-(ResourcePool a, ResourcePool b)
    {
        foreach(var kvp in b)
        {
            if(!a.ContainsKey(kvp.Key))
            {
                a.Add(kvp.Key, kvp.Value);
            }
            else
            {
                a[kvp.Key] -= kvp.Value;
            }
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
