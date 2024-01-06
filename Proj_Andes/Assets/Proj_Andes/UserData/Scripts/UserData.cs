using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData 
{
	public string id;
	public string name;
	public int age;
	public UserGender gender;
    public string city;
	public string institution;
	public bool tutorialNarrative;	
    public Dictionary<string, bool> tutorialStepsDone = new Dictionary<string, bool>();
    public Dictionary<string, object> userAnayticsResults = new Dictionary<string, object>();


    //This refers to the Game group index
    public int CheckPointIdx = -1;
	//This refers to the item idx inside of the game group index
	public int CheckPointSubIdx = -1;
	public List<NarrativeNavigationNode> narrativeNavCheckPointsNodes;
	public List<int> itemsPlayedIdxs = new List<int>();
	public int Coins;
	public List<Monsters> myCollectionMonsters = new List<Monsters>();

    public UserData()
	{
		id = Guid.NewGuid().ToString();
		name = "Unnamed";
		age = -1;
		gender = UserGender.NONE;
		city = string.Empty;
		institution = string.Empty;
        userAnayticsResults = new Dictionary<string, object>();

        CheckPointIdx = -1;
		CheckPointSubIdx = -1;
		narrativeNavCheckPointsNodes = new List<NarrativeNavigationNode>();
		itemsPlayedIdxs = new List<int>();
		Coins = 0;
        myCollectionMonsters = new List<Monsters>();
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
