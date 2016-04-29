using NUnit.Framework;


namespace SocialPoint.BundleManagerTest
{
    [TestFixture]
    [Category("SocialPoint.BundleManager")]
    internal class BundleDefinitionTests
    {
        [Test]
        public void CreateBundle()
        {
            BundleManager.CreateNewBundle("NUnitTest", "", false);
            Assert.IsNotNull(BundleManager.GetBundleData("NUnitTest"), "The added bundle could not be found.");
        }

        [Test]
        public void CreateSceneBundle()
        {
            BundleManager.CreateNewBundle("NUnitTest", "", true);
            Assert.IsNotNull(BundleManager.GetBundleData("NUnitTest"), "The added bundle could not be found.");
            Assert.IsTrue(BundleManager.GetBundleData("NUnitTest").sceneBundle, "The added bundle is not a Scene.");
        }

        [Test]
        public void CreateNestedBundle()
        {
            BundleManager.CreateNewBundle("NUnitTestParent", "", false);
            Assert.IsNotNull(BundleManager.GetBundleData("NUnitTestParent"), "The added bundle could not be found.");

            BundleManager.CreateNewBundle("NUnitTestChild", "NUnitTestParent", false);
            Assert.IsNotNull(BundleManager.GetBundleData("NUnitTestChild"), "The added bundle could not be found.");
            Assert.IsTrue(BundleManager.GetBundleData("NUnitTestChild").parent == "NUnitTestParent", "The added bundle could not be found.");
        }

        [Test]
        public void CreateNestedSceneBundle()
        {
            BundleManager.CreateNewBundle("NUnitTestParent", "", true);
            Assert.IsNotNull(BundleManager.GetBundleData("NUnitTestParent"), "The added bundle could not be found.");
            Assert.IsTrue(BundleManager.GetBundleData("NUnitTestParent").sceneBundle, "The added bundle is not a Scene.");

            BundleManager.CreateNewBundle("NUnitTestChild", "NUnitTestParent", true);
            Assert.IsNotNull(BundleManager.GetBundleData("NUnitTestChild"), "The added bundle could not be found.");
            Assert.IsTrue(BundleManager.GetBundleData("NUnitTestChild").sceneBundle, "The added bundle is not a Scene.");
            Assert.IsTrue(BundleManager.GetBundleData("NUnitTestChild").parent == "NUnitTestParent", "The added bundle could not be found.");
        }

        [Test]
        public void RemoveBundle()
        {
            BundleManager.CreateNewBundle("NUnitTest", "", false);
            Assert.IsNotNull(BundleManager.GetBundleData("NUnitTest"), "The added bundle could not be found.");

            BundleManager.RemoveBundle("NUnitTest");
            Assert.IsNull(BundleManager.GetBundleData("NUnitTest"), "The bundle has not been removed");
        }


        [TearDown]
        public void TearDown()
        {
            BundleManager.RemoveBundle("NUnitTest");
            BundleManager.RemoveBundle("NUnitTestChild");
            BundleManager.RemoveBundle("NUnitTestParent");
        }
    }
}
