using Newtonsoft.Json;
using NUnit.Framework;

// The gender of a profile is research data. These guard the rules that keep genero and gen_cod in sync,
// and that profiles saved before the sexo -> genero rename still load.
public class UserDataGenderTests
{
    static UserData FromSavedJson(string json) => JsonConvert.DeserializeObject<UserData>(json);

    [Test]
    public void NewProfileHasNoGenderUntilFilled()
    {
        var user = new UserData();
        Assert.AreEqual(UserGender.NONE, user.gender);
        Assert.IsNull(user.genero);
        Assert.AreEqual(UserData.UnknownGenderCode, user.gen_cod);
    }

    [TestCase(UserGender.Masculino, "Masculino", 0)]
    [TestCase(UserGender.Femenino, "Femenino", 1)]
    [TestCase(UserGender.Otro, "Otro", 9)]
    [TestCase(UserGender.NONE, null, UserData.UnknownGenderCode)]
    public void SettingGenderWritesBothFields(UserGender gender, string expectedField, int expectedCode)
    {
        var user = new UserData();
        user.gender = gender;
        Assert.AreEqual(expectedField, user.genero);
        Assert.AreEqual(expectedCode, user.gen_cod);
    }

    [TestCase("{\"genero\":\"Masculino\",\"gen_cod\":0}", UserGender.Masculino, TestName = "Current man")]
    [TestCase("{\"genero\":\"Femenino\",\"gen_cod\":1}", UserGender.Femenino, TestName = "Current woman")]
    [TestCase("{\"genero\":\"Otro\",\"gen_cod\":9}", UserGender.Otro, TestName = "Current other")]
    [TestCase("{\"gen_cod\":9}", UserGender.Otro, TestName = "Code only")]
    [TestCase("{\"genero\":\"NONE\",\"gen_cod\":1}", UserGender.Femenino, TestName = "NONE text falls back to the code")]
    [TestCase("{\"genero\":\"Femenino\",\"gen_cod\":1,\"sexo_field\":\"Hombre\"}", UserGender.Femenino, TestName = "New fields win over legacy")]
    [TestCase("{\"sexo_field\":\"Hombre\",\"sexo_cod\":0}", UserGender.Masculino, TestName = "Legacy man")]
    [TestCase("{\"sexo_field\":\"Mujer\",\"sexo_cod\":1}", UserGender.Femenino, TestName = "Legacy woman")]
    [TestCase("{\"sexo_cod\":1}", UserGender.Femenino, TestName = "Legacy code only")]
    [TestCase("{\"sexo_cod\":99,\"sexo_legacy\":0}", UserGender.Masculino, TestName = "Only the oldest numeric value")]
    [TestCase("{\"sexo_cod\":0,\"sexo_legacy\":1}", UserGender.Masculino, TestName = "Legacy conflict trusts sexo_cod")]
    [TestCase("{\"sexo_cod\":9}", UserGender.NONE, TestName = "9 never meant anything in the legacy code")]
    [TestCase("{}", UserGender.NONE, TestName = "No data stays unknown")]
    public void ResolvesGenderFromTheBestAvailableField(string savedJson, UserGender expected)
    {
        var user = FromSavedJson(savedJson);
        Assert.AreEqual(expected, user.gender);
        user.NormalizeGender();
        Assert.AreEqual(expected, user.gender, "normalizing must not change the resolved gender");
        Assert.AreEqual(expected == UserGender.NONE ? UserData.UnknownGenderCode : (int)expected, user.gen_cod);
    }

    [Test]
    public void SavingNeverWritesTheOldNames()
    {
        var user = FromSavedJson("{\"sexo_field\":\"Hombre\",\"sexo_cod\":0,\"sexo_legacy\":0}");
        user.NormalizeGender();
        var saved = JsonConvert.SerializeObject(user);
        Assert.IsFalse(saved.Contains("sexo"), saved);
        Assert.IsTrue(saved.Contains("\"genero\":\"Masculino\""), saved);
        Assert.IsTrue(saved.Contains("\"gen_cod\":0"), saved);
    }

    [Test]
    public void LocalSaveRoundTripKeepsTheGender()
    {
        var user = new UserData { gender = UserGender.Otro };
        var loaded = FromSavedJson(JsonConvert.SerializeObject(user));
        Assert.AreEqual(UserGender.Otro, loaded.gender);
        Assert.AreEqual("Otro", loaded.genero);
        Assert.AreEqual(9, loaded.gen_cod);
    }

    [TestCase(UserGender.Masculino, false)]
    [TestCase(UserGender.Femenino, true)]
    [TestCase(UserGender.Otro, false)]
    [TestCase(UserGender.NONE, false)]
    public void OnlyFemeninoGetsTheFeminineContent(UserGender gender, bool expected)
    {
        Assert.AreEqual(expected, new UserData { gender = gender }.UsesFeminineContent);
    }
}
