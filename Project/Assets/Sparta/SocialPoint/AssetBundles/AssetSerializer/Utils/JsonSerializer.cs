using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SocialPoint.Attributes;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif


namespace SocialPoint.AssetSerializer.Utils.JsonSerialization
{
    public sealed class JsonData
    {
        private Attr attr;

        public JsonData (Attr _attr)
        {
            attr = _attr;
        }

        public static JsonData Parse(string data, IAttrParser attrParser=null)
        {
            UnityEngine.Profiler.BeginSample ("Parse string");
#if ATTR_USING_SIMPLEJSON
            var attParser = attrParser ?? new SimpleJsonAttrParser ();
#else
            var attParser = attrParser ?? new LitJsonAttrParser ();
#endif
            UnityEngine.Profiler.BeginSample ("Parse attr");
            Attr parsedAttr = attParser.ParseString (data);
            UnityEngine.Profiler.EndSample ();

            JsonData val = (JsonData)parsedAttr;
            UnityEngine.Profiler.EndSample ();

            return val;
        }

        public JsonData this[int index]
        {
            get
            {
                if (IsAttrNull(attr.AsList[index]))
                    return null;
                else
                    return (JsonData)attr.AsList[index];
            }
            set
            {
                attr.AsList.Set(index, (Attr)value);
            }
        }

        public int Count
        {
            get
            {
                return attr.AsList.Count;
            }
        }

        public JsonData this[string key]
        {
            get
            {
                if (IsAttrNull(attr.AsDic[key]))
                    return null;
                else
                    return (JsonData)attr.AsDic[key];
            }
            set
            {
                attr.AsDic[key] = (Attr)value;
            }
        }

        public bool ContainsKey(string key)
        {
            if(!IsAttrNull(attr))
            {
                return attr.AsDic.ContainsKey(key);
            }
            return false;
        }

        public override string ToString ()
        {
            return attr.ToString ();
        }

        public static bool IsAttrNull(Attr attr)
        {
            return Attr.IsNullOrEmpty(attr);
        }

        #region Explicit Conversions
        public static explicit operator Boolean (JsonData data)
        {
            UnityEngine.Profiler.BeginSample("Bool IC");
            if (!data.attr.IsValue || data.attr.AsValue.AttrValueType != AttrValueType.BOOL)
                throw new InvalidCastException (
                    "Instance of JsonData doesn't hold a boolean");

            bool val;

            val = data.attr.AsValue.ToBool ();
            UnityEngine.Profiler.EndSample();

            return val;
        }

        public static explicit operator String (JsonData data)
        {
            UnityEngine.Profiler.BeginSample("String IC");
            if (!data.attr.IsValue || data.attr.AsValue.AttrValueType != AttrValueType.STRING)
                throw new InvalidCastException (
                    "Instance of JsonData doesn't hold a string");

            string val;

            val = data.attr.AsValue.ToString ();
            UnityEngine.Profiler.EndSample();

            return val;
        }
        
        public static explicit operator Double (JsonData data)
        {
            UnityEngine.Profiler.BeginSample("Double IC");
            if (!data.attr.IsValue ||
                data.attr.AsValue.AttrValueType != AttrValueType.DOUBLE &&
                data.attr.AsValue.AttrValueType != AttrValueType.FLOAT &&
                data.attr.AsValue.AttrValueType != AttrValueType.LONG &&
                data.attr.AsValue.AttrValueType != AttrValueType.INT)
                throw new InvalidCastException (
                    "Instance of JsonData doesn't hold a double");

            double val;

            if (data.attr.AsValue.AttrValueType != AttrValueType.DOUBLE) {
                if (!double.TryParse(data.attr.AsValue.ToString (), out val))
                    throw new InvalidCastException (
                        String.Format ("Instance of JsonData holds a {0} but not a double", val.GetType ()));
            }
            else
                val = data.attr.AsValue.ToLong ();
            UnityEngine.Profiler.EndSample();

            return val;
        }

        public static explicit operator Int64 (JsonData data)
        {
            UnityEngine.Profiler.BeginSample("Long IC");
            if (!data.attr.IsValue ||
                data.attr.AsValue.AttrValueType != AttrValueType.LONG &&
                data.attr.AsValue.AttrValueType != AttrValueType.INT &&
                data.attr.AsValue.AttrValueType != AttrValueType.DOUBLE &&
                data.attr.AsValue.AttrValueType != AttrValueType.FLOAT)
                throw new InvalidCastException (
                    "Instance of JsonData doesn't hold a long");

            long val;

            if (data.attr.AsValue.AttrValueType != AttrValueType.LONG) {
                if (!long.TryParse(data.attr.AsValue.ToString (), out val))
                    throw new InvalidCastException (
                        String.Format ("Instance of JsonData holds a {0} but not a long", val.GetType ()));
            }
            else
                val = data.attr.AsValue.ToLong ();
            UnityEngine.Profiler.EndSample();

            return val;
        }

