using Newtonsoft.Json;
using NUnit.Framework;

// The sex of a profile is research data. These guard the rules that keep sexo_field and sexo_cod in sync.
public class UserDataSexTests
{
    static UserData Profile(string field, int code, object legacy = null)
    {
        var user = new UserData();
        user.sexo_field = field;
        user.sexo_cod = code;
        user.sexo_legacy = legacy;
        return user;
    }

    [Test]
    public void NewProfileHasNoSexUntilFilled()
    {
        var user = new UserData();
        Assert.AreEqual(UserSex.NONE, user.sexo);
        Assert.IsNull(user.sexo_field);
        Assert.AreEqual(UserData.UnknownSexCode, user.sexo_cod);
    }

    [TestCase(UserSex.Hombre, "Hombre", 0)]
    [TestCase(UserSex.Mujer, "Mujer", 1)]
    [TestCase(UserSex.NONE, null, UserData.UnknownSexCode)]
    public void SettingSexWritesBothFields(UserSex sex, string expectedField, int expectedCode)
    {
        var user = new UserData();
        user.sexo = sex;
        Assert.AreEqual(expectedField, user.sexo_field);
        Assert.AreEqual(expectedCode, user.sexo_cod);
    }

    [TestCase("Hombre", 0, null, UserSex.Hombre, TestName = "Current profile")]
    [TestCase(null, 0, 0L, UserSex.Hombre, TestName = "Legacy man")]
    [TestCase(null, 1, 1L, UserSex.Mujer, TestName = "Legacy woman")]
    [TestCase(null, 0, 1L, UserSex.Hombre, TestName = "Legacy conflict trusts sexo_cod")]
    [TestCase(null, UserData.UnknownSexCode, 0L, UserSex.Hombre, TestName = "Only legacy value")]
    [TestCase(null, UserData.UnknownSexCode, null, UserSex.NONE, TestName = "No data stays unknown")]
    [TestCase("NONE", 1, null, UserSex.Mujer, TestName = "NONE text falls back to code")]
    public void ResolvesSexFromTheBestAvailableField(string field, int code, object legacy, UserSex expected)
    {
        var user = Profile(field, code, legacy);
        Assert.AreEqual(expected, user.sexo);
        user.NormalizeSex();
        Assert.AreEqual(expected, user.sexo, "normalizing must not change the resolved sex");
        Assert.AreEqual(expected == UserSex.NONE ? UserData.UnknownSexCode : (int)expected, user.sexo_cod);
    }

    [Test]
    public void LocalSaveRoundTripKeepsTheSex()
    {
        var saved = JsonConvert.SerializeObject(Profile(null, 0, 0L));
        var loaded = JsonConvert.DeserializeObject<UserData>(saved);
        loaded.NormalizeSex();
        Assert.AreEqual(UserSex.Hombre, loaded.sexo);
        Assert.AreEqual("Hombre", loaded.sexo_field);
        Assert.IsFalse(saved.Contains("\"sexo\":"), "the computed property must not be serialized");
    }
}
