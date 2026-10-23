using Newtonsoft.Json;
using NUnit.Framework;

// The school sector is research data. sector and sector_cod must always agree, the codes are the ones of the
// database model (0 privado urbano, 1 publico urbano, 2 publico rural, 3 privado rural, stored lowercase without
// accents), and profiles saved as tipo_colegio / tipo_colegio_cod or with the old capitalized labels keep their meaning.
public class UserDataSectorTests
{
    static UserData FromSavedJson(string json) => JsonConvert.DeserializeObject<UserData>(json);

    [Test]
    public void NewProfileHasNoSectorUntilFilled()
    {
        var user = new UserData();
        Assert.AreEqual(UserSector.NONE, user.schoolSector);
        Assert.IsNull(user.sector);
        Assert.AreEqual(UserData.UnknownSectorCode, user.sector_cod);
    }

    [TestCase(UserSector.PrivadoUrbano, "privado urbano", 0)]
    [TestCase(UserSector.PublicoUrbano, "publico urbano", 1)]
    [TestCase(UserSector.PublicoRural, "publico rural", 2)]
    [TestCase(UserSector.PrivadoRural, "privado rural", 3)]
    [TestCase(UserSector.NONE, null, UserData.UnknownSectorCode)]
    public void SettingSectorWritesBothFields(UserSector sector, string expectedField, int expectedCode)
    {
        var user = new UserData();
        user.schoolSector = sector;
        Assert.AreEqual(expectedField, user.sector);
        Assert.AreEqual(expectedCode, user.sector_cod);
    }

    // The dropdown options are in a different order than the codes, so the code must come from the text.
    [TestCase("Privado urbano", UserSector.PrivadoUrbano)]
    [TestCase("Privado rural", UserSector.PrivadoRural)]
    [TestCase("Público urbano", UserSector.PublicoUrbano)]
    [TestCase("Público rural", UserSector.PublicoRural)]
    [TestCase("público RURAL", UserSector.PublicoRural)]
    [TestCase("publico urbano", UserSector.PublicoUrbano)]
    [TestCase("Sector", UserSector.NONE)]
    [TestCase("", UserSector.NONE)]
    [TestCase(null, UserSector.NONE)]
    public void ParsesTheDropdownText(string text, UserSector expected)
    {
        Assert.AreEqual(expected, UserData.ParseSector(text));
    }

    [TestCase("{\"sector\":\"privado rural\",\"sector_cod\":3}", UserSector.PrivadoRural, TestName = "Current profile")]
    [TestCase("{\"sector\":\"Público urbano\",\"sector_cod\":1}", UserSector.PublicoUrbano, TestName = "Accented label saved before lowercase")]
    [TestCase("{\"sector_cod\":3}", UserSector.PrivadoRural, TestName = "Code only")]
    [TestCase("{\"sector\":\"NONE\",\"sector_cod\":1}", UserSector.PublicoUrbano, TestName = "Unknown text falls back to the code")]
    [TestCase("{\"tipo_colegio\":\"Privado\",\"tipo_colegio_cod\":0}", UserSector.PrivadoUrbano, TestName = "Legacy private school keeps code 0")]
    [TestCase("{\"tipo_colegio\":\"Público Urbano\",\"tipo_colegio_cod\":1}", UserSector.PublicoUrbano, TestName = "Legacy public urban")]
    [TestCase("{\"tipo_colegio\":\"Público Rural\",\"tipo_colegio_cod\":2}", UserSector.PublicoRural, TestName = "Legacy public rural")]
    [TestCase("{\"tipo_colegio_cod\":2}", UserSector.PublicoRural, TestName = "Legacy code only")]
    [TestCase("{\"tipo_colegio_cod\":3}", UserSector.NONE, TestName = "Legacy Otro was never offered")]
    [TestCase("{\"tipo_colegio_cod\":99}", UserSector.NONE, TestName = "Legacy unknown")]
    [TestCase("{\"sector\":\"Público rural\",\"sector_cod\":2,\"tipo_colegio\":\"Privado\"}", UserSector.PublicoRural, TestName = "New fields win over legacy")]
    [TestCase("{}", UserSector.NONE, TestName = "No data stays unknown")]
    public void ResolvesSectorFromTheBestAvailableField(string savedJson, UserSector expected)
    {
        var user = FromSavedJson(savedJson);
        Assert.AreEqual(expected, user.schoolSector);
        user.NormalizeFields();
        Assert.AreEqual(expected, user.schoolSector, "normalizing must not change the resolved sector");
        Assert.AreEqual((int)expected, user.sector_cod);
    }

    [Test]
    public void SavingNeverWritesTheOldNames()
    {
        var user = FromSavedJson("{\"tipo_colegio\":\"Público Rural\",\"tipo_colegio_cod\":2}");
        user.NormalizeFields();
        var saved = JsonConvert.SerializeObject(user);
        Assert.IsFalse(saved.Contains("tipo_colegio"), saved);
        Assert.IsTrue(saved.Contains("\"sector\":\"publico rural\""), saved);
        Assert.IsTrue(saved.Contains("\"sector_cod\":2"), saved);
    }

    [Test]
    public void LocalSaveRoundTripKeepsTheSector()
    {
        var user = new UserData { schoolSector = UserSector.PrivadoRural, gender = UserGender.Otro, level = UserLevel.Superior };
        var loaded = FromSavedJson(JsonConvert.SerializeObject(user));
        Assert.AreEqual(UserSector.PrivadoRural, loaded.schoolSector);
        Assert.AreEqual(UserGender.Otro, loaded.gender);
        Assert.AreEqual(UserLevel.Superior, loaded.level);
    }
}
