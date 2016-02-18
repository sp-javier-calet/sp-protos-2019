using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
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

public class ResourcePool : IEnumerable<KeyValuePair<string,long>>, ICloneable
{

    /// <summary>
    /// Occurs when a resource ammount is modified.
    /// </summary>
    public event Action Modified;

    Dictionary<string,long> _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePool"/> class.
    /// </summary>
    public ResourcePool(ResourcePool other = null)
    {
        if(other == null)
        {
            _data = new Dictionary<string,long>();
        }
        else
        {
            _data = new Dictionary<string, long>(other._data);
            Modified = other.Modified;
        }
    }

    public object Clone()
    {
        return new ResourcePool(this);
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

    public bool IsEmpty
    {
        get
        {
            foreach(var kvp in _data)
            {
                if(kvp.Value != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public bool IsUnit
    {
        get
        {
            foreach(var kvp in _data)
            {
                if(kvp.Value != 1)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public KeyValuePair<string,long> MaxPair
    {
        get
        {
            var max = new KeyValuePair<string,long>(string.Empty, long.MinValue);
            foreach(var kvp in _data)
            {
                if(max.Value < kvp.Value)
                {
                    max = kvp;
                }
            }
            return max;
        }
    }

    public KeyValuePair<string,long> MinPair
    {
        get
        {
            var min = new KeyValuePair<string,long>(string.Empty, long.MaxValue);
            foreach(var kvp in _data)
            {
                if(min.Value > kvp.Value)
                {
                    min = kvp;
                }
            }
            return min;
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
            long value;
            if(_data.TryGetValue(resource, out value))
            {
                return value;
            }
            return 0;
        }

        set
        {
            if(value < 0)
            {
                throw new ResourceException("Value can't be negative");
            }
            long old;
            if(!_data.TryGetValue(resource, out old) || value != old)
            {
                _data[resource] = value;
                if(Modified != null)
                {
                    Modified();
                }
            }
        }
    }

    void Add(string key, long value)
    {
        if(!_data.ContainsKey(key))
        {
            _data[key] = value;
        }
        else
        {
            _data[key] += value;
        }
    }

    void Multiply(string key, long value)
    {
        if(_data.ContainsKey(key))
        {
            _data[key] *= value;
        }
    }

    public void Assign(ResourcePool b)
    {
        bool modified = false;
        foreach(var kvp in b)
        {
            if(kvp.Value != this[kvp.Key])
            {
                this[kvp.Key] = kvp.Value;
                modified = true;
            }
        }
        if(modified && Modified != null)
        {
            Modified();
        }
    }
        
    public void Assign(long b)
    {
        bool modified = false;
        var keys = new List<string>(_data.Keys);
        foreach(var key in keys)
        {
            if(b != _data[key])
            {
                _data[key] = b;
                modified = true;
            }
        }
        if(modified && Modified != null)
        {
            Modified();
        }
    }

    public void Min(ResourcePool b)
    {
        bool modified = false;
        foreach(var kvp in b)
        {
            if(kvp.Value < this[kvp.Key])
            {
                this[kvp.Key] = kvp.Value;
                modified = true;
            }
        }
        if(modified && Modified != null)
        {
            Modified();
        }
    }
        
    public void Min(long b)
    {
        bool modified = false;
        var keys = new List<string>(_data.Keys);
        foreach(var key in keys)
        {
            if(b < _data[key])
            {
                _data[key] = b;
                modified = true;
            }
        }
        if(modified && Modified != null)
        {
            Modified();
        }
    }

    public void Max(ResourcePool b)
    {
        bool modified = false;
        foreach(var kvp in b)
        {
            if(kvp.Value > this[kvp.Key])
            {
                this[kvp.Key] = kvp.Value;
                modified = true;
            }
        }
        if(modified && Modified != null)
        {
            Modified();
        }
    }
    
    public void Max(long b)
    {
        bool modified = false;
        var keys = new List<string>(_data.Keys);
        foreach(var key in keys)
        {
            if(b > _data[key])
            {
                _data[key] = b;
                modified = true;
            }
        }
        if(modified && Modified != null)
        {
            Modified();
        }
    }

    public void Add(ResourcePool b)
    {
        if(b != null && !b.IsEmpty)
        {
            foreach(var kvp in b)
            {
                Add(kvp.Key, kvp.Value);
            }
            if(Modified != null)
            {
                Modified();
            }
        }
    }
        
    public void Add(long b)
    {
        if(b != 0)
        {
            var keys = new List<string>(_data.Keys);
            foreach(var key in keys)
            {
                Add(key, b);
            }
            if(Modified != null)
            {
                Modified();
            }
        }
    }

    public void Substract(ResourcePool b)
    {
        if(b != null && !b.IsEmpty)
        {
            foreach(var kvp in b)
            {
                Add(kvp.Key, -kvp.Value);
            }
            if(Modified != null)
            {
                Modified();
            }
        }
    }
        
    public void Substract(long b)
    {
        if(b != 0)
        {
            var keys = new List<string>(_data.Keys);
            foreach(var key in keys)
            {
                Add(key, -b);
            }
            if(Modified != null)
            {
                Modified();
            }
        }
    }

    
    public bool CanSubstract(ResourcePool b)
    {
        if(b == null || b.IsEmpty)
        {
            return true;
        }
        foreach(var kvp in b)
        {
            if(this[kvp.Key] < kvp.Value)
            {
                return false;
            }
        }
        return true;
    }

    public bool CanSubstract(long b)
    {
        foreach(var kvp in _data)
        {
            if(kvp.Value < b)
            {
                return false;
            }
        }
        return true;
    }

    public void Multiply(ResourcePool b)
    {
        if(b != null && !b.IsUnit)
        {
            foreach(var kvp in b)
            {
                Multiply(kvp.Key, kvp.Value);
            }
            if(Modified != null)
            {
                Modified();
            }
        }
    }
        
    public void Multiply(long b)
    {
        if(b != 1)
        {
            var keys = new List<string>(_data.Keys);
            foreach(var key in keys)
            {
                Multiply(key, b);
            }
            if(Modified != null)
            {
                Modified();
            }
        }
    }

    public void Divide(ResourcePool b)
    {
        if(b != null && !b.IsUnit)
        {
            foreach(var kvp in b)
            {
                Multiply(kvp.Key, 1 / kvp.Value);
            }
            if(Modified != null)
            {
                Modified();
            }
        }
    }
    
    public void Divide(long b)
    {
        if(b != 1)
        {
            var keys = new List<string>(_data.Keys);
            foreach(var key in keys)
            {
                Multiply(key, 1 / b);
            }
            if(Modified != null)
            {
                Modified();
            }
        }
    }

    public static ResourcePool operator+(ResourcePool a, ResourcePool b)
    {
        var c = new ResourcePool(a);
        c.Add(b);
        return c;
    }

    public static ResourcePool operator+(ResourcePool a, long b)
    {
        var c = new ResourcePool(a);
        c.Add(b);
        return c;
    }

    public static ResourcePool operator-(ResourcePool a, ResourcePool b)
    {
        var c = new ResourcePool(a);
        c.Substract(b);
        return c;
    }

    
    public static ResourcePool operator-(ResourcePool a, long b)
    {
        var c = new ResourcePool(a);
        c.Substract(b);
        return c;
    }
    
    public static ResourcePool operator*(ResourcePool a, ResourcePool b)
    {
        var c = new ResourcePool(a);
        c.Multiply(b);
        return c;
    }
        
    public static ResourcePool operator*(ResourcePool a, long b)
    {
        var c = new ResourcePool(a);
        c.Multiply(b);
        return c;
    }

    public static ResourcePool operator/(ResourcePool a, ResourcePool b)
    {
        var c = new ResourcePool(a);
        c.Divide(b);
        return c;
    }
        
    public static ResourcePool operator/(ResourcePool a, long b)
    {
        var c = new ResourcePool(a);
        c.Divide(b);
        return c;
    }

    public static ResourcePool Min(ResourcePool a, ResourcePool b)
    {
        var c = new ResourcePool(a);
        c.Min(b);
        return c;
    }
    
    public static ResourcePool Min(ResourcePool a, long b)
    {
        var c = new ResourcePool(a);
        c.Min(b);
        return c;
    }
        
    public static ResourcePool Max(ResourcePool a, ResourcePool b)
    {
        var c = new ResourcePool(a);
        c.Max(b);
        return c;
    }

    public static ResourcePool Max(ResourcePool a, long b)
    {
        var c = new ResourcePool(a);
        c.Max(b);
        return c;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("[ResourcePool");
        if(_data.Count > 0)
        {
            builder.Append(": ");
        }
        int i = 0;
        foreach(var kvp in this)
        {
            builder.AppendFormat("{0}={1}", kvp.Key, kvp.Value);
            if(i < _data.Count - 1)
            {
                builder.Append(", ");
            }
            i++;
        }
        builder.Append("]");
        return builder.ToString();
    }
}
