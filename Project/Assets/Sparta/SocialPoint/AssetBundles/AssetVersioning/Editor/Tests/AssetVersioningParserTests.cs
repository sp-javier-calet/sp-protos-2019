using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.IO;
using SocialPoint.Attributes;
using SocialPointEditor.Assets.PlatformEx;

namespace SocialPoint.AssetVersioning
{
    [TestFixture]
    [Category("SocialPoint.AssetVersioning")]
    internal class AssetVersioningParserTests
    {

        string assetVersioningFullPath;

        [SetUp]
        public void SetUp()
        {
            var guid = AssetDatabase.FindAssets("asset_versioning")[0];
            assetVersioningFullPath = Path.Combine(Application.dataPath.Replace("/Assets", ""), AssetDatabase.GUIDToAssetPath(guid)).ToSysPath();
        }

        [Test]
        public void ParseAssetVersioning()
        {    
            JsonStreamReader jsonReader;    
            using(var f = new StreamReader(assetVersioningFullPath))
            {
                jsonReader = new JsonStreamReader(f.ReadToEnd());
            }
            jsonReader.Read();

            var assetVersioningDict = new AssetVersioningDictionary();
            AssetVersioningParser.Parse(assetVersioningDict, jsonReader, null);

            Assert.IsTrue(assetVersioningDict.ContainsKey("HAB_Fire_01_EVO01"));
            Assert.IsTrue(assetVersioningDict.ContainsKey("prp_egg_fire_01"));
            Assert.IsTrue(assetVersioningDict.ContainsKey("prp_egg_icecube_01"));

            AssetVersioningData habFire = assetVersioningDict["HAB_Fire_01_EVO01"];
            Assert.AreEqual(habFire.Client, "1.0");
            Assert.AreEqual(habFire.Version, 3);
            Assert.IsTrue(string.IsNullOrEmpty(habFire.Parent));

            AssetVersioningData prpEggFire = assetVersioningDict["prp_egg_fire_01"];
            Assert.AreEqual(prpEggFire.Client, "1.0");
            Assert.AreEqual(prpEggFire.Version, 3);
            Assert.AreEqual(prpEggFire.Parent, "prp_egg");

            AssetVersioningData prpEggIce = assetVersioningDict["prp_egg_icecube_01"];
            Assert.AreEqual(prpEggIce.Client, "1.0");
            Assert.AreEqual(prpEggIce.Version, 3);
            Assert.AreEqual(prpEggIce.Parent, "prp_egg");
        }
    }
}
