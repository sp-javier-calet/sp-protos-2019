using NUnit.Framework;

namespace SocialPoint.Base
{
    [TestFixture]
    [Category("SocialPoint.Base")]
    public sealed class AppVersionTests
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
            Assert.That(new AppVersion(0, 0) == new AppVersion(0, 0, 0));
            Assert.That(AppVersion.Zero == "0.0");

            Assert.That(AppVersion.Zero == "0.0");
            Assert.That(AppVersion.Zero == "0.0.0");

            Assert.That(new AppVersion(10, 0) == new AppVersion(10, 0, 0));
            Assert.That(new AppVersion(10, 0) == "10.0.0");

            Assert.That(new AppVersion(10, 0, 0) == new AppVersion(10, 0));
            Assert.That(new AppVersion(10, 0, 0) == "10.0");

            Assert.That(AppVersion.Zero.ToString() == "0.0");
        }

        [Test]
        public void check_creating_versions_from_numbers()
        {
            AppVersion version;

            version = new AppVersion(2, 1);

            Assert.That(version.ToString() == "2.1");

            Assert.That(version.Major, Is.EqualTo(2));
            Assert.That(version.Minor, Is.EqualTo(1));
            Assert.That(version.Revision, Is.EqualTo(0));
            Assert.That(version.Build, Is.EqualTo(0));

            version = new AppVersion(1, 20, 5);
            Assert.That(version.ToString() == "1.20.5");

            Assert.That(version.Major, Is.EqualTo(1));
            Assert.That(version.Minor, Is.EqualTo(20));
            Assert.That(version.Revision, Is.EqualTo(5));
            Assert.That(version.Build, Is.EqualTo(0));


            version = new AppVersion(4, 234, 5120, 123);

            Assert.That(version.ToString(), Is.EqualTo("4.234.5120.123"));



            Assert.That(version.Major, Is.EqualTo(4));
            Assert.That(version.Minor, Is.EqualTo(234));
            Assert.That(version.Revision, Is.EqualTo(5120));
            Assert.That(version.Build, Is.EqualTo(123));
        }

        [Test]
        public void check_creating_versions_from_strings()
        {
            AppVersion version;

            version = new AppVersion("20.5.56.1234");

            Assert.That(version.ToString(), Is.EqualTo("20.5.56.1234"));

            Assert.That(version.Major, Is.EqualTo(20));
            Assert.That(version.Minor, Is.EqualTo(5));
            Assert.That(version.Revision, Is.EqualTo(56));
            Assert.That(version.Build, Is.EqualTo(1234));


            version = new AppVersion("1.2");
            Assert.That(version.ToString(), Is.EqualTo("1.2"));

            Assert.That(version.Major, Is.EqualTo(1));
            Assert.That(version.Minor, Is.EqualTo(2));
            Assert.That(version.Revision, Is.EqualTo(0));
            Assert.That(version.Build, Is.EqualTo(0));
        }

        [Test]
        public void check_parsing_new_version()
        {
            var version = new AppVersion(2, 6);

            Assert.That(version.ToString(), Is.EqualTo("2.6"));

            Assert.That(version.Major, Is.EqualTo(2));
            Assert.That(version.Minor, Is.EqualTo(6));
            Assert.That(version.Revision, Is.EqualTo(0));
            Assert.That(version.Build, Is.EqualTo(0));

            version.Parse("3.4.6");
            Assert.That(version.ToString(), Is.EqualTo("3.4.6"));

            Assert.That(version.Major, Is.EqualTo(3));
            Assert.That(version.Minor, Is.EqualTo(4));
            Assert.That(version.Revision, Is.EqualTo(6));
            Assert.That(version.Build, Is.EqualTo(0));
        }

        [Test]
        public void check_version_zero()
        {
            Assert.That(AppVersion.Zero == new AppVersion(0, 0));
            Assert.That(AppVersion.Zero == "0.0");
        }

        [Test]
        public void check_create_safe_version()
        {
            foreach (var versionStr in new [] {
                                        "343.33...3434",
                                        "1.2.3.4.5",
                                        "-1.2.3"
                                    })
            {
                var version = new AppVersion(versionStr);
                Assert.That(version, Is.EqualTo(AppVersion.Zero), versionStr);
                Assert.That(version.ToString(), Is.EqualTo(AppVersion.Zero.ToString()), versionStr);
            }

            Assert.That(new AppVersion(-1, 0), Is.EqualTo(AppVersion.Zero));
            Assert.That(new AppVersion(-1, 0).ToString(), Is.EqualTo(AppVersion.Zero.ToString()));
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
