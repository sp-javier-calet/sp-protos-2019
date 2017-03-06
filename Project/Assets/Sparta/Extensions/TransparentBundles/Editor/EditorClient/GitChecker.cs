using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public static class GitChecker
    {

        public static bool CheckFilePending(ref List<string> infractions, params string[] assetPaths)
        {
            var query = new Repository(Application.dataPath).CreateQuery("status").WithArg("--porcelain");

            var gitStatus = query.Exec();

            bool anyInfraction = false;

            for(int i = 0; i < assetPaths.Length; i++)
            {
                var path = assetPaths[i];
                var idx = gitStatus.IndexOf(path);

                if(idx != -1)
                {
                    anyInfraction = true;
                    var shiftedIdx = 1;
                    var prevChar = gitStatus[idx - 1];

                    while(prevChar != '\n' && idx != 0)
                    {
                        shiftedIdx++;
                        idx--;
                        prevChar = gitStatus[idx - 1];
                    }

                    var line = gitStatus.Substring(idx, shiftedIdx + path.Length);

                    if(line.StartsWith("M"))
                    {
                        line = line.Remove(0).Insert(0, "This file is modified and pending to commit:");
                    }
                    else if(line.StartsWith("??"))
                    {
                        line = line.Remove(0, 2).Insert(0, "This file is new and pending to commit:");
                    }

                    infractions.Add(line);
                }
            }

            return anyInfraction;
        }
    }
}
