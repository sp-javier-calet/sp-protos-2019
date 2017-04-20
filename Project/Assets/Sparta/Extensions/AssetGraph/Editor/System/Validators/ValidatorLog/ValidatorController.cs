using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using SocialPoint.AWS.S3;
using UnityEditor;
using UnityEngine;
using System.Globalization;

namespace AssetBundleGraph
{
    public class ValidatorController
    {
        const string S3URL = "https://s3.amazonaws.com/sp-tools/";

        public static ValidatorLog GetLastValidatorLog()
        {
            var localValidator = ValidatorLog.LoadFromDisk();

            var files = ListFromS3();

            if(files.Count > 0)
            {
                var lastFile = files[0];
                var date = GetS3ValidationDate(lastFile.Key);

                if(localValidator.executedPlatforms.Count == 0 || localValidator.lastExecuted < date)
                {
                    return ValidatorLog.LoadFromText(DownloadFromS3(lastFile.Key), lastFile.Key);
                }
                else
                {
                    return localValidator;
                }
            }
            else
            {
                return localValidator;
            }
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
            var connection = new S3Connection("AKIAJVQTABXWU2SVVPBQ", "5+u5ZJS92KRodbJECS96CAHGx/mGbn/tTKbJNEMX");

            var bucket = connection.GetBucket("sp-tools");

            var files = bucket.ListFiles("AssetGraph/" + GetProjectId() + "/" + GetBranch() + "/");

            var nonFolderFiles = files.FindAll(x => !x.IsFolder);

            //sort by name descending
            nonFolderFiles.Sort((x, y) => y.Key.CompareTo(x.Key));

            return nonFolderFiles;
        }


        public static string DownloadFromS3(string filepathUrl)
        {
            var url = S3URL + filepathUrl;

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


        static int GetProjectId()
        {
            return 37;
        }

        static string GetBranch()
        {
            //TODO Git get current branch
            return "test_branch";
        }

    }
}
