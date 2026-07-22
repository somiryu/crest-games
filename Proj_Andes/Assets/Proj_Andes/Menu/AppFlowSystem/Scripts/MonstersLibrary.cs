using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonstersLibrary", menuName = "MonstersMarket/MonstersLibrary")]
public class MonstersLibrary : ScriptableObject
{
	public List<MonstersList> monsters;

	private void OnValidate()
	{
	}

	public Monsters GetMonsterByID(string id)
	{
		return null;
	}

	public Monsters GetRandomMonster(MonsterChestType type)
	{
		return null;
	}
}
