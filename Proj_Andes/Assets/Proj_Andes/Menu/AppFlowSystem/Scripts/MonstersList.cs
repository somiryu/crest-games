using System;
using System.Collections.Generic;

[Serializable]
public class MonstersList
{
	public MonsterChestType chestType;

	public List<Monsters> monsters;

	public void OnValidate()
	{
	}

	public Monsters GetMonsterByID(string ID)
	{
		return null;
	}

	public Monsters GetRandomMonster()
	{
		return null;
	}
}
