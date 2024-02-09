using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MonstersLibrary", menuName = "MonstersMarket/MonstersLibrary")]
public class MonstersLibrary : ScriptableObject
{
	public List<MonstersList> monsters = new List<MonstersList>();

	private void OnValidate()
	{
		for (int i = 0; i < monsters.Count; i++) monsters[i].OnValidate();
	}

	public Monsters GetMonsterByID(string id)
	{
		for(int i = 0;i < monsters.Count;i++)
		{
			var curr = monsters[i].GetMonsterByID(id);
			if (curr != null) return curr;
		}
		return null;
	}

	public Monsters GetRandomMonster(MonsterChestType type)
	{
		var monstertList = monsters.Find(x => x.chestType == type);
		if(monstertList == null)
		{
			Debug.Log("No monster list of type: " + type + " was found");
			return null;
		}
		return monstertList.GetRandomMonster();
	}


}

[Serializable]
public class MonstersList
{
	public MonsterChestType chestType;
	public List<Monsters> monsters;
	public void OnValidate()
	{
		for (int i = 0; i < monsters.Count; i++)
		{
			monsters[i].OnValidate();
		}
	}

	public Monsters GetMonsterByID(string ID)
	{
		for (int i = 0; i < monsters.Count; i++)
		{
			if (monsters[i].guid == ID) return monsters[i];
		}
		return null;
	}

	public Monsters GetRandomMonster()
	{
		var random = Random.Range(0, monsters.Count);
		return monsters[random];
	}
}
