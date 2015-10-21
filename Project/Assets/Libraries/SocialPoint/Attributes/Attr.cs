using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SocialPoint.Base;

namespace SocialPoint.Attributes
{
    public enum AttrType
    {
        UNKNOWN,
        EMPTY,
        VALUE,
        DICTIONARY,
        LIST
    }

    public enum AttrValueType
    {
        UNKNOWN,
        EMPTY,
        STRING,
        BOOL,
        INT,
        LONG,
        FLOAT,
        DOUBLE
    }

    public class Attr : IDisposable
    {
        public AttrType AttrType { get; private set; }

        public Attr(AttrType type = AttrType.UNKNOWN)
        {
            AttrType = type;
        }

        public static Attr Copy(Attr attr)
        {
            switch(attr.AttrType)
            {
            case AttrType.EMPTY:
                return new AttrEmpty();
            case AttrType.DICTIONARY:
                return new AttrDic((AttrDic)attr);
            case AttrType.LIST:
                return new AttrList((AttrList)attr);
            case AttrType.VALUE:
                return AttrValue.Copy((AttrValue)attr);
            }

            return new Attr(attr.AttrType);
        }

        public static bool operator ==(Attr la, Attr ra)
        {
            if(System.Object.ReferenceEquals(la, ra))
            {
                return true;
            }
            if(((object)la == null) || ((object)ra == null))
            {
                return false;
            }

            if(la.AttrType != ra.AttrType)
            {
                return false;
            }

            switch(la.AttrType)
            {
            case AttrType.EMPTY:
                return (AttrEmpty)la == (AttrEmpty)ra;
            case AttrType.DICTIONARY:
                return (AttrDic)la == (AttrDic)ra;
            case AttrType.LIST:
                return (AttrList)la == (AttrList)ra;
            case AttrType.VALUE:
                return (AttrValue)la == (AttrValue)ra;
            }
            
            return false;
        }
        
