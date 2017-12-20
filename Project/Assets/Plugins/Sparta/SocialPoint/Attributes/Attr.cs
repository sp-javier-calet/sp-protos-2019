using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public enum AttrType
    {
        VALUE,
        DICTIONARY,
        LIST
    }

    public enum AttrValueType
    {
        EMPTY,
        STRING,
        BOOL,
        INT,
        LONG,
        FLOAT,
        DOUBLE
    }

    public abstract class Attr : IDisposable, ICloneable
    {
        public AttrType AttrType { get; private set; }

        public Attr(AttrType type)
        {
            AttrType = type;
        }

        [Obsolete("Use Clone()")]
        public static Attr Copy(Attr attr)
        {
            return (Attr)attr.Clone();
        }

        public abstract object Clone();

        public static bool IsNullOrEmpty(Attr attr)
        {
            if(attr == null)
            {
                return true;
            }
            if(attr.IsValue && attr.AsValue.AttrValueType == AttrValueType.EMPTY)
            {
                return true;
            }
            return false;
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


        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as Attr;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return (int)AttrType;
        }

        public virtual void AppendToStringBuilder(StringBuilder b)
        {
            DebugUtils.Assert(!IsDic && !IsList, "AttrDic and AttrList should be overriden");

            b.Append(ToString());
        }

        public override string ToString()
        {
            return string.Empty;
        }

        public virtual AttrValue AsValue
        {
            get
            {
                return InvalidValue;
            }
        }

        public virtual AttrDic AsDic
        {
            get
            {
                return InvalidDic;
            }
        }

        public virtual AttrList AsList
        {
            get
            {
                return InvalidList;
            }
        }

        public static AttrDic InvalidDic
        {
            get
            {
                return new AttrDic();
            }
        }

        public static AttrList InvalidList
        {
            get
            {
                return new AttrList();
            }
        }

        public static AttrValue InvalidValue
        {
            get
            {
                return new AttrEmpty();
            }
        }

        public static Attr Invalid
        {
            get
            {
                return InvalidValue;
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

    public abstract class AttrValue : Attr
    {
        public AttrValueType AttrValueType { get; private set; }

        public AttrValue(AttrValueType type) : base(AttrType.VALUE)
        {
            AttrValueType = type;
        }

        public AttrValue(AttrValue attr) : this(attr.AttrValueType)
        {
        }

        public virtual float ToFloat()
        {
            return ToInt();
        }

        public virtual int ToInt()
        {
            DebugUtils.Assert(false, "AttrValue: Can't convert AttrValue to this type, unimplemented method in child class");
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

        [Obsolete("Use Clone()")]
        public static AttrValue Copy(AttrValue attr)
        {
            return (AttrValue)attr.Clone();
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
            case AttrValueType.EMPTY:
                return (AttrEmpty)la == (AttrEmpty)ra;
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

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrValue;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (int)AttrValueType;
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

    public sealed class AttrEmpty : AttrValue
    {
        public AttrEmpty() : base(AttrValueType.EMPTY)
        {
        }

        public override object Clone()
        {
            return new AttrEmpty();
        }

        public static bool operator ==(AttrEmpty la, AttrEmpty ra)
        {
            return true;
        }

        public static bool operator !=(AttrEmpty la, AttrEmpty ra)
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrEmpty;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Empty;
        }

        public override int ToInt()
        {
            return 0;
        }

        public override long ToLong()
        {
            return 0L;
        }

        public override float ToFloat()
        {
            return 0.0f;
        }

        public override double ToDouble()
        {
            return 0.0;
        }

        public override bool ToBool()
        {
            return false;
        }
    }

    public sealed class AttrBool : AttrValue
    {
        bool _value;

        public AttrBool(bool v) : base(AttrValueType.BOOL)
        {
            _value = v;
        }

        public AttrBool(AttrBool attr) : this(attr.ToBool())
        {
        }

        public override object Clone()
        {
            return new AttrBool(this);
        }

        public override bool ToBool()
        {
            return _value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override float ToFloat()
        {
            return _value ? 1.0f : 0.0f;
        }

        public override int ToInt()
        {
            return _value ? 1 : 0;
        }

        public override long ToLong()
        {
            return _value ? 1L : 0L;
        }

        public override double ToDouble()
        {
            return _value ? 1.0 : 0.0;
        }

        public override void SetFloat(float val)
        {
            _value = val != 0.0f;
        }

        public override void SetInt(int val)
        {
            _value = val == 0 ? false : true;
        }

        public override void SetBool(bool val)
        {
            _value = val;
        }

        const string TrueString = "true";
        const string FalseString = "false";
        const string ZeroString = "0";

        public override void SetString(string val)
        {
            string testVal = val.Trim();
            if(testVal == TrueString)
            {
                _value = true;
            }
            else if(testVal == FalseString)
            {
                _value = false;
            }
            else if(testVal == ZeroString || testVal.Length == 0)
            {
                _value = false;
            }
            else
            {
                _value = true;
            }
        }

        public static bool operator ==(AttrBool la, AttrBool ra)
        {
            return la._value == ra._value;
        }

        public static bool operator !=(AttrBool la, AttrBool ra)
        {
            return !(la == ra);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrBool;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _value.GetHashCode();
        }

        protected override object GetValue()
        {
            return _value;
        }

    }

    public sealed class AttrInt : AttrValue
    {
        int _value;

        public AttrInt(int v) : base(AttrValueType.INT)
        {
            _value = v;
        }

        public AttrInt(AttrInt attr) : this(attr.ToInt())
        {
        }

        public override object Clone()
        {
            return new AttrInt(this);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override float ToFloat()
        {
            DebugUtils.Assert(_value <= float.MaxValue && _value >= float.MinValue, "AttrInt: can't cast to float: losing precision");
            return (float)_value;
        }

        public override long ToLong()
        {
            return (long)_value;
        }

        public override int ToInt()
        {
            return _value;
        }

        public override void SetInt(int val)
        {
            _value = val;
        }

        public override void SetString(string val)
        {
            int.TryParse(val, out _value);
        }

        public static bool operator ==(AttrInt la, AttrInt ra)
        {
            return la._value == ra._value;
        }

        public static bool operator !=(AttrInt la, AttrInt ra)
        {
            return !(la == ra);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrInt;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _value.GetHashCode();
        }

        protected override object GetValue()
        {
            return _value;
        }
    }

    public sealed class AttrLong : AttrValue
    {
        long _value;

        public AttrLong(long v) : base(AttrValueType.LONG)
        {
            _value = v;
        }

        public AttrLong(AttrLong attr) : this(attr.ToLong())
        {
        }

        public override object Clone()
        {
            return new AttrLong(this);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override float ToFloat()
        {
            return (float)_value;
        }

        public override int ToInt()
        {
            DebugUtils.Assert(_value <= int.MaxValue && _value >= int.MinValue, "AttrLong: Can't cast from AttrLong to int, losing precision");
            return (int)_value;
        }

        public override long ToLong()
        {
            return _value;
        }

        public override double ToDouble()
        {
            return (double)_value;
        }

        public override void SetFloat(float val)
        {
            DebugUtils.Assert(false, "AttrLong: Can't store float, losing precision");
            _value = (long)val;
        }

        public override void SetInt(int val)
        {
            _value = (long)val;
        }

        public override void SetLong(long val)
        {
            _value = val;
        }

        public override void SetDouble(double val)
        {
            DebugUtils.Assert(false, "AttrLong: Can't store double, losing precision");
            _value = (long)val;
        }

        public override void SetString(string val)
        {
            long.TryParse(val, out _value);
        }

        public static bool operator ==(AttrLong la, AttrLong ra)
        {
            return la._value == ra._value;
        }

        public static bool operator !=(AttrLong la, AttrLong ra)
        {
            return !(la == ra);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrLong;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _value.GetHashCode();
        }

        protected override object GetValue()
        {
            return _value;
        }
    }

    public sealed class AttrString : AttrValue
    {
        string _value;

        public AttrString(string v = null) : base(AttrValueType.STRING)
        {
            if(v == null)
            {
                v = string.Empty;
            }
            _value = v;
        }

        public AttrString(AttrString attr) : this(attr.ToString())
        {
        }

        public override object Clone()
        {
            return new AttrString(this);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override float ToFloat()
        {
            float r = 0.0f;
            if(float.TryParse(_value, out r))
            {
                return r;
            }
            return 0.0f;
        }

        public override int ToInt()
        {
            int r = 0;
            if(int.TryParse(_value, out r))
            {
                return r;
            }
            return 0;
        }

        public override long ToLong()
        {
            long r = 0L;
            if(long.TryParse(_value, out r))
            {
                return r;
            }
            return 0L;
        }

        public override double ToDouble()
        {
            double r = 0.0;
            if(double.TryParse(_value, out r))
            {
                return r;
            }
            return 0.0;
        }

        public override bool ToBool()
        {
            bool r = false;
            if(bool.TryParse(_value, out r))
            {
                return r;
            }
            return false;
        }

        public override void SetFloat(float val)
        {
            _value = val.ToString();
        }

        public override void SetInt(int val)
        {
            _value = val.ToString();
        }

        public override void SetBool(bool val)
        {
            _value = val.ToString();
        }

        public override void SetLong(long val)
        {
            _value = val.ToString();
        }

        public override void SetDouble(double val)
        {
            _value = val.ToString();
        }

        public override void SetString(string val)
        {
            _value = val.ToString();
        }

        public static bool operator ==(AttrString la, AttrString ra)
        {
            return la._value == ra._value;
        }

        public static bool operator !=(AttrString la, AttrString ra)
        {
            return !(la == ra);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrString;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _value.GetHashCode();
        }

        protected override object GetValue()
        {
            return _value;
        }
    }

    public sealed class AttrFloat : AttrValue
    {
        float _value;

        public AttrFloat(float v) : base(AttrValueType.FLOAT)
        {
            _value = v;
        }

        public AttrFloat(AttrFloat attr) : this(attr.ToFloat())
        {
        }

        public override object Clone()
        {
            return new AttrFloat(this);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override float ToFloat()
        {
            return _value;
        }

        public override int ToInt()
        {
            DebugUtils.Assert(false, "AttrFloat: Can't cast from AttrFloat to int, losing precision");
            return (int)_value;
        }

        public override double ToDouble()
        {
            return (double)_value;
        }

        public override long ToLong()
        {
            DebugUtils.Assert(false, "AttrFloat: Can't cast from AttrFloat to long, losing precision");
            return (long)_value;
        }

        public override void SetString(string val)
        {
            float.TryParse(val, out _value);
        }

        public override void SetInt(int val)
        {
            _value = (float)val;
        }

        public override void SetLong(long val)
        {
            _value = (float)val;
        }

        public override void SetFloat(float val)
        {
            _value = val;
        }

        public static bool operator ==(AttrFloat la, AttrFloat ra)
        {
            return la._value == ra._value;
        }

        public static bool operator !=(AttrFloat la, AttrFloat ra)
        {
            return !(la == ra);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrFloat;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _value.GetHashCode();
        }

        protected override object GetValue()
        {
            return _value;
        }
    }

    public sealed class AttrDouble : AttrValue
    {
        double _value;

        public AttrDouble(double v) : base(AttrValueType.DOUBLE)
        {
            _value = v;
        }

        public AttrDouble(AttrDouble attr) : this(attr.ToDouble())
        {
        }

        public override object Clone()
        {
            return new AttrDouble(this);
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public override float ToFloat()
        {
            DebugUtils.Assert(_value <= float.MaxValue && _value >= float.MinValue, "AttrDouble: can't cast to float: value overflow");
            return (float)_value;
        }

        public override int ToInt()
        {
            DebugUtils.Assert(false, "AttrDouble: Can't cast from AttrDouble to int, losing precision");
            return (int)_value;
        }

        public override long ToLong()
        {
            DebugUtils.Assert(false, "AttrDouble: Can't cast from AttrDouble to long, losing precision");
            return (long)_value;
        }

        public override double ToDouble()
        {
            return _value;
        }

        public override void SetFloat(float val)
        {
            _value = (double)val;
        }

        public override void SetInt(int val)
        {
            _value = (double)val;
        }

        public override void SetLong(long val)
        {
            _value = (double)val;
        }

        public override void SetDouble(double val)
        {
            _value = val;
        }

        public override void SetString(string val)
        {
            double.TryParse(val, out _value);
        }

        public static bool operator ==(AttrDouble la, AttrDouble ra)
        {
            return la._value == ra._value;
        }

        public static bool operator !=(AttrDouble la, AttrDouble ra)
        {
            return !(la == ra);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrDouble;
            if((object)p == null)
            {
                return false;
            }

            return this == p;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ _value.GetHashCode();
        }

        protected override object GetValue()
        {
            return _value;
        }
    }

    public class AttrDic : Attr, IEnumerable<KeyValuePair<string, Attr>>
    {
        Dictionary<string, Attr> _value = new Dictionary<string, Attr>();

        public int Count
        {
            get{ return _value.Count; }
        }

        public AttrDic(Dictionary<string, Attr> other = null) : base(AttrType.DICTIONARY)
        {
            if(other != null)
            {
                var itr = other.GetEnumerator();
                while(itr.MoveNext())
                {
                    Attr val = null;
                    var pair = itr.Current;
                    if(pair.Value != null)
                    {
                        val = (Attr)pair.Value.Clone();
                    }
                    Set(pair.Key, val);
                }
                itr.Dispose();
            }
        }

        public AttrDic(AttrDic other) : this(other._value)
        {
        }

        public AttrDic(Dictionary<string, string> other) : this()
        {
            if(other != null)
            {
                var itr = other.GetEnumerator();
                while(itr.MoveNext())
                {
                    var pair = itr.Current;
                    SetValue(pair.Key, pair.Value);
                }
                itr.Dispose();
            }
        }

        public AttrDic(AttrList other) : this()
        {
            if(other != null)
            {
                for(var i = 0; i < other.Count; i++)
                {
                    Set(i.ToString(), (Attr)other.Get(i).Clone());
                }
            }
        }

        public override object Clone()
        {
            return new AttrDic(this);
        }

        public ICollection<string> Keys
        {
            get
            {
                return _value.Keys;
            }
        }

        public bool ContainsValue(Attr value)
        {
            return _value.ContainsValue(value);
        }

        public bool ContainsValue(string value)
        {
            return ContainsValue(new AttrString((value != null ? value : "")));
        }

        public Dictionary<string, Attr>.Enumerator GetEnumerator()
        {
            return _value.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, Attr>> IEnumerable<KeyValuePair<string, Attr>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override void AppendToStringBuilder(StringBuilder b)
        {
            b.Append("{");
            var i = 0;
            var itr = GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                b.Append(pair.Key);
                b.Append(" = ");
                pair.Value.AppendToStringBuilder(b);
                if(i < Count - 1)
                {
                    b.Append(", ");
                }
                i++;
            }
            itr.Dispose();
            b.Append("}");
        }

        public override string ToString()
        {
            var b = StringUtils.StartBuilder();
            AppendToStringBuilder(b);
            return StringUtils.FinishBuilder(b);
        }

        public void Clear()
        {
            _value.Clear();
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
                _value.Add(key, attr);
            }
            else
            {
                _value[key] = attr;
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
            return _value.ContainsKey(key);
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
                return Attr.Invalid;
            }
            return _value[key];
        }

        public AttrValue GetValue(string key)
        {
            return Get(key).AsValue;
        }

        public bool Remove(string key)
        {
            return _value.Remove(key);
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

            var itr = la.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                if(!ra.ContainsKey(pair.Key) || !pair.Value.Equals(ra.Get(pair.Key)))
                {
                    itr.Dispose();
                    return false;
                }
            }
            itr.Dispose();

            return true;
        }

        public static bool operator !=(AttrDic la, AttrDic ra)
        {
            return !(la == ra);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrDic;
            if((object)p == null)
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
            int seed = 0;
            var itr = _value.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                seed = CryptographyUtils.HashCombine(seed, item.Key.GetHashCode());
                seed = CryptographyUtils.HashCombine(seed, item.Value.GetHashCode());
            }
            itr.Dispose();
            return base.GetHashCode() ^ seed;
        }

        public Dictionary<string,V> ToDictionary<V>()
        {
            var dic = new Dictionary<string, V>();
            var itr = GetEnumerator();
            while(itr.MoveNext())
            {
                V val = default(V);
                var pair = itr.Current;
                if(pair.Value != null)
                {
                    val = pair.Value.AsValue.ToValue<V>();
                }
                dic.Add(pair.Key, val);
            }
            itr.Dispose();
            return dic;
        }

        public override void Dispose()
        {
            var itr = GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                if(pair.Value != null)
                {
                    pair.Value.Dispose();
                }
            }
            itr.Dispose();
        }
    }

    public sealed class AttrList : Attr, IEnumerable<Attr>
    {
        public bool AllowDuplicates { get; set; }

        List<Attr> _value = new List<Attr>();

        public int Count
        {
            get
            {
                return _value.Count;
            }
        }

        public AttrList(List<Attr> other = null) : base(AttrType.LIST)
        {
            AllowDuplicates = true;
            if(other != null)
            {
                for(var i = 0; i < other.Count; i++)
                {
                    Attr val = null;
                    var elm = other[i];
                    if(elm != null)
                    {
                        val = (Attr)elm.Clone();
                    }
                    Add(val);
                }
            }
        }

        public AttrList(List<string> other) : base(AttrType.LIST)
        {
            AllowDuplicates = true;
            if(other != null)
            {
                for(var i = 0; i < other.Count; i++)
                {
                    AddValue(other[i]);
                }
            }
        }

        public AttrList(AttrDic other) : this()
        {
            var itr = other.GetEnumerator();
            while(itr.MoveNext())
            {
                var pair = itr.Current;
                Attr val = null;
                if(pair.Value != null)
                {
                    val = (Attr)pair.Value.Clone();
                }
                Add(val);
            }
            itr.Dispose();
        }

        public AttrList(AttrList other) : this(other._value)
        {
        }

        public override object Clone()
        {
            return new AttrList(this);
        }

        public Attr this[int idx]
        {
            get
            {
                return Get(idx);
            }
            set
            {
                Set(idx, value);
            }
        }

        public V Get<V>(int idx)
        {
            var elm = Get(idx);
            return elm != null ? elm.AsValue.ToValue<V>() : default(V);
        }

        public List<V> ToList<V>()
        {
            var list = new List<V>();
            for(var i = 0; i < Count; i++)
            {
                list.Add(Get<V>(i));
            }
            return list;
        }

        public V[] ToArray<V>()
        {
            V[] array = new V[Count];
            for(var i = 0; i < Count; i++)
            {
                array[i] = Get<V>(i);
            }
            return array;
        }

        public override void AppendToStringBuilder(StringBuilder b)
        {
            b.Append("[");
            for(var i = 0; i < Count; i++)
            {
                b.Append(Get(i));
                if(i != _value.Count - 1)
                {
                    b.Append(",");
                }
            }
            b.Append("]");
        }

        public override string ToString()
        {
            var b = StringUtils.StartBuilder();
            AppendToStringBuilder(b);
            return StringUtils.FinishBuilder(b);
        }

        public List<Attr>.Enumerator GetEnumerator()
        {
            return _value.GetEnumerator();
        }

        IEnumerator<Attr> IEnumerable<Attr>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Set(int idx, Attr attr)
        {
            if(AllowDuplicates || (!AllowDuplicates && !Contains(attr)))
            {
                _value[idx] = attr;
                return true;
            }
            return false;
        }

        public bool Insert(int idx, Attr attr)
        {
            if(idx < 0 || idx > _value.Count)
            {
                return false;
            }
            if(AllowDuplicates || (!AllowDuplicates && !Contains(attr)))
            {
                _value.Insert(idx, attr);
                return true;
            }
            return false;
        }

        public bool Add(Attr attr)
        {
            if(AllowDuplicates || (!AllowDuplicates && !Contains(attr)))
            {
                _value.Add(attr);
                return true;
            }
            return false;
        }

        public bool AddValue(bool val)
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

        public bool SetValue(int idx, bool val)
        {
            return Set(idx, new AttrBool(val));
        }

        public bool SetValue(int idx, int val)
        {
            return Set(idx, new AttrInt(val));
        }

        public bool SetValue(int idx, short val)
        {
            return Set(idx, new AttrInt(val));
        }

        public bool SetValue(int idx, float val)
        {
            return Set(idx, new AttrFloat(val));
        }

        public bool SetValue(int idx, double val)
        {
            return Set(idx, new AttrDouble(val));
        }

        public bool SetValue(int idx, long val)
        {
            return Set(idx, new AttrLong(val));
        }

        public bool SetValue(int idx, string val)
        {
            return Set(idx, new AttrString(val));
        }

        public bool InsertValue(int idx, bool val)
        {
            return Insert(idx, new AttrBool(val));
        }

        public bool InsertValue(int idx, int val)
        {
            return Insert(idx, new AttrInt(val));
        }

        public bool InsertValue(int idx, short val)
        {
            return Insert(idx, new AttrInt(val));
        }

        public bool InsertValue(int idx, float val)
        {
            return Insert(idx, new AttrFloat(val));
        }

        public bool InsertValue(int idx, double val)
        {
            return Insert(idx, new AttrDouble(val));
        }

        public bool InsertValue(int idx, long val)
        {
            return Insert(idx, new AttrLong(val));
        }

        public bool InsertValue(int idx, string val)
        {
            return Insert(idx, new AttrString(val));
        }

        public void Clear()
        {
            _value.Clear();
        }

        public Attr Get(int idx)
        {
            if(idx >= Count)
            {
                return Attr.Invalid;
            }
            return _value[idx];
        }

        public AttrValue GetValue(int idx)
        {
            return _value[idx].AsValue;
        }

        public int IndexOf(Attr attr)
        {
            return _value.IndexOf(attr);
        }

        public bool Contains(Attr attr)
        {
            return _value.Contains(attr);
        }

        public bool Remove(Attr attr)
        {
            return _value.Remove(attr);
        }

        public void RemoveAt(int idx)
        {
            _value.RemoveAt(idx);
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

            for(var i = 0; i < ra.Count; i++)
            {
                if(!la[i].Equals(ra[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(AttrList la, AttrList ra)
        {
            return !(la == ra);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
            {
                return false;
            }

            var p = obj as AttrList;
            if((object)p == null)
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
            int seed = 0;
            var itr = _value.GetEnumerator();
            while(itr.MoveNext())
            {
                var item = itr.Current;
                seed = CryptographyUtils.HashCombine(seed, item.GetHashCode());
            }
            itr.Dispose();
            return base.GetHashCode() ^ seed;
        }

        public override void Dispose()
        {
            for(var i = 0; i < Count; i++)
            {
                var elm = Get(i);
                if(elm != null)
                {
                    elm.Dispose();
                }
            }
            GC.SuppressFinalize(this);
        }
    }

};