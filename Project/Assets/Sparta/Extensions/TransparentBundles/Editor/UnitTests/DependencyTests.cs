using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;

namespace SocialPoint.TransparentBundles
{
    [TestFixture]
    [Category("SocialPoint.TransparentBundles")]
    internal class DependencyTests
    {
        private const string _testAssetsFolder = "Assets/Sparta/Extensions/TransparentBundles/Editor/UnitTests/TestAssets/";
        private const string _prefab1 = "test_prefab_1.prefab";
        private const string _prefab2 = "test_prefab_2.prefab";
        private const string _texture1 = "texture_1.png";

        private BundlesManifest OldBundlesManifest;
        private BundlesManifest testManifest;

        [SetUp]
        public void SetUp()
        {
            OldBundlesManifest = DependencySystem.GetManifest();
            testManifest = new BundlesManifest();
            DependencySystem.SetManifest(testManifest);
        }

        [Test]
        public void AddSingleUserBundle()
        {
            var path = _testAssetsFolder + _prefab1;
            var guid = AssetDatabase.AssetPathToGUID(path);

            DependencySystem.RegisterManualBundledAsset(new DependencySystem.BundleInfo(guid));

            var dependencies = new List<string>(AssetDatabase.GetDependencies(path));
            dependencies.Remove(path);

            //Asset is added and bundled
            Assert.IsTrue(testManifest.HasAsset(guid), "Asset was not included in the manifest");
            var dependencyData = testManifest.GetBundleDependencyDataCopy(guid);
            Assert.IsTrue(dependencyData.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(dependencyData.BundleName), "Asset was not bundled");

            //Checks dependencies were added and not bundled since they are not shared
            foreach(var dependency in dependencies)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);

                Assert.IsTrue(testManifest.HasAsset(guidDep), "Asset was not included in the manifest");
                Assert.IsTrue(string.IsNullOrEmpty(testManifest.GetBundleDependencyDataCopy(guidDep).BundleName), "Asset was incorrectly autobundled");
            }
        }


        [Test]
        public void AddLocalBundle()
        {
            var path = _testAssetsFolder + _prefab1;
            var guid = AssetDatabase.AssetPathToGUID(path);

            DependencySystem.RegisterManualBundledAsset(new DependencySystem.BundleInfo(guid, true));

            var dependencies = new List<string>(AssetDatabase.GetDependencies(path));
            dependencies.Remove(path);

            //Asset is added and bundled
            Assert.IsTrue(testManifest.HasAsset(guid), "Asset was not included in the manifest");
            var bundleData = testManifest.GetBundleDependencyDataCopy(guid);
            Assert.IsTrue(bundleData.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(bundleData.BundleName), "Asset was not bundled");
            Assert.IsTrue(bundleData.IsLocal, "Asset was not marked as local");

            //Checks dependencies were added and not bundled since they are not shared
            foreach(var dependency in dependencies)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);

                var dependencyData = testManifest.GetBundleDependencyDataCopy(guidDep);

                Assert.IsTrue(testManifest.HasAsset(guidDep), "Asset was not included in the manifest");
                Assert.IsTrue(string.IsNullOrEmpty(dependencyData.BundleName), "Asset was incorrectly autobundled");
                Assert.IsTrue(dependencyData.IsLocal, "Asset was not marked as local");
            }
        }

        [Test]
        public void RemoveBundle()
        {
            var path = _testAssetsFolder + _prefab1;
            var guid = AssetDatabase.AssetPathToGUID(path);

            AddSingleUserBundle();

            DependencySystem.RemoveBundles(guid);

            var dependencies = new List<string>(AssetDatabase.GetDependencies(path));
            dependencies.Remove(path);

            //Check that the asset is no longer in the manifest
            Assert.IsFalse(testManifest.HasAsset(guid), "Asset is still included in the manifest when it should have been removed");

            //Checks dependencies were also removed since their parent is gone.
            foreach(var dependency in dependencies)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);

                Assert.IsFalse(testManifest.HasAsset(guidDep), dependency + " dependency is still included in the manifest when it should have been removed");
            }
        }

        [Test]
        public void AddTwoUserBundleWithShared()
        {
            var path1 = _testAssetsFolder + _prefab1;
            var path2 = _testAssetsFolder + _prefab2;

            var guid1 = AssetDatabase.AssetPathToGUID(path1);
            var guid2 = AssetDatabase.AssetPathToGUID(path2);

            var guidShared = AssetDatabase.AssetPathToGUID(_testAssetsFolder + _texture1);

            var userBundles = new List<string>();
            userBundles.Add(guid1);
            userBundles.Add(guid2);

            DependencySystem.UpdateManifest(userBundles);

            //Checks Prefab 1
            Assert.IsTrue(testManifest.HasAsset(guid1), "Asset was not included in the manifest");
            var dependencyData = testManifest.GetBundleDependencyDataCopy(guid1);
            Assert.IsTrue(dependencyData.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(dependencyData.BundleName), "Asset was not bundled");

            //Checks Prefab 2
            Assert.IsTrue(testManifest.HasAsset(guid2), "Asset was not included in the manifest");
            var dependencyData2 = testManifest.GetBundleDependencyDataCopy(guid2);
            Assert.IsTrue(dependencyData2.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(dependencyData2.BundleName), "Asset was not bundled");

            //Checks dependencies of Prefab 1 were added
            var dependencies1 = new List<string>(AssetDatabase.GetDependencies(path1));
            dependencies1.Remove(path1);
            foreach(var dependency in dependencies1)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);
                Assert.IsTrue(testManifest.HasAsset(guidDep), "Dependency was not included in the manifest");
            }

            //Checks dependencies of Prefab 2 were added
            var dependencies2 = new List<string>(AssetDatabase.GetDependencies(path2));
            dependencies2.Remove(path2);
            foreach(var dependency in dependencies2)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);
                Assert.IsTrue(testManifest.HasAsset(guidDep), "Dependency was not included in the manifest");
            }

            //Checks Shared Texture is auto-bundled
            Assert.IsFalse(string.IsNullOrEmpty(testManifest.GetBundleDependencyDataCopy(guidShared).BundleName), "Autobundle didn't behave as expected. The dependency should be bundled because it is shared.");
        }

        [Test]
        public void RemoveAutobundle()
        {

            var path1 = _testAssetsFolder + _prefab1;
            var path2 = _testAssetsFolder + _prefab2;

            var guid1 = AssetDatabase.AssetPathToGUID(path1);
            var guid2 = AssetDatabase.AssetPathToGUID(path2);
            var guidShared = AssetDatabase.AssetPathToGUID(_testAssetsFolder + _texture1);

            AddTwoUserBundleWithShared();

            DependencySystem.RemoveBundles(guid1);

            //Checks Prefab 1
            Assert.IsFalse(testManifest.HasAsset(guid1), "Asset is still included in the manifest");

            //Checks Prefab 2
            Assert.IsTrue(testManifest.HasAsset(guid2), "Asset was not included in the manifest");
            var bundleData = testManifest.GetBundleDependencyDataCopy(guid2);
            Assert.IsTrue(bundleData.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(bundleData.BundleName), "Asset was not bundled");

            //Checks Shared Texture is not auto-bundled
            Assert.IsTrue(string.IsNullOrEmpty(testManifest.GetBundleDependencyDataCopy(guidShared).BundleName), "Autobundle didn't behave as expected, the dependency shouldn't be bundled because it is not shared anymore.");
        }


        [TearDown]
        public void TearDown()
        {
            DependencySystem.SetManifest(OldBundlesManifest);
            DependencySystem.Save();
        }
    }
}
