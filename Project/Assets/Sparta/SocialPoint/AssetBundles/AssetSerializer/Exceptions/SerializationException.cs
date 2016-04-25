using UnityEngine;
using System;
using System.Collections.Generic;

namespace SocialPoint.AssetSerializer.Exceptions
{
    public class SerializationMissingReferenceException: Exception
    {
        public string message = "";
		
        public SerializationMissingReferenceException(string msg)
        {
            message = msg;
        }
		
        override public string ToString()
        {
            return  "SerializationMissingReferenceException - Error: " + message;
        }
    }

    public class SerializationTypeNotSupportedException: Exception
    {
        private Type sType;
        public string message;

        public SerializationTypeNotSupportedException(Type tp, string msg="")
        {
            sType = tp;
            if(!msg.Equals(String.Empty))
            {
                message = msg;
            }
            else
            {
                message = String.Format("Serializable type not supported: {0}", sType.AssemblyQualifiedName);
            }
        }

        override public string ToString()
        {
            return  "SerializationTypeNotSupportedException - Error: " + message;
        }
    }

    public class SerializationPurePrefabNotSupportedException: Exception
    {
        public string field;
        public string name;
        public string message;

        public SerializationPurePrefabNotSupportedException(string objName, string fieldName, string msg="")
        {
            field = fieldName;
            name = objName;
            if(!msg.Equals(String.Empty))
            {
                message = msg;
            }
            else
            {
                message = String.Format("Pure prefab as property is not supported: {0}, {1}(field, value)", field, name);
            }
        }

        override public string ToString()
        {
            return  "SerializationPurePrefabNotSupported - Error: " + message;
        }
    }

    public class SerializationProcessException: UnityException
    {
        const string errMessage = "Serialization process finished with errors";

        public string[] serialization_errors;

        public SerializationProcessException(List<string> errors) : base(errMessage)
        {
            serialization_errors = new string[errors.Count];
            for(int i = 0; i < errors.Count; ++i)
            {
                serialization_errors[i] = errors[i];
            }
        }

        override public string ToString()
        {
            return  "SerializationProcessException - " + Message;
        }
    }
}
