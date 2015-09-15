using System;
using System.Collections.Generic;

using UnityEngine.Assertions;
using SocialPoint.Events;
using SocialPoint.Attributes;

public class ResourceManager
{
    List<Resource> _resources;

    public List<Resource> Resources
    {
        get
        {
            return _resources;
        }
        set
        {
            _resources = value;
        }
    }

    Dictionary<Resource,int> _resourcesAmounts;

    public delegate void ResourceModifiedDelegate(ResourceOperation op);
    public event ResourceModifiedDelegate ResourceModified = delegate {};

    public ResourceManager()
    {
        _resources = new List<Resource>();
        _resourcesAmounts = new Dictionary<Resource, int>();
    }

    public void RegisterResource(Resource resource)
    {
        RegisterResource(resource, 0);
    }

    public void RegisterResource(Resource resource, int ammount)
    {
        Assert.IsTrue(!_resources.Contains(resource), "This resource has been already registered");
        Assert.IsTrue(ammount >= 0);
        _resources.Add(resource);
        _resourcesAmounts.Add(resource, ammount);
    }

    public Resource GetResourceByID(string ID)
    {
        Resource resource = _resources.Find(r => r.ID == ID);
        Assert.IsTrue(!string.IsNullOrEmpty(resource.ID), "there's no resource with that ID");
        return resource;
    }

    public int GetResourceAmmount(Resource resource)
    {
        return _resourcesAmounts[resource];
    }

    public int GetResourceAmmount(String ID)
    {
        var resource = GetResourceByID(ID);
        return GetResourceAmmount(resource);
    }

    public void AddResource(string ID, int ammount)
    {
        var resource = GetResourceByID(ID);
        AddResource(resource, ammount);
    }

    public void AddResource(Resource resource, int ammount)
    {
        Assert.IsTrue(ammount >= 0, "ammount can't be negative");
        ModifyResourceAmmount(resource, ammount);
    }

    public void SubstractResource(string ID, int ammount)
    {
        var resource = GetResourceByID(ID);
        SubstractResource(resource, ammount);
    }

    public void SubstractResource(Resource resource, int ammount)
    {
        Assert.IsTrue(ammount >= 0,  "ammount can't be negative");
        ModifyResourceAmmount(resource, -ammount);
    }

    void ModifyResourceAmmount(Resource resource, int ammount)
    {
        var current = _resourcesAmounts[resource];
        current += ammount;
        Assert.IsTrue(current >= 0, "remaining resources can't be negative");
        _resourcesAmounts[resource] = current;
        //TODO: trackResource 
        var op = new ResourceOperation();
        ResourceModified(op);
    }

    public Attr ToAttr()
    {
        var data = new AttrDic();
        foreach(var kvp in _resourcesAmounts)
        {
            data.Set(kvp.Key.ID, new AttrInt(kvp.Value));
        }
        return data;
    }
}
