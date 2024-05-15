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
    [FirestoreProperty] public UserSex sexo { get; set; }
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
		id_jugador = Guid.NewGuid().ToString();
		pin = "Unnamed";
		id_proyecto = "Empty";
		edad = -1;
        grado = -1;
        sexo = UserSex.Mujer;
		sexo_cod = 1;
		tipo_colegio = UserSchoolType.NONE.ToString();
		tipo_colegio_cod = -1;
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
	NONE = -1,
    Privado = 0,
    PublicoUrbano = 1,
    PublicoRural = 2,
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
	Abandoned,
	Finished
}
