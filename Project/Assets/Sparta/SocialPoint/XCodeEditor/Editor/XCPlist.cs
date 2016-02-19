using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Xml;

namespace SocialPoint.XCodeEditor
{
    public class XCPlistDictionary : Dictionary<string, object>
    {
        public XCPlistDictionary()
        {
        }

        public XCPlistDictionary(Hashtable table)
        {
            foreach(DictionaryEntry entry in table)
            {
                string key = (string)entry.Key;
                this[key] = XCPlist.CreateElement(fixTableValue(key, entry.Value));
            }
        }

        private object fixTableValue(string key, object value)
        {
            if(value is string)
            {
                string str = (string)value;
                IDictionary<string, string> vars = new Dictionary<string, string>();
                vars.Add("TIME_VERSION", DateTime.Now.ToString("yyMMddHHmm"));
                
                foreach(var entry in vars)
                {
                    str = str.Replace("{" + entry.Key + "}", entry.Value);
                }
                return str;
            }
            return value;
        }

        public void Combine(Hashtable table)
        {
            Combine(new XCPlistDictionary(table));
        }

        public void Combine(XCPlistDictionary dict)
        {
            foreach(var entry in dict)
            {
                if(!ContainsKey(entry.Key))
                {
                    this.Add(entry.Key, entry.Value);
                }
                else
                if(this[entry.Key] is XCPlistDictionary && entry.Value is XCPlistDictionary)
                {
                    ((XCPlistDictionary)this[entry.Key]).Combine((XCPlistDictionary)entry.Value);
                }
                else
                if(this[entry.Key] is XCPlistArray && entry.Value is XCPlistArray)
                {
                    ((XCPlistArray)this[entry.Key]).Combine((XCPlistArray)entry.Value);
                }
                else
                {
                    this[entry.Key] = entry.Value;
                }
            }
        }

    }

    public class XCPlistArray : List<object>
    {
        public XCPlistArray()
        {
        }
        
        public XCPlistArray(ArrayList array)
        {
            foreach(object entry in array)
            {
                Add(XCPlist.CreateElement(entry));
            }
        }

        public void Combine(ArrayList list)
        {
            Combine(new XCPlistArray(list));
        }
        
        public void Combine(XCPlistArray array)
        {
            AddRange(array);
        }
    }

    public class XCPlistData
    {
        private string contents;

        public XCPlistData(string contents)
        {
            this.contents = contents;
        }

        override public string ToString()
        {
            return this.contents;
        }
    }

    public class XCPlist : XCPlistDictionary
    {
        private const string XML_ELEMENT_PLIST = "plist";
        private const string XML_ELEMENT_TRUE = "true";
        private const string XML_ELEMENT_FALSE = "false";
        private const string XML_ELEMENT_KEY = "key";
        private const string XML_ELEMENT_STRING = "string";
        private const string XML_ELEMENT_INTEGER = "integer";
        private const string XML_ELEMENT_REAL = "real";
        private const string XML_ELEMENT_ARRAY = "array";
        private const string XML_ELEMENT_DICTIONARY = "dict";
        private const string XML_ELEMENT_DATE = "date";
        private const string XML_ELEMENT_DATA = "data";

        public string filePath { get; private set; }

        public static object CreateElement(object obj)
        {
            if(obj is ArrayList)
            {
                obj = new XCPlistArray((ArrayList)obj);
            }
            else
            if(obj is Hashtable)
            {
                obj = new XCPlistDictionary((Hashtable)obj);
            }

            return obj;
        }
        
        virtual public bool LoadFile(string filename)
        {
            filePath = filename;
            FileInfo projectFileInfo = new FileInfo(filename);
            if(!projectFileInfo.Exists)
            {
                XCDebug.LogWarning("File " + filename + " does not exist.");
                return false;
            }
            var file = projectFileInfo.OpenText();
            string contents = file.ReadToEnd();
            file.Close();
            return LoadString(contents);
        }

        virtual public bool LoadString(string contents)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(contents);
            return LoadDocument(doc);
        }

        virtual public bool LoadDocument(XmlDocument doc)
        {
            XmlNode node = doc.DocumentElement;
            if(node.Name != XML_ELEMENT_PLIST)
            {
                XCDebug.LogWarning("Xml does not contain plist tag");
                return false;
            }
            if(node.ChildNodes.Count < 1 || node.ChildNodes[0].Name != XML_ELEMENT_DICTIONARY)
            {
                XCDebug.LogWarning("Xml does not contain a dict tag");
                return false;
            }
            node = node.ChildNodes[0];
            XCPlistDictionary dict = (XCPlistDictionary)LoadObject(node);
            foreach(var elm in dict)
            {
                this[elm.Key] = elm.Value;
            }
            return true;
        }

