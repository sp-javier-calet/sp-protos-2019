using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;
using LitJson;
using System;

public sealed class BRCompResultsResponse {
    public sealed class CompilationResultData
    {
        public string author;
        public List<List<object>> bundles;
        public int id;
        public string status;
        public DateTime created;
    }
    


    public List<CompilationResultData> compilations;
    public string message;
    public string result;


    public static BRCompResultsResponse FromAttr(Attr data)
    {
        var serializer = new LitJsonAttrSerializer();
        var writer = new JsonWriter();
        serializer.Serialize(data, writer);

        string tostring = writer.ToString();        

        return JsonMapper.ToObject<BRCompResultsResponse>(tostring);
    }
}
