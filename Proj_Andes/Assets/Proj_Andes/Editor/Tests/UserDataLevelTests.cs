using Newtonsoft.Json;
using NUnit.Framework;

// The school level of a profile is research data. nivel and nivel_cod must always agree, and profiles saved
// before the field existed must load as unknown (99) instead of failing.
public class UserDataLevelTests
{
    [Test]
    public void NewProfileHasNoLevelUntilFilled()
    {
        var user = new UserData();
        Assert.AreEqual(UserLevel.NONE, user.level);
        Assert.IsNull(user.nivel);
        Assert.AreEqual(UserData.UnknownLevelCode, user.nivel_cod);
    }

    [TestCase(UserLevel.Preescolar, "Preescolar", 1)]
    [TestCase(UserLevel.Primaria, "Primaria", 2)]
    [TestCase(UserLevel.Secundaria, "Secundaria", 3)]
    [TestCase(UserLevel.Superior, "Superior", 4)]
    [TestCase(UserLevel.Otro, "Otro", 9)]
    [TestCase(UserLevel.NONE, null, UserData.UnknownLevelCode)]
    public void SettingLevelWritesBothFields(UserLevel level, string expectedField, int expectedCode)
    {
        var user = new UserData();
        user.level = level;
        Assert.AreEqual(expectedField, user.nivel);
        Assert.AreEqual(expectedCode, user.nivel_cod);
    }

    [TestCase("{\"nivel\":\"Superior\",\"nivel_cod\":4}", UserLevel.Superior, TestName = "Current profile")]
    [TestCase("{\"nivel_cod\":3}", UserLevel.Secundaria, TestName = "Code only")]
    [TestCase("{\"nivel\":\"NONE\",\"nivel_cod\":2}", UserLevel.Primaria, TestName = "NONE text falls back to the code")]
    [TestCase("{\"nivel_cod\":7}", UserLevel.NONE, TestName = "A code that doesn't exist is unknown")]
    [TestCase("{}", UserLevel.NONE, TestName = "Profile from before the field existed")]
    public void ResolvesLevelFromTheBestAvailableField(string savedJson, UserLevel expected)
    {
        var user = JsonConvert.DeserializeObject<UserData>(savedJson);
        Assert.AreEqual(expected, user.level);
        user.NormalizeFields();
        Assert.AreEqual(expected, user.level, "normalizing must not change the resolved level");
        Assert.AreEqual((int)expected, user.nivel_cod);
    }

    [Test]
    public void LocalSaveRoundTripKeepsTheLevel()
    {
        var user = new UserData { level = UserLevel.Preescolar, gender = UserGender.Femenino };
        var loaded = JsonConvert.DeserializeObject<UserData>(JsonConvert.SerializeObject(user));
        Assert.AreEqual(UserLevel.Preescolar, loaded.level);
        Assert.AreEqual(1, loaded.nivel_cod);
        Assert.AreEqual(UserGender.Femenino, loaded.gender, "the level must not disturb the gender");
    }
}
