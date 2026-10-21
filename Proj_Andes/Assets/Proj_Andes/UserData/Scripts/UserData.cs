using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[FirestoreData]
public class UserData 
{
    [FirestoreProperty] public string id_jugador { get; set; }
    [FirestoreProperty] public string pin { get; set; }
    [FirestoreProperty] public string id_proyecto { get; set; }
    [FirestoreProperty] public int edad { get; set; }
    [FirestoreProperty] public int grado { get; set; }
    // The gender is stored in two fields that must always agree: genero ("Masculino"/"Femenino"/"Otro", read by the CMS)
    // and gen_cod (0 Masculino, 1 Femenino, 9 Otro, 99 unknown). Only write them through `gender`, which updates both.
    [FirestoreProperty] public string genero { get; set; }
    [FirestoreProperty] public int gen_cod { get; set; }

    public const int UnknownGenderCode = 99;

    // Profiles created before the gender rename stored "sexo_field" ("Hombre"/"Mujer"), "sexo_cod" (0/1, 99 unknown) and
    // an old numeric "sexo". They are only read (from Firestore through AdoptLegacyGender, from the local save through
    // Newtonsoft) and never written back: the first time the profile is normalized they are folded into genero/gen_cod.
    [Newtonsoft.Json.JsonProperty] string sexo_field { get; set; }
    [Newtonsoft.Json.JsonProperty] int sexo_cod { get; set; } = UnknownGenderCode;
    [Newtonsoft.Json.JsonProperty] object sexo_legacy { get; set; }
    public bool ShouldSerializesexo_field() => false;
    public bool ShouldSerializesexo_cod() => false;
    public bool ShouldSerializesexo_legacy() => false;

    [Newtonsoft.Json.JsonIgnore]
    public UserGender gender
    {
        get
        {
            if (Enum.TryParse(genero, true, out UserGender fromField) && fromField != UserGender.NONE) return fromField;
            var fromCode = GenderFromCode(gen_cod);
            if (fromCode != UserGender.NONE) return fromCode;
            return LegacyGender();
        }
        set
        {
            genero = value == UserGender.NONE ? null : value.ToString();
            gen_cod = value == UserGender.NONE ? UnknownGenderCode : (int)value;
            sexo_field = null;
            sexo_cod = UnknownGenderCode;
            sexo_legacy = null;
        }
    }

    // The content of the game (texts, narrator and button audios) has a feminine variant and a default one.
    // Only "Femenino" gets the feminine one; "Otro" and any unknown value get the default.
    [Newtonsoft.Json.JsonIgnore]
    public bool UsesFeminineContent => gender == UserGender.Femenino;

    /// <summary>Makes genero and gen_cod agree with the resolved gender. Called whenever a profile is loaded or saved.</summary>
    public void NormalizeGender() => gender = gender;

    // The school level is stored like the gender: nivel ("Preescolar" ... "Otro") and nivel_cod (1, 2, 3, 4, 9; 99 unknown).
    // Profiles created before the field existed come without it and stay unknown (99) until they are filled.
    [FirestoreProperty] public string nivel { get; set; }
    [FirestoreProperty] public int nivel_cod { get; set; }

    public const int UnknownLevelCode = 99;

    [Newtonsoft.Json.JsonIgnore]
    public UserLevel level
    {
        get
        {
            if (Enum.TryParse(nivel, true, out UserLevel fromField) && fromField != UserLevel.NONE) return fromField;
            return Enum.IsDefined(typeof(UserLevel), nivel_cod) ? (UserLevel)nivel_cod : UserLevel.NONE;
        }
        set
        {
            nivel = value == UserLevel.NONE ? null : value.ToString();
            nivel_cod = (int)value;
        }
    }

    /// <summary>Makes nivel and nivel_cod agree with the resolved level. Called whenever a profile is loaded or saved.</summary>
    public void NormalizeLevel() => level = level;

    /// <summary>Normalizes every field that is stored twice (text and code).</summary>
    public void NormalizeFields()
    {
        NormalizeGender();
        NormalizeLevel();
        NormalizeSector();
    }

