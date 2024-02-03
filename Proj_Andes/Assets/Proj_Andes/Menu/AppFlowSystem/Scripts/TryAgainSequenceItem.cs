using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TryAgainSequenceItem", menuName = "GameSequencesList/TryAgainSequenceItem")]
public class TryAgainSequenceItem : SimpleGameSequenceItem
{
    public int tryAgainTrial;
    [NonSerialized] public int clickAmounts;
    public override string GetSceneID()
    {
        switch (tryAgainTrial)
        {
            case 1: return DataIds.tryAgainClicks1;
            case 2: return DataIds.tryAgainClicks2;
            case 3: return DataIds.tryAgainClicks3;
            default: return DataIds.tryAgainClicks1;
        }
    }
    public override void SaveAnalytics()
    {
        clickAmounts = TryAgainManager.clickCounts;
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(GetSceneID(), clickAmounts.ToString());
        UserDataManager.SaveUserAnayticsPerGame(GetSceneID(), itemAnalytics);

    }
    public override void ResetCurrentAnalytics()
    {
        clickAmounts = 0;
        base.ResetCurrentAnalytics();
    }
}
