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
    [FirestoreProperty] public string sexo_field { get; set; }
    //Profiles created before the shipped APK stored the sex as a number in "sexo" (0 Hombre, 1 Mujer).
    //Mapped so it can be read as a fallback and is not erased when the document is saved again.
    //object because the Firebase Unity SDK doesn't support nullable value types (null when the field is missing)
    [FirestoreProperty("sexo")] public object sexo_legacy { get; set; }
    //Stored as a string in the database (sexo_field), so that the value is readable in the CMS
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public UserSex sexo
    {
        get
        {
            if (Enum.TryParse(sexo_field, out UserSex value)) return value;
            if (sexo_legacy is IConvertible legacyCode && !(sexo_legacy is string))
            {
                var code = legacyCode.ToInt64(null);
                if (code == (int)UserSex.Hombre) return UserSex.Hombre;
                if (code == (int)UserSex.Mujer) return UserSex.Mujer;
            }
            return UserSex.NONE;
        }
        set => sexo_field = value.ToString();
    }
    [FirestoreProperty] public int sexo_cod { get; set; }
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
        sexo = UserSex.Mujer;
		sexo_field = sexo.ToString();
		sexo_cod = 1;
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