    /// <summary>Reads the pre-rename fields of a Firestore document (sexo*, tipo_colegio*), which ConvertTo ignores.</summary>
    public void AdoptLegacyFields(DocumentSnapshot document)
    {
        if (document.TryGetValue<string>("tipo_colegio", out var legacySchool)) tipo_colegio = legacySchool;
        if (document.TryGetValue<object>("tipo_colegio_cod", out var legacySchoolCode) && legacySchoolCode != null) tipo_colegio_cod = (int)Convert.ToInt64(legacySchoolCode);
        if (document.TryGetValue<string>("sexo_field", out var legacyField)) sexo_field = legacyField;
        if (document.TryGetValue<object>("sexo_cod", out var legacyCode) && legacyCode != null) sexo_cod = (int)Convert.ToInt64(legacyCode);
        if (document.TryGetValue<object>("sexo", out var legacyOld)) sexo_legacy = legacyOld;
    }

    // sexo_cod is written once when the profile is created; in prod it never contradicts a legacy 0,
    // and where it contradicts a legacy 1 the legacy value was the corrupted one
    UserGender LegacyGender()
    {
        if (sexo_field == "Hombre") return UserGender.Masculino;
        if (sexo_field == "Mujer") return UserGender.Femenino;
        var fromCode = LegacyFromCode(sexo_cod);
        if (fromCode != UserGender.NONE) return fromCode;
        return LegacyFromCode(sexo_legacy);
    }

    static UserGender GenderFromCode(object code)
    {
        if (code is IConvertible convertible && !(code is string))
        {
            var value = convertible.ToInt64(null);
            if (value == (int)UserGender.Masculino) return UserGender.Masculino;
            if (value == (int)UserGender.Femenino) return UserGender.Femenino;
            if (value == (int)UserGender.Otro) return UserGender.Otro;
        }
        return UserGender.NONE;
    }

    //The old codes were 0 Hombre and 1 Mujer; 9 never existed there
    static UserGender LegacyFromCode(object code)
    {
        var gender = GenderFromCode(code);
        return gender == UserGender.Otro ? UserGender.NONE : gender;
    }
    // The school sector is stored like the gender: sector ("Privado urbano" ...) and sector_cod (0 Privado urbano,
    // 1 Público urbano, 2 Público rural, 3 Privado rural; 99 unknown). It used to be tipo_colegio / tipo_colegio_cod
    // with only "Privado" (0), "Público Urbano" (1) and "Público Rural" (2); those are read but never written back.
    [FirestoreProperty] public string sector { get; set; }
    [FirestoreProperty] public int sector_cod { get; set; }

    public const int UnknownSectorCode = 99;

    [Newtonsoft.Json.JsonProperty] string tipo_colegio { get; set; }
    [Newtonsoft.Json.JsonProperty] int tipo_colegio_cod { get; set; } = UnknownSectorCode;
    public bool ShouldSerializetipo_colegio() => false;
    public bool ShouldSerializetipo_colegio_cod() => false;

    [Newtonsoft.Json.JsonIgnore]
    public UserSector schoolSector
    {
        get
        {
            var fromField = ParseSector(sector);
            if (fromField != UserSector.NONE) return fromField;
            if (IsSectorCode(sector_cod)) return (UserSector)sector_cod;
            return LegacySector();
        }
        set
        {
            sector = value == UserSector.NONE ? null : SectorLabel(value);
            sector_cod = (int)value;
            tipo_colegio = null;
            tipo_colegio_cod = UnknownSectorCode;
        }
    }

    /// <summary>Makes sector and sector_cod agree with the resolved sector. Called whenever a profile is loaded or saved.</summary>
    public void NormalizeSector() => schoolSector = schoolSector;

    public static string SectorLabel(UserSector sector)
    {
        switch (sector)
        {
            case UserSector.PrivadoUrbano: return "Privado urbano";
            case UserSector.PrivadoRural: return "Privado rural";
            case UserSector.PublicoUrbano: return "Público urbano";
            case UserSector.PublicoRural: return "Público rural";
            default: return null;
        }
    }

