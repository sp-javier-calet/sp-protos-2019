using System.Text.RegularExpressions;
using UnityEngine;

public class BMUtility
{
    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    public static string InterpretPath(string origPath, BuildPlatform platform, string texfmt=null)
    {
        var matches = Regex.Matches(origPath, @"\$\((\w+)\)");
        foreach(Match match in matches)
        {
            string var = match.Groups[1].Value;
            string token = @"$(" + var + ")";
            if (var == "TextureFmt")
            {
                //Replace and if specifies a full directory try to remove slashes from path
                if (texfmt == null || platform != BuildPlatform.Android)
                {
                    var idx = origPath.IndexOf(token);
                    var last_idx = idx + token.Length - 1;
                    //Remove ending slashes?
                    if(idx == 0 && origPath.Length>=last_idx+2 && origPath[last_idx+1]=='/')
                    {
                        origPath = origPath.Replace(token + "/", "");
                    }
                    //Remove leading slashes?
                    else if(idx > 0 && origPath[idx-1]=='/' && (origPath.Length<last_idx+2 || origPath[last_idx+1]=='/'))
                    {
                        origPath = origPath.Replace("/" + token, "");
                    }
                    //Just replace the token
                    else
                    {
                        origPath = origPath.Replace(token, "");
                    }

                }
                else
                {
                    origPath = origPath.Replace(token, texfmt);
                }
            }
            else
            {
                origPath = origPath.Replace(token, EnvVarToString(var, platform));
            }
        }
        
        return origPath;
    }

    public static int[] long2doubleInt(long a)
    {
        int a1 = (int)(a & uint.MaxValue);
        int a2 = (int)(a >> 32);
        return new int[] { a1, a2 };
    }
    
    public static long doubleInt2long(int a1, int a2)
    {
        long b = a2;
        b = b << 32;
        b = b | (uint)a1;
        return b;
    }
    
    private static string EnvVarToString(string varString, BuildPlatform platform)
    {
        switch(varString)
        {
        case "DataPath":
            return Application.dataPath;
        case "PersistentDataPath":
            return Application.persistentDataPath;
        case "StreamingAssetsPath":
            return Application.streamingAssetsPath;
        case "Platform":
            return platform.ToString();
        default:
            Debug.LogError("Cannot solve enviroment var " + varString);
            return "";
        }
    }
}
