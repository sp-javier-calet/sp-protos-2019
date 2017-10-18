using NSubstitute;
using NUnit.Framework;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Base;

namespace SocialPoint.Locale
{
    [TestFixture]
    [Category("SocialPoint.Locale")]
    public sealed class LocalizationTests
    {
        IHttpClient HttpClient;
        LocalizationManager LocalizationManager;

        [SetUp]
        public void SetUp()
        {
            PathsManager.Init();

            HttpClient = Substitute.For<IHttpClient>();
            var DeviceInfo = Substitute.For<UnityDeviceInfo>();
            var environments = Substitute.For<IBackendEnvironment>();
            environments.Environments.Returns(new []{ new Environment { Name = "Production", Url = string.Empty, Type = EnvironmentType.Production}});

            LocalizationManager = new LocalizationManager(null);
            LocalizationManager.HttpClient = HttpClient;
            LocalizationManager.AppInfo = DeviceInfo.AppInfo;
            LocalizationManager.BackendEnvironments = environments;
            LocalizationManager.Location.ProjectId = "ds";
            LocalizationManager.Location.EnvironmentsData.Add(new LocaleInstaller.EnvironmentData{ EnvironmentType = EnvironmentType.Production, Id = "prod", SecretKey = "4HKu9W2Wv4Ooolrt"});
            LocalizationManager.SupportedLanguages = LocalizationManager.DefaultSupportedLanguages;
        }

        [TearDown]
        public void TearDown()
        {
            LocalizationManager.Dispose();
        }

        [Test]
        public void ValidatePortuguese()
        {
            LocalizationManager.CurrentLanguage = Localization.BrasilianIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.BrasilianIdentifier);

            LocalizationManager.CurrentLanguage = Localization.PortugueseIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.BrasilianIdentifier);
        }

        [Test]
        public void ValidateGerman()
        {
            LocalizationManager.CurrentLanguage = Localization.GermanIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.GermanIdentifier);
        }

        [Test]
        public void ValidateEnglish()
        {
            LocalizationManager.CurrentLanguage = Localization.EnglishIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.EnglishIdentifier);
        }

        [Test]
        public void ValidateSpanish()
        {
            LocalizationManager.CurrentLanguage = Localization.SpanishIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.SpanishIdentifier);

            LocalizationManager.CurrentLanguage = Localization.BasqueIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.SpanishIdentifier);

            LocalizationManager.CurrentLanguage = Localization.CatalanIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.SpanishIdentifier);

            LocalizationManager.CurrentLanguage = Localization.GalicianIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.SpanishIdentifier);
        }

        [Test]
        public void ValidateFrench()
        {
            LocalizationManager.CurrentLanguage = Localization.FrenchIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.FrenchIdentifier);
        }

        [Test]
        public void ValidateItalian()
        {
            LocalizationManager.CurrentLanguage = Localization.ItalianIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.ItalianIdentifier);
        }

        [Test]
        public void ValidateJapanese()
        {
            LocalizationManager.CurrentLanguage = Localization.JapaneseIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.JapaneseIdentifier);
        }

        [Test]
        public void ValidateKorean()
        {
            LocalizationManager.CurrentLanguage = Localization.KoreanIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.KoreanIdentifier);
        }

        [Test]
        public void ValidateRussian()
        {
            LocalizationManager.CurrentLanguage = Localization.RussianIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.RussianIdentifier);
        }

        [Test]
        public void ValidateTurkish()
        {
            LocalizationManager.CurrentLanguage = Localization.TurkishIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, Localization.TurkishIdentifier);
        }

        [Test]
        public void ValidateSimplifiedChinese()
        {
            LocalizationManager.CurrentLanguage = Localization.SimplifiedChineseIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, LocalizationManager.SimplifiedChineseServerIdentifier);

            LocalizationManager.CurrentLanguage = Localization.SimplifiedChineseIdentifierCountry;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, LocalizationManager.SimplifiedChineseServerIdentifier);
        }

        [Test]
        public void ValidateTraditionalChinese()
        {
            LocalizationManager.CurrentLanguage = Localization.TraditionalChineseIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, LocalizationManager.TraditionalChineseServerIdentifier);

            LocalizationManager.CurrentLanguage = Localization.TraditionalChineseIdentifierCountry;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, LocalizationManager.TraditionalChineseServerIdentifier);

            LocalizationManager.CurrentLanguage = Localization.TraditionalHongKongChineseIdentifier;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, LocalizationManager.TraditionalChineseServerIdentifier);

            LocalizationManager.CurrentLanguage = Localization.TraditionalHongKongChineseIdentifierCountry;
            Assert.AreEqual(LocalizationManager.CurrentLanguage, LocalizationManager.TraditionalChineseServerIdentifier);
        }
    }
}