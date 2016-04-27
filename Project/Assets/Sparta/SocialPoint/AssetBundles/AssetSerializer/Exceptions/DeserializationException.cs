using System;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.AssetSerializer.Exceptions
{
    public class DeserializationException: Exception
    {
        public List<string> MissingComponents;
        public List<string> MissingFields;
        public List<string> MissingReaders;
        public List<string> MissingTypes;

        public DeserializationException()
        {
            MissingComponents = new List<string>();
            MissingFields = new List<string>();
            MissingReaders = new List<string>();
            MissingTypes = new List<string>();
        }

        override public string ToString()
        {
            return "DeserializationException - MissingComponents: "
                + string.Join(", ", MissingComponents.ToArray())
                + " MissingFields: " + string.Join(", ", MissingFields.ToArray())
                + " MissingReaders: " + string.Join(", ", MissingReaders.ToArray())
                + " MissingTypes: " + string.Join(", ", MissingTypes.ToArray());
        }
    }

    public class MissingTypeException: Exception
    {
        public string MissingType;

        public MissingTypeException(string missingType)
        {
            MissingType = missingType;
        }

        override public string ToString()
        {
            return "DeserializationException - MissingType: " + MissingType;
        }
    }

    public class DuplicatedChildNameException: Exception
    {
        public string name;
        public string prefix = "";
        
        public DuplicatedChildNameException(string _name)
        {
            name = _name;
        }
        
        public DuplicatedChildNameException(string _name, string _prefix)
        {
            name = _name;
            prefix = _prefix;
        }
        
        override public string ToString()
        {
            return "DeserializationException - DuplicatedChildNameException: " + prefix + name;
        }
    }
}