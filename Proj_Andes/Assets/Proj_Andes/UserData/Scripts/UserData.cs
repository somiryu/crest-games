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
    // The sex is stored in two fields that must always agree: sexo_field ("Hombre"/"Mujer", read by the CMS)
    // and sexo_cod (0 Hombre, 1 Mujer, 99 unknown). Only write them through `sexo`, which updates both.
    // Profiles created before the shipped APK also have a numeric "sexo" (sexo_legacy), kept so it isn't erased.
    [FirestoreProperty] public string sexo_field { get; set; }
    [FirestoreProperty] public int sexo_cod { get; set; }
    //object because the Firebase Unity SDK doesn't support nullable value types (null when the field is missing)
    [FirestoreProperty("sexo")] public object sexo_legacy { get; set; }

    public const int UnknownSexCode = 99;

    [Newtonsoft.Json.JsonIgnore]
    public UserSex sexo
    {
        get
        {
            if (Enum.TryParse(sexo_field, out UserSex fromField) && fromField != UserSex.NONE) return fromField;
            //sexo_cod is written once when the profile is created; in prod it never contradicts a legacy 0,
            //and where it contradicts a legacy 1 the legacy value was the corrupted one
            var fromCode = SexFromCode(sexo_cod);
            if (fromCode != UserSex.NONE) return fromCode;
            return SexFromCode(sexo_legacy);
        }
        set
        {
            sexo_field = value == UserSex.NONE ? null : value.ToString();
            sexo_cod = value == UserSex.NONE ? UnknownSexCode : (int)value;
        }
    }

    /// <summary>Makes sexo_field and sexo_cod agree with the resolved sex. Called whenever a profile is loaded or saved.</summary>
    public void NormalizeSex() => sexo = sexo;

    static UserSex SexFromCode(object code)
    {
        if (code is IConvertible convertible && !(code is string))
        {
            var value = convertible.ToInt64(null);
            if (value == (int)UserSex.Hombre) return UserSex.Hombre;
            if (value == (int)UserSex.Mujer) return UserSex.Mujer;
        }
        return UserSex.NONE;
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
        sexo = UserSex.NONE;
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

public enum UserSex
{
	Hombre,
	Mujer,
	NONE,
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
