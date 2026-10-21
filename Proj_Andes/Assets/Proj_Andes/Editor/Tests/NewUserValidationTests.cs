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
        Assert.AreEqual(99, user.genero_cod);
        Assert.AreEqual(99, user.nivel_cod);
        Assert.AreEqual(99, user.sector_cod);
    }
}
