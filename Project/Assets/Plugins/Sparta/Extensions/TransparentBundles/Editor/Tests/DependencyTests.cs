using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

namespace SocialPoint.TransparentBundles
{
    [TestFixture]
    [Category("SocialPoint.TransparentBundles")]
    class DependencyTests
    {
        readonly string _testAssetsFolder;
        const string _prefab1 = "test_prefab_1.prefab";
        const string _prefab2 = "test_prefab_2.prefab";
        const string _texture1 = "texture_1.png";

        BundlesManifest _oldBundlesManifest;
        BundlesManifest _testManifest;

        public DependencyTests()
        {
            const string type = "t:prefab";
            var guids = AssetDatabase.FindAssets(string.Format("{0} {1}", _prefab1.TrimEnd(".prefab".ToCharArray()), type));
            foreach(var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                _testAssetsFolder = path.TrimEnd(_prefab1.ToCharArray());
            }
        }

        [SetUp]
        public void SetUp()
        {
            _oldBundlesManifest = DependencySystem.Manifest;
            _testManifest = new BundlesManifest();
            DependencySystem.Manifest = _testManifest;
        }

        [Test]
        public void AddSingleUserBundle()
        {
            string path = _testAssetsFolder + _prefab1;
            var guid = AssetDatabase.AssetPathToGUID(path);

            DependencySystem.RegisterManualBundledAsset(new DependencySystem.BundleInfo(guid));

            var dependencies = new List<string>(AssetDatabase.GetDependencies(path));
            dependencies.Remove(path);

            //Asset is added and bundled
            Assert.IsTrue(_testManifest.HasAsset(guid), "Asset was not included in the manifest");
            var dependencyData = _testManifest.GetBundleDependencyDataCopy(guid);
            Assert.IsTrue(dependencyData.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(dependencyData.BundleName), "Asset was not bundled");

            //Checks dependencies were added and not bundled since they are not shared
            foreach(var dependency in dependencies)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);

