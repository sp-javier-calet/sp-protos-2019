using UnityEngine;
using System.Collections;

public static class ObjectSerializationUtils
{
    public static int GetByteLength(this object obj)
    {
        long size = 0;
        object o = new object();
        using(System.IO.Stream s = new System.IO.MemoryStream())
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(s, o);
            size = s.Length;
        }
        return (int)size;
    }
}