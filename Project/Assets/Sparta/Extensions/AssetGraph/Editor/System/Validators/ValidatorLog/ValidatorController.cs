using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using SocialPoint.AWS.S3;
using UnityEngine;
using System.Globalization;
using SpartaTools.Editor.Utils;

namespace AssetBundleGraph
{
    public class ValidatorController
    {
        const string _S3URL = "https://s3.amazonaws.com/sp-tools/";

        public static ValidatorLog GetLastValidatorLog()
        {
            ValidatorLog resValidator;
            var localValidator = ValidatorLog.LoadFromDisk();

            var files = ListFromS3();

            if(files.Count > 0)
            {
                var lastFile = files[0];
                var date = GetS3ValidationDate(lastFile.Key);

                if(localValidator.executedPlatforms.Count == 0 || localValidator.lastExecuted < date)
                {
                    resValidator = ValidatorLog.LoadFromText(DownloadFromS3(lastFile.Key), lastFile.Key);
                }
                else
                {
                    resValidator = localValidator;
                }
            }
            else
            {
                resValidator = localValidator;
            }

            return resValidator;
        }


        public static DateTime GetS3ValidationDate(string name)
        {
            var filename = Path.GetFileNameWithoutExtension(name);
            var datestring = filename.Substring(filename.IndexOf("_T") + 2);
            var date = DateTime.ParseExact(datestring, "yyyy_MM_dd_HH_mm_ss", CultureInfo.InvariantCulture);
            DateTime.SpecifyKind(date, DateTimeKind.Utc);

            return date;
        }

        public static List<S3ListObject> ListFromS3()
        {
            List<S3ListObject> res = new List<S3ListObject>();
            try
            {
                var connection = new S3Connection("AKIAJVQTABXWU2SVVPBQ", "5+u5ZJS92KRodbJECS96CAHGx/mGbn/tTKbJNEMX");

                var bucket = connection.GetBucket("sp-tools");

                var files = bucket.ListFiles("AssetGraph/" + GetProjectId() + "/" + GetBranch() + "/");

                res = files.FindAll(x => !x.IsFolder);

                //sort by name descending
                res.Sort((x, y) => y.Key.CompareTo(x.Key));
            }
            catch(Exception e)
            {
                Debug.LogWarning(e);
            }

            return res;
        }


        public static string DownloadFromS3(string filepathUrl)
        {
            var url = _S3URL + filepathUrl;

            RemoteCertificateValidationCallback SSLDelegate = (x, y, z, w) => true;

            ServicePointManager.ServerCertificateValidationCallback += SSLDelegate;
            WebRequest req = HttpWebRequest.Create(url);

            var response = req.GetResponse();

            string fileContent = string.Empty;
            using(var reader = new StreamReader(response.GetResponseStream()))
            {
                fileContent = reader.ReadToEnd();
            }
            response.Close();

            ServicePointManager.ServerCertificateValidationCallback -= SSLDelegate;

            return fileContent;
        }


        static string GetProjectId()
        {
            var projectID = AssetGraphCIConfig.GetConfig().ProjectID;

            if(string.IsNullOrEmpty(projectID))
            {
                throw new Exception("Project Id is not initialized. This is required to retrieve S3 Validations. Configure it in " + AssetGraphCIConfig.ConfigDefaultPath);
            }

            return projectID;
        }

        static string GetBranch()
        {
            var repo = new Repository(Application.dataPath);
            var query = repo.CreateQuery("rev-parse").WithOption("abbrev-ref HEAD");
            var currentBranch = query.Exec().TrimEnd('\n');

            return currentBranch;
        }

    }
}
