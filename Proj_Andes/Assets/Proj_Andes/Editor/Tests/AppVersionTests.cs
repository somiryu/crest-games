using System.Collections.Generic;
using NUnit.Framework;

// Every document the app uploads carries version_app (the Firestore rules reject it otherwise), and a build is only
// blocked when the minimum published in config/app is above it.
public class AppVersionTests
{
    const string TestCollection = "qa_app_version_test";

    [Test]
    public void IsAboveThePublishedApks() => Assert.Greater(AppVersion.Number, 1);

    [TestCase(-1, false)] //offline or not answered: keep working
    [TestCase(0, false)]
    [TestCase(AppVersion.Number, false)]
    [TestCase(AppVersion.Number + 1L, true)]
    public void BlocksOnlyBelowTheMinimum(long minVersion, bool outdated) => Assert.AreEqual(outdated, AppVersion.IsOutdated(minVersion));

    [Test]
    public void AnalyticsDocumentsCarryTheVersion()
    {
        try
        {
            UserDataManager.SaveUserAnayticsPerGame(TestCollection, new Dictionary<string, object> { ["campo"] = 1 });
            foreach (var doc in UserDataManager.userAnayticsPerGame[TestCollection].Values)
                Assert.AreEqual(AppVersion.Number, doc[DataIds.AppVersionField]);
        }
        finally { UserDataManager.userAnayticsPerGame.Remove(TestCollection); }
    }

    [Test]
    public void NormalizedProfilesCarryTheVersion()
    {
        var user = new UserData();
        Assert.AreEqual(0, user.version_app);
        user.NormalizeFields();
        Assert.AreEqual(AppVersion.Number, user.version_app);
    }
}
