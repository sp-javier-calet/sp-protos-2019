using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.XCodeEditor
{
    public class PBXDictionary : Dictionary<string, object>
    {
        public PBXDictionary()
        {
            
        }
        
        public PBXDictionary(Hashtable table)
        {
            foreach(DictionaryEntry item in table)
            {
                string k = (string)item.Key;
                if(item.Value is Hashtable)
                {
                    this.Add(k, new PBXDictionary((Hashtable)item.Value));
                }
                else
                if(item.Value is ArrayList)
                {
                    this.Add(k, new PBXList((ArrayList)item.Value));
                }
                else
                {
                    this.Add(k, item.Value);
                }
            }
        }

        public void Append(PBXDictionary dictionary)
        {
            foreach(var item in dictionary)
            {
                if(!this.ContainsKey(item.Key))
                {
                    this.Add(item.Key, item.Value);
                }
            }
        }
        
        public void Append<T>(PBXDictionary<T> dictionary) where T : PBXObject
        {
            foreach(var item in dictionary)
            {
                if(!this.ContainsKey(item.Key))
                {
                    this.Add(item.Key, item.Value);
                }
            }
        }

        public void Combine(PBXDictionary dictionary)
        {
            foreach(var item in dictionary)
            {
                if(!this.ContainsKey(item.Key))
                {
                    this[item.Key] = item.Value;
                }
                else
                if(this[item.Key] is PBXDictionary && item.Value is PBXDictionary)
                {
                    ((PBXDictionary)this[item.Key]).Combine((PBXDictionary)item.Value);
                }
                else
                if(this[item.Key] is PBXList && item.Value is PBXList)
                {
                    ((PBXList)this[item.Key]).Add((PBXList)item.Value);
                }
                else
                {
                    this[item.Key] = item.Value;
                }
            }
        }
    }
    
    public class PBXDictionary<T> : Dictionary<string, T> where T : PBXObject
    {
        public PBXDictionary()
        {
            
        }
        
        public PBXDictionary(PBXDictionary genericDictionary)
        {
            foreach(KeyValuePair<string, object> currentItem in genericDictionary)
            {
                if(((string)((PBXDictionary)currentItem.Value)["isa"]).CompareTo(typeof(T).Name) == 0)
                {
                    T instance = (T)System.Activator.CreateInstance(typeof(T), currentItem.Key, (PBXDictionary)currentItem.Value);
                    this.Add(currentItem.Key, instance);
                }
            }   
        }
        
        public void Add(T newObject)
        {
            this.Add(newObject.guid, newObject);
        }
        
        public void Append(PBXDictionary<T> dictionary)
        {
            foreach(KeyValuePair<string, T> item in dictionary)
            {
                this.Add(item.Key, (T)item.Value);
            }
        }
        
    }
}