        private object LoadObject(XmlNode node)
        {
            if(node.Name == XML_ELEMENT_ARRAY)
            {
                XCPlistArray array = new XCPlistArray();
                foreach(XmlNode child in node.ChildNodes)
                {
                    array.Add(LoadObject(child));
                }
                return array;
            }
            else
            if(node.Name == XML_ELEMENT_DICTIONARY)
            {
                XCPlistDictionary dict = new XCPlistDictionary();
                string key = null;
                foreach(XmlNode child in node.ChildNodes)
                {
                    if(child.Name == XML_ELEMENT_KEY)
                    {
                        key = child.InnerText;
                    }
                    else
                    if(key != null)
                    {
                        dict[key] = LoadObject(child);
                        key = null;
                    }
                }
                return dict;
            }
            else
            if(node.Name == XML_ELEMENT_STRING)
            {
                return node.InnerText;
            }
            else
            if(node.Name == XML_ELEMENT_TRUE)
            {
                return true;
            }
            else
            if(node.Name == XML_ELEMENT_FALSE)
            {
                return false;
            }
            else
            if(node.Name == XML_ELEMENT_INTEGER)
            {
                return Int32.Parse(node.InnerText);
            }
            else
            if(node.Name == XML_ELEMENT_REAL)
            {
                return float.Parse(node.InnerText);
            }
            else
            if(node.Name == XML_ELEMENT_DATE)
            {
                return DateTime.Parse(node.InnerText);
            }
            else
            if(node.Name == XML_ELEMENT_DATA)
            {
                return new XCPlistData(node.InnerText);
            }
            else
            {
                XCDebug.LogWarning("Invalid xml node: " + node.Name);
                return null;
            }
        }

        public bool SaveFile(string path=null)
        {
            if(path == null)
            {
                path = filePath;
            }
            if(path == null)
            {
                return false;
            }
            SaveDocument().Save(path);
            return true;
        }

        public XmlDocument SaveDocument()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(decl, doc.DocumentElement);
            XmlDocumentType doctype = doc.CreateDocumentType("plist", "-//Apple//DTD PLIST 1.0//EN",
                                             "http://www.apple.com/DTDs/PropertyList-1.0.dtd", null);
            doc.AppendChild(doctype);
            XmlNode plistNode = doc.CreateElement(XML_ELEMENT_PLIST);
            plistNode.AppendChild(SaveObject(this, doc));
            doc.AppendChild(plistNode);
            return doc;
        }

        private XmlNode SaveObject(object obj, XmlDocument doc)
        {
            if(obj is XCPlistDictionary)
            {
                XmlNode node = doc.CreateElement(XML_ELEMENT_DICTIONARY);
                foreach(var elm in (XCPlistDictionary)obj)
                {
                    XmlNode child = SaveObject(elm.Value, doc);
                    if(child != null)
                    {
                        XmlNode keyChild = doc.CreateElement(XML_ELEMENT_KEY);
                        keyChild.InnerText = elm.Key;
                        node.AppendChild(keyChild);
                        node.AppendChild(child);
                    }
                }
                return node;
            }
            else
            if(obj is XCPlistArray)
            {
                XmlNode node = doc.CreateElement(XML_ELEMENT_ARRAY);
                foreach(var elm in (XCPlistArray)obj)
                {
                    XmlNode child = SaveObject(elm, doc);
                    if(child != null)
                    {
                        node.AppendChild(child);
                    }
                }
                return node;
            }
            else
            if(obj is XCPlistData)
            {
                XmlNode node = doc.CreateElement(XML_ELEMENT_DATA);
                node.InnerText = (string)obj;
                return node;
            }
            else
            if(obj is DateTime)
            {
                XmlNode node = doc.CreateElement(XML_ELEMENT_DATE);
                node.InnerText = ((DateTime)obj).ToString("o");
                return node;
            }
            else
            if(obj is Boolean)
            {
                bool bobj = (Boolean)obj;
                return doc.CreateElement(bobj ? XML_ELEMENT_TRUE : XML_ELEMENT_FALSE);
            }
            else
            if(obj is Int32)
            {
                XmlNode node = doc.CreateElement(XML_ELEMENT_INTEGER);
                node.InnerText = obj.ToString();
                return node;
            }
            else
            if(obj is float)
            {
                XmlNode node = doc.CreateElement(XML_ELEMENT_REAL);
                node.InnerText = obj.ToString();
                return node;
            }
            else
            if(obj is String && !String.IsNullOrEmpty(obj.ToString()))
            {
                XmlNode node = doc.CreateElement(XML_ELEMENT_STRING);
                node.InnerText = (string)obj;
                return node;
            }
            else
            {
                XCDebug.LogWarning("Invalid object");
                return null;
            }
        }
        
        public void Dispose()
        {
        }
    }
}