        public static explicit operator Int32 (JsonData data)
        {
            UnityEngine.Profiler.BeginSample("Int IC");
            if (!data.attr.IsValue || 
                data.attr.AsValue.AttrValueType != AttrValueType.INT &&
                data.attr.AsValue.AttrValueType != AttrValueType.LONG &&
                data.attr.AsValue.AttrValueType != AttrValueType.DOUBLE &&
                data.attr.AsValue.AttrValueType != AttrValueType.FLOAT)
                throw new InvalidCastException (
                    "Instance of JsonData doesn't hold an int");

            int val;

            if (data.attr.AsValue.AttrValueType != AttrValueType.INT) {
                if (!int.TryParse(data.attr.AsValue.ToString (), out val))
                    throw new InvalidCastException (
                        String.Format ("Instance of JsonData holds a {0} but not an int", val.GetType ()));
            }
            else
                val = data.attr.AsValue.ToInt ();
            UnityEngine.Profiler.EndSample();

            return val;
        }

        public static explicit operator JsonData (Attr data)
        {
            UnityEngine.Profiler.BeginSample("Jsondata IC");
            JsonData val;

            val = new JsonData(data);
            UnityEngine.Profiler.EndSample();

            return val;
        }

        public static explicit operator Attr (JsonData data)
        {

            UnityEngine.Profiler.BeginSample("Attr IC");
            Attr val;

            val = data.attr;
            UnityEngine.Profiler.EndSample();

            return val;
        }
        #endregion
    }

    public sealed class JsonWriter {

        string          currentProperty;
        Stack<Attr>     contextStack;
        Attr            root;

        StringBuilder   inst_str_builder;
        TextWriter      writer;

        public JsonWriter(StringBuilder stringBuilder)
        {
            inst_str_builder = stringBuilder;
            writer = new StringWriter (inst_str_builder);
            currentProperty = string.Empty;
            contextStack = new Stack<Attr> ();
        }

        public override string ToString ()
        {
            if (inst_str_builder == null)
                return string.Empty;
            
            return inst_str_builder.ToString ();
        }

        bool HasReachedEnd ()
        {
            return !JsonData.IsAttrNull(root) && contextStack.Count == 0;
        }

        void WritoToStringBuilder ()
        {
#if ATTR_USING_SIMPLEJSON
            writer.Write (new SimpleJsonAttrSerializer().SerializeString (root));
#else
            writer.Write (new LitJsonAttrSerializer().SerializeString (root));
#endif
        }

        public void Write (object value)
        {
            if (contextStack.Count == 0)
                throw new Exception ("JsonSerializer: 'Write' on empty stack");

            if (value == null)
            {
                if (contextStack.Peek().IsList)
                {
                    contextStack.Peek().AsList.Add (new AttrEmpty());
                }
                else
                {
                    if (currentProperty == string.Empty)
                        throw new Exception ("JsonSerializer: 'Write' in dict without property defined");

                    contextStack.Peek().AsDic[currentProperty] = new AttrEmpty();
                    currentProperty = string.Empty;
                }
            }
            else if (value is bool)
                WriteBool ((bool)value);
            else if (value is double)
                WriteDouble ((double)value);
            else if (value is float)
                WriteFloat ((float)value);
            else if (value is int)
                WriteInt ((int)value);
            else if (value is long)
                WriteLong ((long)value);
            else if (value is string)
                WriteString ((string)value);
            else
                throw new Exception (String.Format("JsonSerializer: 'Write' of unsupported type {0}", value.GetType()));
        }

        #region Value writers
        void WriteBool (bool boolean)
        {
            if (contextStack.Peek().IsList)
            {
                contextStack.Peek().AsList.AddValue(boolean);
            }
            else
            {
                if (currentProperty == string.Empty)
                    throw new Exception ("JsonSerializer: 'Write' in dict without property defined");

                contextStack.Peek().AsDic[currentProperty] = new AttrBool (boolean);
                currentProperty = string.Empty;
            }
        }
        
        void WriteDouble (double number)
        {
            if (contextStack.Peek().IsList)
            {
                contextStack.Peek().AsList.AddValue (number);
            }
            else
            {
                if (currentProperty == string.Empty)
                    throw new Exception ("JsonSerializer: 'Write' in dict without property defined");

                contextStack.Peek().AsDic[currentProperty] = new AttrDouble (number);
                currentProperty = string.Empty;
            }
        }

