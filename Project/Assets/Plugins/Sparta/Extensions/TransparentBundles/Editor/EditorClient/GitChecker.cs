using UnityEngine;
using System.Collections.Generic;
using System.IO;
using SpartaTools.Editor.Utils;

namespace SocialPoint.TransparentBundles
{
    public static class GitChecker
    {
        const string _gitModifiedToken = " M";
        const string _gitUntrackedToken = "??";
        const string _gitDeletedToken = " D";

        public static List<string> CheckInBranchUpdated()
        {
            string branchName = TBConfig.GetConfig().branchName;
            var infractions = new List<string>();
            var repo = new Repository(Application.dataPath);

            var query = repo.CreateQuery("rev-parse").WithOption("abbrev-ref HEAD");

            var currentBranch = query.Exec().TrimEnd('\n');

            if(currentBranch != branchName)
            {
                infractions.Add(string.Format("The current branch is {0}. Was expecting {1}.", currentBranch, branchName));
                return infractions;
            }

            query = repo.CreateQuery("rev-parse").WithOption("abbrev-ref HEAD@{u}");

            var remote = query.Exec().TrimEnd('\n');
            var remoteBranch = remote.Substring(remote.IndexOf("/") + 1);

            if(remoteBranch != branchName)
            {
                infractions.Add(string.Format("The remote tracked branch is {0}. Was expecting {1}.", remote, branchName));
                return infractions;
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
            var paths = new List<string>(assetPaths);
            var infractions = new List<string>();
            var query = new Repository(Application.dataPath).CreateQuery("status").WithOption("porcelain");
            var gitStatus = query.Exec();
            var reader = new StringReader(gitStatus);

            string line = null;
            do
            {
                line = reader.ReadLine();
                if(line != null)
                {
                    if(line.StartsWith(_gitDeletedToken))
                    {
                        line = line.Remove(0, _gitDeletedToken.Length).Insert(0, "This file is deleted and pending to commit:");
                        infractions.Add(line);
                    }
                    else if(paths.Exists(x => line.Contains(x)))
                    {
                        if(line.StartsWith(_gitModifiedToken))
                        {
                            line = line.Remove(0, _gitModifiedToken.Length).Insert(0, "This file is modified and pending to commit:");
                        }
                        else if(line.StartsWith(_gitUntrackedToken))
                        {
                            line = line.Remove(0, _gitUntrackedToken.Length).Insert(0, "This file is new and pending to commit:");
                        }
                        infractions.Add(line);
                    }
                }
            } while(line != null);

            return infractions;
        }
    }
}
