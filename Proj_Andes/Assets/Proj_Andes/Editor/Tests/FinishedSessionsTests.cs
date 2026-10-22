using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Only finished sessions are uploaded: whatever an abandoned one leaves must be discarded, and the exit word of the
// home button must be typed exactly (ignoring case and spaces around it).
public class FinishedSessionsTests
{
    const string Finished = "session-finished";
    const string Abandoned = "session-abandoned";

    static Dictionary<string, object> Doc(string sessionID, params (string key, object value)[] extra)
    {
        var doc = new Dictionary<string, object>();
        if (sessionID != null) doc[DataIds.TestID] = sessionID;
        foreach (var (key, value) in extra) doc[key] = value;
        return doc;
    }

    static Dictionary<string, Dictionary<string, Dictionary<string, object>>> Sessions() =>
        new Dictionary<string, Dictionary<string, Dictionary<string, object>>>
        {
            [DataIds.test] = new Dictionary<string, Dictionary<string, object>>
            {
                [Finished] = Doc(Finished, (DataIds.state, "Finalizado")),
                [Abandoned] = Doc(Abandoned, (DataIds.state, "Abandonado")),
            },
            [DataIds.heartsAndStarsGame] = new Dictionary<string, Dictionary<string, object>>
            {
                ["h1"] = Doc(Finished), ["h2"] = Doc(Finished), ["h3"] = Doc(Abandoned),
            },
            [DataIds.sizeRocketsGame] = new Dictionary<string, Dictionary<string, object>>
            {
                ["c1"] = Doc(Abandoned), ["c2"] = Doc(Abandoned),
            },
            ["without-session"] = new Dictionary<string, Dictionary<string, object>>
            {
                ["x1"] = Doc(null),
            },
        };

    [Test]
    public void OnlyTheSessionsMarkedFinalizadoAreFinished()
    {
        var finished = DatabaseManager.GetFinishedSessionIds(Sessions());
        CollectionAssert.AreEquivalent(new[] { Finished }, finished);
    }

    [Test]
    public void NothingIsFinishedWithoutItsSessionDocument()
    {
        var onlyGames = Sessions();
        onlyGames.Remove(DataIds.test);
        Assert.IsEmpty(DatabaseManager.GetFinishedSessionIds(onlyGames));
        Assert.IsEmpty(DatabaseManager.GetFinishedSessionIds(null));
    }

    [Test]
    public void TheUploadHasOnlyTheDocumentsOfFinishedSessions()
    {
        var upload = DatabaseManager.FinishedSessionsOnly(Sessions());

        CollectionAssert.AreEquivalent(new[] { Finished }, upload[DataIds.test].Keys);
        CollectionAssert.AreEquivalent(new[] { "h1", "h2" }, upload[DataIds.heartsAndStarsGame].Keys);
        Assert.IsFalse(upload.ContainsKey(DataIds.sizeRocketsGame), "a collection with only abandoned documents leaves no empty entry");
        Assert.IsTrue(upload.ContainsKey("without-session"), "documents without a session id are not touched");
    }

    [Test]
    public void BuildingTheUploadDoesNotChangeTheQueue()
    {
        var queue = Sessions();
        DatabaseManager.FinishedSessionsOnly(queue);
        Assert.AreEqual(2, queue[DataIds.test].Count);
        Assert.AreEqual(2, queue[DataIds.sizeRocketsGame].Count);
    }

    [Test]
    public void DiscardingUnfinishedSessionsRemovesThemInPlace()
    {
        var queue = Sessions();
        var removed = DatabaseManager.RemoveUnfinishedSessions(queue);

        Assert.AreEqual(1 + 1 + 2, removed, "the abandoned prueba, one hearts document and both rockets documents");
        CollectionAssert.AreEquivalent(new[] { "h1", "h2" }, queue[DataIds.heartsAndStarsGame].Keys);
        Assert.IsFalse(queue.ContainsKey(DataIds.sizeRocketsGame));
    }

    [Test]
    public void AbandoningASessionRemovesOnlyItsDocuments()
    {
        var queue = Sessions();
        var removed = DatabaseManager.RemoveSessionDocs(queue, Abandoned);

        Assert.AreEqual(4, removed);
        CollectionAssert.AreEquivalent(new[] { Finished }, queue[DataIds.test].Keys, "the finished session stays");
        CollectionAssert.AreEquivalent(new[] { "h1", "h2" }, queue[DataIds.heartsAndStarsGame].Keys);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("another-session")]
    public void AbandoningWithoutASessionRemovesNothing(string sessionID)
    {
        var queue = Sessions();
        Assert.AreEqual(0, DatabaseManager.RemoveSessionDocs(queue, sessionID));
        Assert.AreEqual(2, queue[DataIds.test].Count);
    }

    [Test]
    public void AProfileStaysLocalUntilItsSessionIsFinished()
    {
        const string id = "profile-for-the-test";
        try
        {
            DatabaseManager.MarkProfileAsLocalOnly(id);
            StringAssert.Contains(id, PlayerPrefs.GetString(DatabaseManager.localOnlyUsersJSONKey));

            DatabaseManager.ForgetLocalOnlyProfile(id);
            StringAssert.DoesNotContain(id, PlayerPrefs.GetString(DatabaseManager.localOnlyUsersJSONKey));
        }
        finally
        {
            DatabaseManager.ForgetLocalOnlyProfile(id);
            PlayerPrefs.DeleteKey(DatabaseManager.localOnlyUsersJSONKey);
        }
    }

    [TestCase("salir", true)]
    [TestCase("Salir", true)]
    [TestCase("SALIR", true)]
    [TestCase("  salir  ", true)]
    [TestCase("sali", false)]
    [TestCase("salirr", false)]
    [TestCase("sal ir", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void OnlyTheExitWordConfirmsLeaving(string typed, bool expected)
    {
        Assert.AreEqual(expected, GameUIController.IsExitWord(typed));
    }
}