        void WriteFloat (float number)
        {
            double val = double.Parse(number.ToString ());

            if (contextStack.Peek().IsList)
            {
                contextStack.Peek().AsList.AddValue (val);
            }
            else
            {
                if (currentProperty == string.Empty)
                    throw new Exception ("JsonSerializer: 'Write' in dict without property defined");
                
                contextStack.Peek().AsDic[currentProperty] = new AttrDouble (val);
                currentProperty = string.Empty;
            }
        }
        
        void WriteInt (int number)
        {
            if (contextStack.Peek().IsList)
            {
                contextStack.Peek().AsList.AddValue (number);
            }
            else
            {
                if (currentProperty == string.Empty)
                    throw new Exception ("JsonSerializer: 'Write' in dict without property defined");

                contextStack.Peek().AsDic[currentProperty] = new AttrInt (number);
                currentProperty = string.Empty;
            }
        }
        
        void WriteLong (long number)
        {
            if (contextStack.Peek().IsList)
            {
                contextStack.Peek().AsList.AddValue (number);
            }
            else
            {
                if (currentProperty == string.Empty)
                    throw new Exception ("JsonSerializer: 'Write' in dict without property defined");

                contextStack.Peek().AsDic[currentProperty] = new AttrLong (number);
                currentProperty = string.Empty;
            }
        }
        
        void WriteString (string str)
        {
            if (contextStack.Peek().IsList)
            {
                contextStack.Peek().AsList.AddValue (str);
            }
            else
            {
                if (currentProperty == string.Empty)
                    throw new Exception ("JsonSerializer: 'Write' in dict without property defined");

                contextStack.Peek().AsDic[currentProperty] = new AttrString (str);
                currentProperty = string.Empty;
            }
        }
        #endregion

        public void WriteArrayStart ()
        {
            var newList = new AttrList ();

            if (contextStack.Count == 0)
            {
                if (!JsonData.IsAttrNull(root))
                    throw new Exception ("JsonSerializer: 'WriteArrayStart' called after serializer end");

                root = newList;
            }
            else if (contextStack.Peek().IsList)
            {
                contextStack.Peek().AsList.Add (newList);
            }
            else
            {
                if (currentProperty == string.Empty)
                    throw new Exception ("JsonSerializer: 'WriteArrayStart' in dict without property defined");

                contextStack.Peek().AsDic[currentProperty] = newList;
                currentProperty = string.Empty;
            }

            contextStack.Push (newList);
        }
        
        public void WriteArrayEnd ()
        {
            if (contextStack.Count == 0)
                throw new Exception ("JsonSerializer: 'WriteArrayEnd' on empty stack");

            if (contextStack.Peek().IsList)
            {
                contextStack.Pop ();
            }
            else
            {
                throw new Exception ("JsonSerializer: 'WriteArrayEnd' called on non array definition");
            }

            if (HasReachedEnd ())
                WritoToStringBuilder ();
        }

        public void WriteObjectStart ()
        {
            var newDic = new AttrDic ();

            if (contextStack.Count == 0)
            {
                if (!JsonData.IsAttrNull(root))
                    throw new Exception ("JsonSerializer: 'WriteObjectStart' called after serializer end");

                root = newDic;
            }
            else if (contextStack.Peek().IsList)
            {
                contextStack.Peek().AsList.Add (newDic);
            }
            else
            {
                if (currentProperty == string.Empty)
                    throw new Exception ("JsonSerializer: 'WriteObjectStart' in dict without property defined");
                
                contextStack.Peek().AsDic[currentProperty] = newDic;
                currentProperty = string.Empty;
            }
            
            contextStack.Push (newDic);
        }
        
        public void WriteObjectEnd ()
        {
            if (contextStack.Count == 0)
                throw new Exception ("JsonSerializer: 'WriteObjectEnd' on empty stack");

            if (contextStack.Peek().IsDic)
            {
                contextStack.Pop ();
            }
            else
            {
                throw new Exception ("JsonSerializer: 'WriteObjectEnd' called on non dict definition");
            }

            if (HasReachedEnd ())
                WritoToStringBuilder ();
        }
        
        public void WritePropertyName (string property_name)
        {
            if (contextStack.Count == 0)
                throw new Exception ("JsonSerializer: 'WritePropertyName' on empty stack");

            if (currentProperty != string.Empty)
                throw new Exception ("JsonSerializer: 'WritePropertyName' while an already current property is active");

            if (contextStack.Peek().IsDic)
            {
                currentProperty = property_name;
                contextStack.Peek().AsDic[property_name] = null;
            }
            else
            {
                throw new Exception ("JsonSerializer: 'WritePropertyName' called on non dict definition");
            }   
        }
    }
}