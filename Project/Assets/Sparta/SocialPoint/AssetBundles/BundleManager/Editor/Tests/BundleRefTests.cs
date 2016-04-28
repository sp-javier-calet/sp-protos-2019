using NUnit.Framework;
using UnityEditor;


namespace SocialPoint.BundleManagerTest
{
    [TestFixture]
    [Category("SocialPoint.BundleManager")]
    internal class BundleRefTests
    {
        BundleData bData;
        string guid, path;
        string[] dependencies;
        
        [SetUp]
        public void SetUp()
        {
            //Creates the bundle
            BundleManager.CreateNewBundle("NUnitTest", "", false);
            bData = BundleManager.GetBundleData("NUnitTest");

            //Asset to include in bundle
            guid = AssetDatabase.FindAssets("GUI_GameLoading t:Prefab")[0];
            path = AssetDatabase.GUIDToAssetPath(guid);

            //Collects dependencies and remove the main asset
            dependencies = AssetDatabase.GetDependencies(path);
            ArrayUtility.Remove<string>(ref dependencies, path);
        }

        [Test]
        public void AddIncludeAsset()
        {
            BundleManager.AddPathToBundle(path, bData.name);

            Assert.IsTrue(bData.includeGUIDs.Contains(guid), "The guid is not included in the bundle");
            Assert.IsTrue(bData.includs.Contains(path), "The path is not included in the bundle.");
        }

        [Test]
        public void RemoveIncludeAsset()
        {
            BundleManager.AddPathToBundle(path, bData.name);

            Assert.IsTrue(bData.includeGUIDs.Contains(guid), "The guid is not included in the bundle");
            Assert.IsTrue(bData.includs.Contains(path), "The path is not included in the bundle.");

            BundleManager.RemoveAssetFromBundle(guid, bData.name);

            Assert.IsFalse(bData.includeGUIDs.Contains(guid), "The guid is still included in the bundle");
            Assert.IsFalse(bData.includs.Contains(path), "The path is still included in the bundle.");
            
        }

        [Test]
        public void CanAddAssetToBundle()
        {
            Assert.IsTrue(BundleManager.CanAddPathToBundle(path, bData.name));

            BundleManager.AddPathToBundle(path, bData.name);
            Assert.IsTrue(bData.includeGUIDs.Contains(guid), "The guid is not included in the bundle");
            Assert.IsTrue(bData.includs.Contains(path), "The path is not included in the bundle.");
            
            Assert.IsFalse(BundleManager.CanAddPathToBundle(path, bData.name));
        }


        [Test]
        public void IncludeDependencies()
        {
            BundleManager.AddPathToBundle(path, bData.name);

            Assert.IsTrue(bData.includeGUIDs.Contains(guid), "The guid is not included in the bundle");
            Assert.IsTrue(bData.includs.Contains(path), "The path is not included in the bundle.");

            foreach (string str in dependencies)
                Assert.IsTrue(bData.dependAssets.Contains(str), "A dependency was not added to bundle depends " +str);

        }
        
        [TearDown]
        public void TearDown()
        {
            BundleManager.RemoveBundle("NUnitTest");
        }
    }
}
