using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterMarketConfig", menuName = "GameSequencesList/MonsterMarketConfig")]
public class MonsterMarketConfig : SimpleGameSequenceItem
{
    public int RegularChestPrice;
    public int RareChestPrice;
    public int LegendaryChestPrice;
    public int availableCoins;
    [NonSerialized] public List<Monsters> myCollectionMonsters = new List<Monsters>();

    public void InitConfig()
    {
        myCollectionMonsters = UserDataManager.CurrUser.myCollectionMonsters;
        availableCoins = UserDataManager.CurrUser.Coins;
    }
    public override void SaveAnalytics()
    {
        if(UserDataManager.CurrUser != null)
        {
            UserDataManager.CurrUser.Coins = availableCoins;
            UserDataManager.CurrUser.myCollectionMonsters = myCollectionMonsters;
        }
        Debug.Log("Storing market stuff");
    }
}
