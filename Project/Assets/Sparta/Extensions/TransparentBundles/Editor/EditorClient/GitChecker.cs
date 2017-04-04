using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public static class GitChecker
    {
        const string _gitModifiedToken = " M";
        const string _gitUntrackedToken = "??";

        public static List<string> CheckInBranchUpdated(string branchName)
        {
            var infractions = new List<string>();
            var repo = new Repository(Application.dataPath);

            var query = repo.CreateQuery("rev-parse").WithOption("abbrev-ref HEAD");

            var currentBranch = query.Exec().TrimEnd('\n');

            if(currentBranch != branchName)
            {
                infractions.Add(string.Format("The current branch is {0}. Was expecting {1}.", currentBranch, branchName));
            }

            var queryLocal = repo.CreateQuery("log").WithArg("-1").WithOption("oneline").WithArg(currentBranch);
            var queryRemote = repo.CreateQuery("log").WithArg("-1").WithOption("oneline").WithArg(currentBranch + "@{u}");

            var lastLocalCommit = queryLocal.Exec().TrimEnd('\n');
            var lastFetchedRemoteCommit = queryRemote.Exec().TrimEnd('\n');
            if(lastLocalCommit != lastFetchedRemoteCommit)
            {
                infractions.Add(string.Format("The current commit is {0}. The remote commit is {1}. Please update your changes with pull/push.", lastLocalCommit, lastFetchedRemoteCommit));
            }

            return infractions;
        }

        public static List<string> CheckFilePending(params string[] assetPaths)
        {
            var infractions = new List<string>();
            var query = new Repository(Application.dataPath).CreateQuery("status").WithOption("porcelain");

            var gitStatus = query.Exec();

            for(int i = 0; i < assetPaths.Length; i++)
            {
                var path = assetPaths[i];
                var idx = gitStatus.IndexOf(path);

                if(idx != -1)
                {
                    var prevIdx = idx - 1;
                    while(prevIdx > 0 && gitStatus[prevIdx - 1] != '\n')
                    {
                        prevIdx--;
                    }

                    var nextIdx = idx + path.Length;
                    while(nextIdx < gitStatus.Length && gitStatus[nextIdx] != '\n')
                    {
                        nextIdx++;
                    }

                    var line = gitStatus.Substring(prevIdx, nextIdx - prevIdx);

                    if(line.StartsWith(_gitModifiedToken))
                    {
                        line = line.Remove(0, 2).Insert(0, "This file is modified and pending to commit:");
                    }
                    else if(line.StartsWith(_gitUntrackedToken))
                    {
                        line = line.Remove(0, 2).Insert(0, "This file is new and pending to commit:");
                    }

                    infractions.Add(line);
                }
            }

            return infractions;
        }
    }
}
