using UnityEngine;
using System.Collections;
using System.IO;
using SocialPoint.IO;
using LitJson;

public class SPAMConfigJson {
    [System.NonSerialized]
    static SPAMConfigJson
               _instance;
    public static SPAMConfigJson Instance
    {
        get
        {
            if (_instance == null)
            {
                var filePath = Path.Combine(Application.dataPath, "SpamConfig/spamConfigJson.json");

                var content = FileUtils.ReadAllText(filePath);

                _instance = JsonMapper.ToObject<SPAMConfigJson>(content);
            }
            return _instance;
        }
    }

    public bool autoSplitEnabled = true;
    public int maxBundlesPerRequest = 200;
    public int pastCompilationsShown = 50;

    public static SPAMConfigJson RefreshInstance()
    {
        _instance = null;

        return Instance;
    }

    public void Save()
    {
        var filePath = Path.Combine(Application.dataPath, "SpamConfig/spamConfigJson.json");

        var json = JsonMapper.ToJson(this);

        FileUtils.WriteAllText(filePath,json);
    }
}