        public static bool operator !=(Attr la, Attr ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as Attr;
            if((System.Object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return (int)AttrType;
        }

        public override string ToString()
        {
            return string.Empty;
        }

        public virtual AttrValue AsValue
        {
            get
            {
                return AttrValue.Invalid;
            }
        }

        public virtual AttrDic AsDic
        {
            get
            {
                return AttrDic.Invalid;
            }
        }

        public virtual AttrList AsList
        {
            get
            {
                return AttrList.Invalid;
            }
        }

        public bool IsValue
        {
            get
            {
                return AttrType == AttrType.VALUE;
            }
        }
        
        public bool IsDic
        {
            get
            {
                return AttrType == AttrType.DICTIONARY;
            }
        }
        
        public bool IsList
        {
            get
            {
                return AttrType == AttrType.LIST;
            }
        }

        public AttrValue AssertValue
        {
            get
            {
                DebugUtils.Assert(IsValue);
                return AsValue;
            }
        }
        
        public AttrDic AssertDic
        {
            get
            {
                DebugUtils.Assert(IsDic);
                return AsDic;
            }
        }
        
        public AttrList AssertList
        {
            get
            {
                DebugUtils.Assert(IsList);
                return AsList;
            }
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class AttrEmpty : Attr
    {

        public AttrEmpty() : base(AttrType.EMPTY)
        {
        }

        public static bool operator ==(AttrEmpty la, AttrEmpty ra)
        {
            return true;
        }
        
        public static bool operator !=(AttrEmpty la, AttrEmpty ra)
        {
            return false;
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrEmpty;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        const string NullString = "NULL";

        public override string ToString()
        {
            return NullString;
        }
    }

    public class AttrValue : Attr
    {
        public AttrValueType AttrValueType { get; private set; }

        public AttrValue(AttrValueType type) : base(AttrType.VALUE)
        {
            AttrValueType = type;
        }

        public AttrValue(AttrValue attr) : this(attr.AttrValueType)
        {
        }

        public static AttrValue Invalid
        {
            get
            {
                return new AttrValue(AttrValueType.UNKNOWN);
            }
        }

        public virtual float ToFloat()
        {
            return ToInt();
        }

        public virtual int ToInt()
        {
            return 0;
        }

        public virtual bool ToBool()
        {
            return ToInt() != 0;
        }

        public virtual long ToLong()
        {
            return ToInt();
        }

        public virtual double ToDouble()
        {
            return ToFloat();
        }

        public virtual void SetFloat(float val)
        {
            SetInt((int)val);
        }
        
        public virtual void SetInt(int val)
        {
        }
        
        public virtual void SetBool(bool val)
        {
            SetInt(val ? 1 : 0);
        }
        
        public virtual void SetLong(long val)
        {
            SetInt((int)val);
        }
        
        public virtual void SetDouble(double val)
        {
            SetFloat((float)val);
        }

        public virtual void SetString(string val)
        {
        }

        public static AttrValue Copy(AttrValue attr)
        {
            switch(attr.AttrValueType)
            {
            case AttrValueType.BOOL:
                return new AttrBool((AttrBool)attr);
            case AttrValueType.INT:
                return new AttrInt((AttrInt)attr);
            case AttrValueType.LONG:
                return new AttrLong((AttrLong)attr);
            case AttrValueType.FLOAT:
                return new AttrFloat((AttrFloat)attr);
            case AttrValueType.DOUBLE:
                return new AttrDouble((AttrDouble)attr);
            case AttrValueType.STRING:
                return new AttrString((AttrString)attr);
            }
            
            return new AttrValue(attr.AttrValueType);
        }

        public static bool operator ==(AttrValue la, bool ra)
        {
            return la.ToBool() == ra;
        }

        public static bool operator ==(AttrValue la, int ra)
        {
            return la.ToInt() == ra;
        }

        public static bool operator ==(AttrValue la, long ra)
        {
            return la.ToLong() == ra;
        }

        public static bool operator ==(AttrValue la, float ra)
        {
            return la.ToFloat() == ra;
        }

        public static bool operator ==(AttrValue la, double ra)
        {
            return la.ToDouble() == ra;
        }

        public static bool operator ==(AttrValue la, string ra)
        {
            return la.ToString() == ra;
        }

        public static bool operator !=(AttrValue la, bool ra)
        {
            return !(la == ra);
        }
        
        public static bool operator !=(AttrValue la, int ra)
        {
            return !(la == ra);
        }
        
        public static bool operator !=(AttrValue la, long ra)
        {
            return !(la == ra);
        }
        
        public static bool operator !=(AttrValue la, float ra)
        {
            return !(la == ra);
        }
        
        public static bool operator !=(AttrValue la, double ra)
        {
            return !(la == ra);
        }
        
        public static bool operator !=(AttrValue la, string ra)
        {
            return !(la == ra);
        }

        public static bool operator ==(AttrValue la, AttrValue ra)
        {
            if(System.Object.ReferenceEquals(la, ra))
            {
                return true;
            }
            if(((object)la == null) || ((object)ra == null))
            {
                return false;
            }

            if(la.AttrValueType != ra.AttrValueType)
            {
                return false;
            }
            
            switch(la.AttrValueType)
            {
            case AttrValueType.BOOL:
                return (AttrBool)la == (AttrBool)ra;
            case AttrValueType.INT:
                return (AttrInt)la == (AttrInt)ra;
            case AttrValueType.LONG:
                return (AttrLong)la == (AttrLong)ra;
            case AttrValueType.FLOAT:
                return (AttrFloat)la == (AttrFloat)ra;
            case AttrValueType.DOUBLE:
                return (AttrDouble)la == (AttrDouble)ra;
            case AttrValueType.STRING:
                return (AttrString)la == (AttrString)ra;
            }
            
            return false;
        }
        
        public static bool operator !=(AttrValue la, AttrValue ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrValue;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }
        
        public override int GetHashCode()
        {
            return (int)AttrValueType ^ ToInt();
        }

        public override AttrValue AsValue
        {
            get
            {
                return this;
            }
        }

        protected virtual object GetValue()
        {
            return null;
        }

        public T ToValue<T>()
        {
            if(typeof(T) == typeof(int))
            {
                return (T)Convert.ChangeType(ToInt(), typeof(T), CultureInfo.CurrentCulture);
            }
            else if(typeof(T) == typeof(bool))
            {
                return (T)Convert.ChangeType(ToBool(), typeof(T), CultureInfo.CurrentCulture);
            }
            else if(typeof(T) == typeof(float))
            {
                return (T)Convert.ChangeType(ToFloat(), typeof(T), CultureInfo.CurrentCulture);
            }
            else if(typeof(T) == typeof(double))
            {
                return (T)Convert.ChangeType(ToDouble(), typeof(T), CultureInfo.CurrentCulture);
            }
            else if(typeof(T) == typeof(long))
            {
                return (T)Convert.ChangeType(ToLong(), typeof(T), CultureInfo.CurrentCulture);
            }
            else if(typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(ToString(), typeof(T), CultureInfo.CurrentCulture);
            }
            return (T)Convert.ChangeType(GetValue(), typeof(T), CultureInfo.CurrentCulture);
        }
    }

    public class AttrBool : AttrValue
    {
        protected bool value;

        public AttrBool(bool v) : base(AttrValueType.BOOL)
        {
            value = v;
        }

        public AttrBool(AttrBool attr) : this(attr.ToBool())
        {
        }

        public override bool ToBool()
        {
            return value;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override float ToFloat()
        {
            return value ? 1.0f : 0.0f;
        }
    
        public override int ToInt()
        {
            return value ? 1 : 0;
        }

        public override long ToLong()
        {
            return value ? 1L : 0L;
        }

        public override double ToDouble()
        {
            return value ? 1.0 : 0.0;
        }

        public override void SetFloat(float val)
        {
            value = val != 0.0f;
        }
        
        public override void SetInt(int val)
        {
            value = val == 0 ? false : true;
        }
        
        public override void SetBool(bool val)
        {
            value = val;
        }

        public override void SetString(string val)
        {
            string testVal = val.Trim();
            if(testVal == "true")
            {
                value = true;
            }
            else if(testVal == "false")
            {
                value = false;
            }
            else if(testVal == "0" || testVal.Length == 0)
            {
                value = false;
            }
            else
            {
                value = true;
            }
        }

        public static bool operator ==(AttrBool la, AttrBool ra)
        {
            return la.value == ra.value;
        }

        public static bool operator !=(AttrBool la, AttrBool ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrBool;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }

        public override int GetHashCode()
        {
            return (int)AttrType ^ value.GetHashCode();
        }

        protected override object GetValue()
        {
            return value;
        }

    }
  
    public class AttrInt : AttrValue
    {
        protected int value;

        public AttrInt(int v) : base(AttrValueType.INT)
        {
            value = v;
        }

        public AttrInt(AttrInt attr) : this(attr.ToInt())
        {
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override int ToInt()
        {
            return value;
        }
    
        public override void SetInt(int val)
        {
            value = val;
        }
        
        public override void SetString(string val)
        {
            int.TryParse(val, out value);
        }

        public static bool operator ==(AttrInt la, AttrInt ra)
        {
            return la.value == ra.value;
        }
        
        public static bool operator !=(AttrInt la, AttrInt ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrInt;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }
        
        public override int GetHashCode()
        {
            return (int)AttrType ^ value.GetHashCode();
        }

        protected override object GetValue()
        {
            return value;
        }
    }
  
    public class AttrLong : AttrValue
    {
        protected long value;

        public AttrLong(long v) : base(AttrValueType.LONG)
        {
            value = v;
        }

        public AttrLong(AttrLong attr) : this(attr.ToLong ())
        {
        }
   
        public override string ToString()
        {
            return value.ToString();
        }

        public override float ToFloat()
        {
            return (float)value;
        }
    
        public override int ToInt()
        {
            return (int)value;
        }
    
        public override long ToLong()
        {
            return value;
        }
    
        public override double ToDouble()
        {
            return (double)value;
        }

        public override void SetFloat(float val)
        {
            value = (long)val;
        }
        
        public override void SetInt(int val)
        {
            value = (long)val;
        }
        
        public override void SetLong(long val)
        {
            value = val;
        }
        
        public override void SetDouble(double val)
        {
            value = (long)val;
        }
        
        public override void SetString(string val)
        {
            long.TryParse(val, out value);
        }

        public static bool operator ==(AttrLong la, AttrLong ra)
        {
            return la.value == ra.value;
        }
        
        public static bool operator !=(AttrLong la, AttrLong ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrLong;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }
        
        public override int GetHashCode()
        {
            return (int)AttrType ^ value.GetHashCode();
        }

        protected override object GetValue()
        {
            return value;
        }
    }
  
    public class AttrString : AttrValue
    {
        protected string value;

        public AttrString(string v = null) : base(AttrValueType.STRING)
        {
            if(v == null)
            {
                v = string.Empty;
            }
            value = v;
        }

        public AttrString(AttrString attr) : this(attr.ToString ())
        {
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override float ToFloat()
        {
            float r = 0.0f;
            if(float.TryParse(value, out r))
            {
                return r;
            }
            return 0.0f;
        }
    
        public override int ToInt()
        {
            int r = 0;
            if(int.TryParse(value, out r))
            {
                return r;
            }
            return 0;
        }
    
        public override long ToLong()
        {
            long r = 0L;
            if(long.TryParse(value, out r))
            {
                return r;
            }
            return 0L;
        }
    
        public override double ToDouble()
        {
            double r = 0.0;
            if(double.TryParse(value, out r))
            {
                return r;
            }
            return 0.0;
        }

        public override bool ToBool()
        {
            bool r = false;
            if(bool.TryParse(value, out r))
            {
                return r;
            }
            return false;
        }

        public override void SetFloat(float val)
        {
            value = val.ToString();
        }
        
        public override void SetInt(int val)
        {
            value = val.ToString();
        }
        
        public override void SetBool(bool val)
        {
            value = val.ToString();
        }
        
        public override void SetLong(long val)
        {
            value = val.ToString();
        }
        
        public override void SetDouble(double val)
        {
            value = val.ToString();
        }
        
        public override void SetString(string val)
        {
            value = val.ToString();
        }

        public static bool operator ==(AttrString la, AttrString ra)
        {
            return la.value == ra.value;
        }
        
        public static bool operator !=(AttrString la, AttrString ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrString;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }
        
        public override int GetHashCode()
        {
            return (int)AttrType ^ value.GetHashCode();
        }

        protected override object GetValue()
        {
            return value;
        }
    }
  
    public class AttrFloat : AttrValue
    {
        protected float value;

        public AttrFloat(float v) : base(AttrValueType.FLOAT)
        {
            value = v;
        }

        public AttrFloat(AttrFloat attr) : this(attr.ToFloat ())
        {
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override float ToFloat()
        {
            return value;
        }
    
        public override int ToInt()
        {
            return (int)value;
        }
    
        public override void SetString(string val)
        {
            float.TryParse(val, out value);
        }
    
        public override void SetInt(int val)
        {
            value = (float)val;
        }

        public override void SetFloat(float val)
        {
            value = val;
        }

        public static bool operator ==(AttrFloat la, AttrFloat ra)
        {
            return la.value == ra.value;
        }
        
        public static bool operator !=(AttrFloat la, AttrFloat ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrFloat;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }
        
        public override int GetHashCode()
        {
            return (int)AttrType ^ value.GetHashCode();
        }

        protected override object GetValue()
        {
            return value;
        }
    }

    public class AttrDouble : AttrValue
    {
        protected double value;

        public AttrDouble(double v) : base(AttrValueType.DOUBLE)
        {
            value = v;
        }

        public AttrDouble(AttrDouble attr) : this(attr.ToDouble ())
        {
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override float ToFloat()
        {
            return (float)value;
        }
    
        public override int ToInt()
        {
            return (int)value;
        }
    
        public override long ToLong()
        {
            return (long)value;
        }
    
        public override double ToDouble()
        {
            return value;
        }

        public override void SetFloat(float val)
        {
            value = (double)val;
        }
        
        public override void SetInt(int val)
        {
            value = (double)val;
        }

        public override void SetLong(long val)
        {
            value = (double)val;
        }
        
        public override void SetDouble(double val)
        {
            value = val;
        }
        
        public override void SetString(string val)
        {
            double.TryParse(val, out value);
        }

        public static bool operator ==(AttrDouble la, AttrDouble ra)
        {
            return la.value == ra.value;
        }
        
        public static bool operator !=(AttrDouble la, AttrDouble ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrDouble;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }

        public override int GetHashCode()
        {
            return (int)AttrType ^ value.GetHashCode();
        }

        protected override object GetValue()
        {
            return value;
        }
    }
 
    public class AttrDic : Attr, IEnumerable<KeyValuePair<string, Attr>>
    {
        public Dictionary<string, Attr> Dic = new Dictionary<string, Attr>();

        public int Count
        {
            get{ return Dic.Count; }
        }

        public AttrDic(Dictionary<string, Attr> val = null) : base(AttrType.DICTIONARY)
        {
            if(val != null)
            {
                foreach(var pair in val)
                {
                    Set(pair.Key, Attr.Copy(pair.Value));
                }
            }
        }

        public AttrDic(AttrDic other) : this(other.Dic)
        {
        }

        public AttrDic(Dictionary<string, string> val) : this()
        {
            if(val != null)
            {
                foreach(var pair in this)
                {
                    Set(pair.Key, Attr.Copy(pair.Value));
                }
            }
        }
        
        public AttrDic(AttrList other) : this()
        {
            if(other != null)
            {
                var i = 0;
                foreach(var val in other)
                {
                    Set(i.ToString(), Attr.Copy(val));
                    i++;
                }
            }
        }

        public static AttrDic Invalid
        {
            get
            {
                return new AttrDic();
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return Dic.Keys;
            }
        }

        public bool ContainsValue(Attr value)
        {
            return Dic.ContainsValue(value);
        }

        public bool ContainsValue(string value)
        {
            return ContainsValue(new AttrString((value != null ? value : "")));
        }

        public IEnumerator<KeyValuePair<string, Attr>> GetEnumerator()
        {
            return Dic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public KeyValuePair<string, Attr> ElementAt(int index)
        {
            return Dic.ElementAt(index);
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append("{");
            var i = 0;
            foreach(var pair in this)
            {
                b.Append(pair.Key);
                b.Append(" = ");
                b.Append(pair.Value);
                if(i < Count - 1)
                {
                    b.Append(", ");
                }
                i++;
            }
            b.Append("}");
            return b.ToString();
        }

        public void Clear()
        {
            Dic.Clear();
        }

        public override AttrDic AsDic
        {
            get
            {
                return this;
            }
        }

        public override AttrList AsList
        {
            get
            {
                return new AttrList(this);
            }
        }

        public bool Set(string key, Attr attr)
        {
            if(!ContainsKey(key))
            {
                Dic.Add(key, attr);
            }
            else
            {
                Dic[key] = attr;
            }
            return true;
        }

        public bool SetValue(string key, bool val)
        {
            return Set(key, new AttrBool(val));
        }

        public bool SetValue(string key, int val)
        {
            return Set(key, new AttrInt(val));
        }

        public bool SetValue(string key, short val)
        {
            return Set(key, new AttrInt(val));
        }

        public bool SetValue(string key, float val)
        {
            return Set(key, new AttrFloat(val));
        }

        public bool SetValue(string key, double val)
        {
            return Set(key, new AttrDouble(val));
        }

        public bool SetValue(string key, long val)
        {
            return Set(key, new AttrLong(val));
        }

        public bool SetValue(string key, string val)
        {
            return Set(key, new AttrString(val));
        }

        public bool ContainsKey(string key)
        {
            return Dic.ContainsKey(key);
        }

        public Attr this[string key]
        {
            get
            {
                return Get(key);
            }

            set
            {
                Set(key, value);
            }
        }

        public Attr Get(string key)
        {
            if(!ContainsKey(key))
            {
                return new Attr();
            }
            return Dic[key];
        }

        public AttrValue GetValue(string key)
        {
            return Get(key).AsValue;
        }

        public bool Remove(string key)
        {
            return Dic.Remove(key);
        }

        public static bool operator ==(AttrDic la, AttrDic ra)
        {
            if(System.Object.ReferenceEquals(la, ra))
            {
                return true;
            }
            if(((object)la == null) || ((object)ra == null))
            {
                return false;
            }
            
            if(la.Count != ra.Count)
            {
                return false;
            }

            foreach(var pair in la)
            {
                if(!ra.ContainsKey(pair.Key) || !pair.Value.Equals(ra.Get(pair.Key)))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        public static bool operator !=(AttrDic la, AttrDic ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrDic;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }

        public bool Equals(AttrDic dic)
        {
            if((object)dic == null)
            {
                return false;
            }
                        
            return this == dic;
        }
        
        public override int GetHashCode()
        {
            return Dic.GetHashCode();
        }

        public Dictionary<string,V> ToDictionary<V>()
        {
            var dic = new Dictionary<string, V>();
            foreach(var pair in this)
            {
                dic.Add(pair.Key, pair.Value.AsValue.ToValue<V>());
            }

            return dic;
        }

        public override void Dispose()
        {
            foreach(var pair in this)
            {
                if(pair.Value != null)
                {
                    pair.Value.Dispose();
                }
            }
            GC.SuppressFinalize(this);
        }
    }
  
    public class AttrList : Attr, IEnumerable<Attr>
    {
        public bool AllowDuplicates { get; set; }

        public List<Attr> List = new List<Attr>();

        public int Count
        {
            get
            {
                return List.Count;
            }
        }

        public AttrList(List<Attr> val = null) : base(AttrType.LIST)
        {
            AllowDuplicates = true;
            if(val != null)
            {
                foreach(var elm in val)
                {
                    Add(Attr.Copy(elm));
                }
            }
        }

        public AttrList(AttrDic otherDic) : this()
        {
            foreach(var pair in otherDic)
            {
                Add(pair.Value);
            }
        }

        public AttrList(AttrList otherList) : this(otherList.List)
        {
        }

        public static AttrList Invalid
        {
            get
            {
                return new AttrList();
            }
        }

        public Attr this[int index]
        {
            get
            {
                return List[index];
            }
            set
            {
                List[index] = value;
            }
        }

        public override string ToString()
        {
            var b = new StringBuilder();
            b.Append("[");
            var i = 0;
            foreach(var elm in this)
            {
                b.Append(elm);
                if(i != List.Count - 1)
                {
                    b.Append(",");
                }
                i++;
            }
            b.Append("]");
            return b.ToString();
        }

        public  IEnumerator<Attr> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public bool Set(Attr attr, int idx)
        {
            if(AllowDuplicates || (!AllowDuplicates && !Contains(attr)))
            {
                List[idx] = attr;
                return true;
            }
            return false;
        }

        public bool Add(Attr attr)
        {
            if(AllowDuplicates || (!AllowDuplicates && !Contains(attr)))
            {
                List.Add(attr);
                return true;
            }
            return false;
        }

        public bool AddValue(string key, bool val)
        {
            return Add(new AttrBool(val));
        }
        
        public bool AddValue(int val)
        {
            return Add(new AttrInt(val));
        }
        
        public bool AddValue(short val)
        {
            return Add(new AttrInt(val));
        }
        
        public bool AddValue(float val)
        {
            return Add(new AttrFloat(val));
        }
        
        public bool AddValue(double val)
        {
            return Add(new AttrDouble(val));
        }
        
        public bool AddValue(long val)
        {
            return Add(new AttrLong(val));
        }
        
        public bool AddValue(string val)
        {
            return Add(new AttrString(val));
        }
                
        public bool AddValue(bool val)
        {
            return Add(new AttrBool(val));
        }

        public void Clear()
        {
            List.Clear();
        }
        
        public Attr Get(int idx)
        {
            return List[idx];
        }

        public AttrValue GetValue(int idx)
        {
            return List[idx].AsValue;
        }

        public int IndexOf(Attr attr)
        {
            return List.IndexOf(attr);
        }

        public bool Contains(Attr attr)
        {
            return List.Contains(attr);
        }

        public bool Remove(Attr attr)
        {
            return List.Remove(attr);
        }

        public void RemoveAt(int idx)
        {
            List.RemoveAt(idx);
        }

        public override AttrDic AsDic
        {
            get
            {
                return new AttrDic(this);
            }
        }

        public override AttrList AsList
        {
            get
            {
                return this;
            }
        }

        public static bool operator ==(AttrList la, AttrList ra)
        {
            if(System.Object.ReferenceEquals(la, ra))
            {
                return true;
            }
            if(((object)la == null) || ((object)ra == null))
            {
                return false;
            }

            if(la.Count != ra.Count)
            {
                return false;
            }

            using(var raItr = ra.GetEnumerator())
            {
                foreach(var elm in la)
                {
                    if(elm != raItr.Current)
                    {
                        return false;
                    }
                    raItr.MoveNext();
                }
            }

            return true;
        }
        
        public static bool operator !=(AttrList la, AttrList ra)
        {
            return !(la == ra);
        }

        public override bool Equals(System.Object obj)
        {
            if(obj == null)
            {
                return false;
            }
            
            var p = obj as AttrList;
            if((System.Object)p == null)
            {
                return false;
            }
            
            return this == p;
        }

        public bool Equals(AttrList list)
        {
            if((object)list == null)
            {
                return false;
            }
            return this == list;
        }
        
        public override int GetHashCode()
        {
            return List.GetHashCode();
        }

        public List<T> ToList<T>()
        {
            var list = new List<T>();
            foreach(var elm in this)
            {
                list.Add(elm.AsValue.ToValue<T>());
            }
            
            return list;
        }

        public override void Dispose()
        {
            foreach(var elm in this)
            {
                if(elm != null)
                {
                    elm.Dispose();
                }
            }
            GC.SuppressFinalize(this);
        }
    }

};
