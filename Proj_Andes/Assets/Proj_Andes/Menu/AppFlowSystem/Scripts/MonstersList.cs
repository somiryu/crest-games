using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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
