using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[FirestoreData]
public class UserData 
{
    [FirestoreProperty] public string id { get; set; }
    [FirestoreProperty] public string name { get; set; }
    [FirestoreProperty] public int age { get; set; }
    [FirestoreProperty] public int grade { get; set; }
    [FirestoreProperty] public UserGender gender { get; set; }
    [FirestoreProperty] public UserSchoolType schoolType { get; set; }
    [FirestoreProperty] public string country { get; set; }
    [FirestoreProperty] public UserLivingWith livingWith { get; set; }


    [FirestoreProperty] public Dictionary<string, Dictionary<string, object>> userAnalytics { get; set; } = new Dictionary<string, Dictionary<string, object>>();
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
		id = Guid.NewGuid().ToString();
		name = "Unnamed";
		age = -1;
        grade = -1;
        gender = UserGender.NONE;
		schoolType = UserSchoolType.NONE;
		country = string.Empty;
		livingWith = UserLivingWith.NONE;

		userAnalytics = new Dictionary<string, Dictionary<string, object>>();

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
	Masculino,
	Femenino,
	NONE,
}

public enum UserSchoolType
{
    Privado,
    PúblicoUrbano,
    PúblicoRural,
	NONE
}

[Flags]
public enum UserLivingWith
{
	NONE = 0,
	Father = 1,
	Mother = 2,
	Siblings = 4,
	Uncles = 8,
	Grandparents = 16,
	Other = 32,
}
