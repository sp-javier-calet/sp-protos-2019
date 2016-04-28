using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NSubstitute;
using System.IO;

namespace SocialPoint.BundleManagerTest
{
    [TestFixture]
    [Category("SocialPoint.BundleManagerSlow")]
    public class BuildAssetBundleTests
    {

        bool previousUseEditorTarget;
        string filepath, filepathChild;
        BundleData parent, child;

        [SetUp]
        public void SetUp()
        {
            BundleManager.RefreshAll();

            //Creates the bundle
            BundleManager.CreateNewBundle("NUnitTest", "", false);
            parent = BundleManager.GetBundleData("NUnitTest");
            BundleManager.CreateNewBundle("NUnitTestChild", "NUnitTest", false);
            child = BundleManager.GetBundleData("NUnitTestChild");
            //Assets to include in bundle
            var guid = AssetDatabase.FindAssets("TestPrefab t:Prefab")[0];
            var path = AssetDatabase.GUIDToAssetPath(guid);
            BundleManager.AddPathToBundle(path, parent.name);
            var guidChild = AssetDatabase.FindAssets("TestPrefab 1 t:Prefab")[0];
            var pathChild = AssetDatabase.GUIDToAssetPath(guidChild);
            BundleManager.AddPathToBundle(pathChild, child.name);

            filepath = Path.Combine(BuildConfiger.InterpretedOutputPath, "NUnitTest." + BuildConfiger.BundleSuffix);
            filepathChild = Path.Combine(BuildConfiger.InterpretedOutputPath, "NUnitTestChild." + BuildConfiger.BundleSuffix);

            previousUseEditorTarget = BuildConfiger.UseEditorTarget;
            BuildConfiger.UseEditorTarget = true;
        }

        [Test]
        public void CreateAssetBundle()
        {
            BuildHelper.BuildBundles(new string[] { "NUnitTest" });
            Assert.IsTrue(File.Exists(filepath), "File not found in " + filepath);
        }

        [Test]
        public void SkipAssetBundle()
        {
            BuildHelper.BuildBundles(new string[] { "NUnitTest" });
            Assert.IsTrue(File.Exists(filepath), "File not found in " + filepath);

            Assert.IsFalse(BuildHelper.IsBundleNeedBuild(parent), "Bundle still needs rebuild");
        }

        [Test]
        public void RebuildPolicyChildren()
        {
            BuildHelper.BuildBundles(new string[] { "NUnitTest", "NUnitTestChild" });

            Assert.IsTrue(File.Exists(filepath), "Parent file not found in " + filepath);
            Assert.IsTrue(File.Exists(filepathChild), "Child file not found in " + filepath);

            Assert.IsFalse(BuildHelper.IsBundleNeedBuild(parent), "Parent bundle still needs rebuild");
            Assert.IsFalse(BuildHelper.IsBundleNeedBuild(child), "Child bundle still needs rebuild");

            var guidChild = AssetDatabase.FindAssets("TestPrefab 2 t:Prefab")[0];
            var pathChild = AssetDatabase.GUIDToAssetPath(guidChild);
            BundleManager.AddPathToBundle(pathChild, child.name);

            Assert.IsFalse(BuildHelper.IsBundleNeedBuild(parent), "Parent bundle still needs rebuild");
            Assert.IsTrue(BuildHelper.IsBundleNeedBuild(child), "Child bundle doesn't need rebuild");
        }

        [Test]
        public void RebuildPolicyParents()
        {
            BuildHelper.BuildBundles(new string[] { "NUnitTest", "NUnitTestChild" });

            Assert.IsTrue(File.Exists(filepath), "Parent file not found in " + filepath);
            Assert.IsTrue(File.Exists(filepathChild), "Child file not found in " + filepath);

            Assert.IsFalse(BuildHelper.IsBundleNeedBuild(parent), "Parent bundle still needs rebuild");
            Assert.IsFalse(BuildHelper.IsBundleNeedBuild(child), "Child bundle still needs rebuild");

            var guidChild = AssetDatabase.FindAssets("TestPrefab 2 t:Prefab")[0];
            var pathChild = AssetDatabase.GUIDToAssetPath(guidChild);
            BundleManager.AddPathToBundle(pathChild, parent.name);

            Assert.IsTrue(BuildHelper.IsBundleNeedBuild(parent), "Parent bundle doesn't need rebuild");
            Assert.IsTrue(BuildHelper.IsBundleNeedBuild(child), "Child bundle doesn't need rebuild");
        }

        [TearDown]
        public void TearDown()
        {
            BundleManager.RemoveBundle("NUnitTest");
            BundleManager.RemoveBundle("NUnitTestChild");
            File.Delete(filepath);
            File.Delete(filepath + ".meta");
            File.Delete(filepathChild);
            File.Delete(filepathChild + ".meta");
            BuildConfiger.UseEditorTarget = previousUseEditorTarget;
        }
    }
}
