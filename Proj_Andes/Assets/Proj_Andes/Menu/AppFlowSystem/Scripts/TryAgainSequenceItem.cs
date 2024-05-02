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
		//If there's no IDs, then there isn't a previous game on which we could write, so we don't store anything
		if (string.IsNullOrEmpty(UserDataManager.LastDocumentIDStored) || string.IsNullOrEmpty(UserDataManager.LastCollectionIDStored)) return;

		if (!UserDataManager.userAnayticsPerGame.TryGetValue(UserDataManager.LastCollectionIDStored, out var collectionFound)) return;
		if (!collectionFound.TryGetValue(UserDataManager.LastDocumentIDStored, out var DocumentFound)) return;

		DocumentFound.Add(DataIds.tryAgainClicks, clickAmounts);
        foreach (var item in DocumentFound)
        {
            Debug.Log("found " + item.Value);
        }
    }

    public override void ResetCurrentAnalytics()
    {
        clickAmounts = 0;
        base.ResetCurrentAnalytics();
    }
}