    /// <summary>The sector whose label is this text (ignoring case), or NONE (the dropdown placeholder, empty text...).</summary>
    public static UserSector ParseSector(string label)
    {
        if (string.IsNullOrEmpty(label)) return UserSector.NONE;
        foreach (UserSector candidate in new[] { UserSector.PrivadoUrbano, UserSector.PrivadoRural, UserSector.PublicoUrbano, UserSector.PublicoRural })
            if (string.Equals(SectorLabel(candidate), label.Trim(), StringComparison.OrdinalIgnoreCase)) return candidate;
        return UserSector.NONE;
    }

    static bool IsSectorCode(int code) => code == (int)UserSector.PrivadoUrbano || code == (int)UserSector.PublicoUrbano
        || code == (int)UserSector.PublicoRural || code == (int)UserSector.PrivadoRural;

    // Old profiles never said whether a private school was urban or rural: they keep code 0, which is now
    // "Privado urbano". The old code 3 ("Otro") was never offered in the dropdown, so it means nothing.
    UserSector LegacySector()
    {
        if (string.Equals(tipo_colegio?.Trim(), "Privado", StringComparison.OrdinalIgnoreCase)) return UserSector.PrivadoUrbano;
        var fromLabel = ParseSector(tipo_colegio);
        if (fromLabel != UserSector.NONE) return fromLabel;
        return tipo_colegio_cod >= 0 && tipo_colegio_cod <= 2 ? (UserSector)tipo_colegio_cod : UserSector.NONE;
    }
    [FirestoreProperty] public string lugar_nacimiento { get; set; }

    [FirestoreProperty] public Dictionary<string, bool> tutorialStepsDone { get; set; } = new Dictionary<string, bool>();
	   
	//This refers to the Game group index
    [FirestoreProperty] public int CheckPointIdx { get; set; } = -1;
    //This refers to the item idx inside of the game group index
    [FirestoreProperty] public int CheckPointSubIdx { get; set; } = -1;
    [FirestoreProperty] public List<NarrativeNavigationNode> narrativeNavCheckPointsNodes { get; set; }
    [FirestoreProperty] public List<int> itemsPlayedIdxs { get; set; } = new List<int>();
    [FirestoreProperty] public int Coins { get; set; }
    //This are the IDXs of the monsters
    [FirestoreProperty] public List<string> myCollectionMonsters { get; set; } = new List<string>();

    public UserData()
	{
		Debug.Log("Creating new User Data");
		id_jugador = Guid.NewGuid().ToString();
		pin = "Unnamed";
		id_proyecto = "Empty";
		edad = 99;
        grado = 99;
        //Unknown until the profile is filled, a default value would hide missing data (it used to make boys load as Mujer)
        gender = UserGender.NONE;
        level = UserLevel.NONE;
		schoolSector = UserSector.NONE;
		lugar_nacimiento = string.Empty;

        CheckPointIdx = -1;
		CheckPointSubIdx = -1;
		narrativeNavCheckPointsNodes = new List<NarrativeNavigationNode>();
		itemsPlayedIdxs = new List<int>();
		Coins = 10;
        myCollectionMonsters = new List<string>();
	}

	public bool IsTutorialStepDone(tutorialSteps step)
	{
		if (!tutorialStepsDone.ContainsKey(step.ToString())) return false;
		else return tutorialStepsDone[step.ToString()];
	}

	public void RegisterTutorialStepDone(string id)
	{
		if(tutorialStepsDone.ContainsKey(id)) tutorialStepsDone[id] = true;
		else tutorialStepsDone.Add(id, true);
	}
}

public enum UserGender
{
	Masculino = 0,
	Femenino = 1,
	Otro = 9,
	NONE = 99,
}

public enum UserLevel
{
	Preescolar = 1,
	Primaria = 2,
	Secundaria = 3,
	Superior = 4,
	Otro = 9,
	NONE = 99,
}

public enum UserSector
{
	PrivadoUrbano = 0,
	PublicoUrbano = 1,
	PublicoRural = 2,
	PrivadoRural = 3,
	NONE = 99,
}

public enum SessionStateLeft
{
	Inconcluso,
	Abandonado,
	Finalizado
}
