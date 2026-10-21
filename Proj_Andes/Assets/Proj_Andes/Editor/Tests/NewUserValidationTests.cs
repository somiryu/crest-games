using NUnit.Framework;

// Only the student's pin is required to create a profile (the project code is asked for in its own panel).
public class NewUserValidationTests
{
    [Test]
    public void AValidPinIsEnough()
    {
        Assert.AreEqual("", FirebaseAnonymousLoginUI.GetNewUserError("10123", false));
    }

    [TestCase("")]
    [TestCase(null)]
    public void ThePinCannotBeEmpty(string pin)
    {
        Assert.AreNotEqual("", FirebaseAnonymousLoginUI.GetNewUserError(pin, false));
    }

    [Test]
    public void ThePinCannotRepeatInTheSameInstitution()
    {
        Assert.AreNotEqual("", FirebaseAnonymousLoginUI.GetNewUserError("10123", true));
    }

    [Test]
    public void AProfileWithNothingButThePinKeepsEverythingElseUnknown()
    {
        var user = new UserData { pin = "10123" };
        Assert.AreEqual(UserGender.NONE, user.gender);
        Assert.AreEqual(UserLevel.NONE, user.level);
        Assert.AreEqual(UserSector.NONE, user.schoolSector);
        Assert.AreEqual(99, user.edad);
        Assert.AreEqual(99, user.gen_cod);
        Assert.AreEqual(99, user.nivel_cod);
        Assert.AreEqual(99, user.sector_cod);
    }
}

// Age and grade are open numeric answers and the birthplace can't carry commas (they break the CSV downloads).
public class OpenAnswersTests
{
    [TestCase("8", 8)]
    [TestCase(" 12 ", 12)]
    [TestCase("0", 0)]
    [TestCase("25", 25)]
    [TestCase("", 99)]
    [TestCase(null, 99)]
    [TestCase("ocho", 99)]
    [TestCase("-3", 99)]
    [TestCase("7,5", 99)]
    public void ParsesWhatWasTyped(string typed, int expected)
    {
        Assert.AreEqual(expected, FirebaseAnonymousLoginUI.ParseOpenNumber(typed));
    }

    [TestCase("Bogotá, Cundinamarca", "Bogotá- Cundinamarca")]
    [TestCase("Cali,Valle,Colombia", "Cali-Valle-Colombia")]
    [TestCase("Medellín", "Medellín")]
    [TestCase("  Pasto  ", "Pasto")]
    [TestCase("", "99")]
    [TestCase("   ", "99")]
    [TestCase(null, "99")]
    public void CleansTheBirthplace(string typed, string expected)
    {
        Assert.AreEqual(expected, FirebaseAnonymousLoginUI.CleanBirthplace(typed));
    }
}
