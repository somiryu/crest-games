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

}
