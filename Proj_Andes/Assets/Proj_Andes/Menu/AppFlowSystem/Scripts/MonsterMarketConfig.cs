using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterMarketConfig", menuName = "MonsterMarket/MonsterMarketConfig")]
public class MonsterMarketConfig : SimpleGameSequenceItem
{
    public int RegularChestPrice;
    public int RareChestPrice;
    public int LegendaryChestPrice;
    public MonstersLibrary monstersLibrary;
    public static bool isLastMarket;
    [HideInInspector] public static int marketAppearTimes;

	public int AvailableCoins => UserDataManager.CurrUser.Coins;
    public List<string> MyCollectionMonsters => UserDataManager.CurrUser.myCollectionMonsters;

    public void ConsumeCoins(int amountToSubstract)
    {
        UserDataManager.CurrUser.Coins -= amountToSubstract;
	}

    public void AddMonsterToCollection(Monsters monster)
    {
        MyCollectionMonsters.Add(monster.guid);
    }
}
