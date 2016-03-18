using NUnit.Framework;

namespace SocialPoint.Base
{
    [TestFixture]
    [Category("SocialPoint.Base")]
    public class AppVersionTests
    {

        [Test]
        public void check_comparision_with_null()
        {
            AppVersion nilRef = null;

            Assert.That(nilRef == null, Is.True);
            Assert.That(null == nilRef, Is.True);

            Assert.That(nilRef != null, Is.False);
            Assert.That(null != nilRef, Is.False);
        }


        [Test]
        public void check_versions_without_revision_number_defaults_to_0()
        {
            Assert.That(new AppVersion(0,0) == new AppVersion(0,0,0));
            Assert.That(AppVersion.Zero == "0.0");

            Assert.That(AppVersion.Zero == "0.0");
            Assert.That(AppVersion.Zero == "0.0.0");

            Assert.That(new AppVersion(10,0) == new AppVersion(10,0,0));
            Assert.That(new AppVersion(10,0) == "10.0.0");

            Assert.That(new AppVersion(10,0,0) == new AppVersion(10,0));
            Assert.That(new AppVersion(10,0,0) == "10.0");
        }



        [Test]
        public void check_version_zero()
        {
            Assert.That(AppVersion.Zero == new AppVersion(0,0));
            Assert.That(AppVersion.Zero == "0.0");
        }

        [Test]
        public void check_create_safe_version()
        {
            Assert.That(new AppVersion("343.33...3434"), Is.EqualTo(AppVersion.Zero));
            Assert.That(new AppVersion("1.2.3.4.5"), Is.EqualTo(AppVersion.Zero));
            Assert.That(new AppVersion("-1.2.3"), Is.EqualTo(AppVersion.Zero));

            Assert.That(new AppVersion(-1,0), Is.EqualTo(AppVersion.Zero));
        }

        [Test]
        public void check_version_less_than_comparision()
        {
            var baseVersion = new AppVersion("10.10.10");

            Assert.That(baseVersion < new AppVersion("10.10.11"));
            Assert.That(baseVersion < new AppVersion("10.11.0"));
            Assert.That(baseVersion < new AppVersion("11.10"));

            Assert.That(baseVersion <= new AppVersion("10.10.10"));


            Assert.That(baseVersion < "10.10.11");
            Assert.That(baseVersion < "10.11.0");
            Assert.That(baseVersion < "11.10");

            Assert.That(baseVersion <= "10.10.10");
        }

        [Test]
        public void check_version_greater_than_comparision()
        {
            var baseVersion = new AppVersion("10.10.10");

            Assert.That(baseVersion > new AppVersion("10.10.9"));
            Assert.That(baseVersion > new AppVersion("10.9.0"));
            Assert.That(baseVersion > new AppVersion("9.9"));

            Assert.That(baseVersion >= new AppVersion("10.10.10"));


            Assert.That(baseVersion > "10.10.9");
            Assert.That(baseVersion > "10.9.0");
            Assert.That(baseVersion > "9.9");

            Assert.That(baseVersion >= "10.10.10");
        }

        [Test]
        public void check_version_equality_comparision()
        {
            var baseVersion = new AppVersion("10.10.10");

            Assert.That(baseVersion == new AppVersion("10.10.10"));
            Assert.That(baseVersion != new AppVersion("10.10"));

            Assert.That(baseVersion == "10.10.10");
            Assert.That(baseVersion != "10.10");
        }

    }

}