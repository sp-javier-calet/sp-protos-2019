using System;
using NSubstitute;
using NUnit.Framework;

namespace SocialPoint.Utils
{
    [TestFixture]
    [Category("SocialPoint.Utils")]
    public class UnityUpdaterTests
    {


        [Test]
        public void RemoveBeforeAdd()
        {
            var scheduler = new UpdateScheduler();
            var updateable = Substitute.For<IUpdateable>();
            scheduler.Add(updateable);
            scheduler.Remove(updateable);
            scheduler.Remove(updateable);
            scheduler.Add(updateable);

            scheduler.Update(0.1f);

            updateable.Received().Update();
        }

    }
}
