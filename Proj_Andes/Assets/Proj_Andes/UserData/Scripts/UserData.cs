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

	//This refers to the Game group index
	public int CheckPointIdx = -1;
	//This refers to the item idx inside of the game group index
	public int CheckPointSubIdx = -1;
	public List<NarrativeNavigationNode> narrativeNavCheckPointsNodes;
	public List<int> itemsPlayedIdxs = new List<int>();
    public List<Dictionary<string, object>> userAnayticsResults = new List<Dictionary<string, object>>();
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
		CheckPointIdx = -1;
		CheckPointSubIdx = -1;
		narrativeNavCheckPointsNodes = new List<NarrativeNavigationNode>();
		itemsPlayedIdxs = new List<int>();
		Coins = 0;
        myCollectionMonsters = new List<Monsters>();
    }
}

public enum UserGender
{
	Masculino,
	Femenino,
	NONE,
}
