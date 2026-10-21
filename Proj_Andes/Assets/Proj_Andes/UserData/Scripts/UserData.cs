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
    // and genero_cod (0 Masculino, 1 Femenino, 9 Otro, 99 unknown). Only write them through `gender`, which updates both.
    [FirestoreProperty] public string genero { get; set; }
    [FirestoreProperty] public int genero_cod { get; set; }

    public const int UnknownGenderCode = 99;

    // Profiles created before the gender rename stored "sexo_field" ("Hombre"/"Mujer"), "sexo_cod" (0/1, 99 unknown) and
    // an old numeric "sexo". They are only read (from Firestore through AdoptLegacyGender, from the local save through
    // Newtonsoft) and never written back: the first time the profile is normalized they are folded into genero/genero_cod.
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
            var fromCode = GenderFromCode(genero_cod);
            if (fromCode != UserGender.NONE) return fromCode;
            return LegacyGender();
        }
        set
        {
            genero = value == UserGender.NONE ? null : value.ToString();
            genero_cod = value == UserGender.NONE ? UnknownGenderCode : (int)value;
            sexo_field = null;
            sexo_cod = UnknownGenderCode;
            sexo_legacy = null;
        }
    }

    // The content of the game (texts, narrator and button audios) has a feminine variant and a default one.
    // Only "Femenino" gets the feminine one; "Otro" and any unknown value get the default.
    [Newtonsoft.Json.JsonIgnore]
    public bool UsesFeminineContent => gender == UserGender.Femenino;

    /// <summary>Makes genero and genero_cod agree with the resolved gender. Called whenever a profile is loaded or saved.</summary>
    public void NormalizeGender() => gender = gender;

    /// <summary>Reads the pre-rename fields of a Firestore document, which ConvertTo ignores.</summary>
    public void AdoptLegacyGender(DocumentSnapshot document)
    {
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
    [FirestoreProperty] public string tipo_colegio { get; set; }
    [FirestoreProperty] public int tipo_colegio_cod { get; set; }
    [FirestoreProperty] public string lugar_nacimiento { get; set; }
    [FirestoreProperty] public string vive { get; set; }

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
		tipo_colegio = UserSchoolType.NONE.ToString();
		tipo_colegio_cod = (int)UserSchoolType.NONE;
		lugar_nacimiento = string.Empty;
		vive = string.Empty;

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

public enum UserSchoolType
{
	NONE = 99,
    Privado = 0,
    PublicoUrbano = 1,
    PublicoRural = 2,
    Otro = 3,
}

[Flags]
public enum UserLivingWith
{
	NONE = 0,
	Papa = 1,
	Mama = 2,
	Hermanos = 4,
	Tios = 8,
	Abuelos = 16,
	Otros = 32,
}
public enum SessionStateLeft
{
	Inconcluso,
	Abandonado,
	Finalizado
}
