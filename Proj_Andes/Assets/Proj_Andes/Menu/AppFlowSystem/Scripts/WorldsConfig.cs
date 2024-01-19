using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldsConfig", menuName = "GameSequencesList/WorldsConfig")]
public class WorldsConfig : SimpleGameSequenceItem
{
    public int gameIndex;
    public override SimpleGameSequenceItem GetNextItem()
    { 
        var currItem = base.GetNextItem();
        if (currItem != null) WorldsManager.index = gameIndex;
        return currItem;
    }
}