                Assert.IsTrue(_testManifest.HasAsset(guidDep), "Asset was not included in the manifest");
                Assert.IsTrue(string.IsNullOrEmpty(_testManifest.GetBundleDependencyDataCopy(guidDep).BundleName), "Asset was incorrectly autobundled");
            }
        }


        [Test]
        public void AddLocalBundle()
        {
            string path = _testAssetsFolder + _prefab1;
            var guid = AssetDatabase.AssetPathToGUID(path);

            DependencySystem.RegisterManualBundledAsset(new DependencySystem.BundleInfo(guid, true));

            var dependencies = new List<string>(AssetDatabase.GetDependencies(path));
            dependencies.Remove(path);

            //Asset is added and bundled
            Assert.IsTrue(_testManifest.HasAsset(guid), "Asset was not included in the manifest");
            var bundleData = _testManifest.GetBundleDependencyDataCopy(guid);
            Assert.IsTrue(bundleData.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(bundleData.BundleName), "Asset was not bundled");
            Assert.IsTrue(bundleData.IsLocal, "Asset was not marked as local");

            //Checks dependencies were added and not bundled since they are not shared
            foreach(var dependency in dependencies)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);

                var dependencyData = _testManifest.GetBundleDependencyDataCopy(guidDep);

                Assert.IsTrue(_testManifest.HasAsset(guidDep), "Asset was not included in the manifest");
                Assert.IsTrue(string.IsNullOrEmpty(dependencyData.BundleName), "Asset was incorrectly autobundled");
                Assert.IsTrue(dependencyData.IsLocal, "Asset was not marked as local");
            }
        }

        [Test]
        public void RemoveBundle()
        {
            string path = _testAssetsFolder + _prefab1;
            var guid = AssetDatabase.AssetPathToGUID(path);

            AddSingleUserBundle();

            DependencySystem.RemoveBundles(guid);

            var dependencies = new List<string>(AssetDatabase.GetDependencies(path));
            dependencies.Remove(path);

            //Check that the asset is no longer in the manifest
            Assert.IsFalse(_testManifest.HasAsset(guid), "Asset is still included in the manifest when it should have been removed");

            //Checks dependencies were also removed since their parent is gone.
            foreach(var dependency in dependencies)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);

                Assert.IsFalse(_testManifest.HasAsset(guidDep), dependency + " dependency is still included in the manifest when it should have been removed");
            }
        }

        [Test]
        public void AddTwoUserBundleWithShared()
        {
            string path1 = _testAssetsFolder + _prefab1;
            string path2 = _testAssetsFolder + _prefab2;

            var guid1 = AssetDatabase.AssetPathToGUID(path1);
            var guid2 = AssetDatabase.AssetPathToGUID(path2);

            var guidShared = AssetDatabase.AssetPathToGUID(_testAssetsFolder + _texture1);

            var userBundles = new List<string>();
            userBundles.Add(guid1);
            userBundles.Add(guid2);

            DependencySystem.UpdateManifest(userBundles);

            //Checks Prefab 1
            Assert.IsTrue(_testManifest.HasAsset(guid1), "Asset was not included in the manifest");
            var dependencyData = _testManifest.GetBundleDependencyDataCopy(guid1);
            Assert.IsTrue(dependencyData.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(dependencyData.BundleName), "Asset was not bundled");

            //Checks Prefab 2
            Assert.IsTrue(_testManifest.HasAsset(guid2), "Asset was not included in the manifest");
            var dependencyData2 = _testManifest.GetBundleDependencyDataCopy(guid2);
            Assert.IsTrue(dependencyData2.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(dependencyData2.BundleName), "Asset was not bundled");

            //Checks dependencies of Prefab 1 were added
            var dependencies1 = new List<string>(AssetDatabase.GetDependencies(path1));
            dependencies1.Remove(path1);
            foreach(var dependency in dependencies1)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);
                Assert.IsTrue(_testManifest.HasAsset(guidDep), "Dependency was not included in the manifest");
            }

            //Checks dependencies of Prefab 2 were added
            var dependencies2 = new List<string>(AssetDatabase.GetDependencies(path2));
            dependencies2.Remove(path2);
            foreach(var dependency in dependencies2)
            {
                var guidDep = AssetDatabase.AssetPathToGUID(dependency);
                Assert.IsTrue(_testManifest.HasAsset(guidDep), "Dependency was not included in the manifest");
            }

            //Checks Shared Texture is auto-bundled
            Assert.IsFalse(string.IsNullOrEmpty(_testManifest.GetBundleDependencyDataCopy(guidShared).BundleName), "Autobundle didn't behave as expected. The dependency should be bundled because it is shared.");
        }

        [Test]
        public void RemoveAutobundle()
        {

            string path1 = _testAssetsFolder + _prefab1;
            string path2 = _testAssetsFolder + _prefab2;

            var guid1 = AssetDatabase.AssetPathToGUID(path1);
            var guid2 = AssetDatabase.AssetPathToGUID(path2);
            var guidShared = AssetDatabase.AssetPathToGUID(_testAssetsFolder + _texture1);

            AddTwoUserBundleWithShared();

            DependencySystem.RemoveBundles(guid1);

            //Checks Prefab 1
            Assert.IsFalse(_testManifest.HasAsset(guid1), "Asset is still included in the manifest");

            //Checks Prefab 2
            Assert.IsTrue(_testManifest.HasAsset(guid2), "Asset was not included in the manifest");
            var bundleData = _testManifest.GetBundleDependencyDataCopy(guid2);
            Assert.IsTrue(bundleData.IsExplicitlyBundled, "Asset was not marked as userbundled");
            Assert.IsFalse(string.IsNullOrEmpty(bundleData.BundleName), "Asset was not bundled");

            //Checks Shared Texture is not auto-bundled
            Assert.IsTrue(string.IsNullOrEmpty(_testManifest.GetBundleDependencyDataCopy(guidShared).BundleName), "Autobundle didn't behave as expected, the dependency shouldn't be bundled because it is not shared anymore.");
        }

        [TearDown]
        public void TearDown()
        {
            DependencySystem.Manifest = _oldBundlesManifest;
            DependencySystem.Save();
        }
    }
}
