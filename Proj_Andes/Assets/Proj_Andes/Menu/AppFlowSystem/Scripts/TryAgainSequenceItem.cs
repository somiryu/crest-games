using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TryAgainSequenceItem", menuName = "GameSequencesList/TryAgainSequenceItem")]
public class TryAgainSequenceItem : SimpleGameSequenceItem
{
    public int tryAgainTrial;
    [NonSerialized] public int clickAmounts;
    public FrustrationTermometer frustrationTermometer;
    public override void SaveAnalytics()
    {
        clickAmounts = TryAgainManager.clickCounts;
        frustrationTermometer.frustrationGameItem.itemAnalytics.Add(DataIds.tryAgainClicks, clickAmounts);

        switch (tryAgainTrial)
        {
            case 1: UserDataManager.SaveUserAnayticsPerGame(DataIds.frustrationGames, frustrationTermometer.frustrationGameItem.itemAnalytics, DataIds.mechanicHandGame);
                break;            
            case 2: UserDataManager.SaveUserAnayticsPerGame(DataIds.frustrationGames, frustrationTermometer.frustrationGameItem.itemAnalytics, DataIds.boostersAndScapeGame);
                break;            
            case 3: UserDataManager.SaveUserAnayticsPerGame(DataIds.frustrationGames, frustrationTermometer.frustrationGameItem.itemAnalytics, DataIds.magnetsGame);
                break;
        }
    }
    public override void ResetCurrentAnalytics()
    {
        clickAmounts = 0;
        base.ResetCurrentAnalytics();
    }
}
