using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TryAgainSequenceItem", menuName = "GameSequencesList/TryAgainSequenceItem")]
public class TryAgainSequenceItem : SimpleGameSequenceItem
{
    public int tryAgainTrial;
    [NonSerialized] public int clickAmounts;
    [NonSerialized] public int ExtraClickAmounts;    
    public override void SaveAnalytics()
    {
        clickAmounts = TryAgainManager.clickCountsBeforeBarCompleted;
        ExtraClickAmounts = TryAgainManager.clickCountsAfterBarCompleted;
		//If there's no IDs, then there isn't a previous game on which we could write, so we don't store anything
		if (UserDataManager.LastDocumentIDsStored == null || string.IsNullOrEmpty(UserDataManager.LastCollectionIDStored)) return;

		if (!UserDataManager.userAnayticsPerGame.TryGetValue(UserDataManager.LastCollectionIDStored, out var collectionFound))
		{
			if (!DatabaseManager.pendingSessionsToUpload.TryGetValue(UserDataManager.LastCollectionIDStored, out collectionFound)) return;
		}


        for (int i = 0; i < UserDataManager.LastDocumentIDsStored.Count; i++)
        {
            if (!collectionFound.TryGetValue(UserDataManager.LastDocumentIDsStored[i], out var DocumentFound)) continue;
            Debug.Log("tryclick " + UserDataManager.LastDocumentIDsStored[i]);
			if (DocumentFound.TryGetValue(DataIds.mechHandThrown, out var valueFound))
			{
				DocumentFound.Add(DataIds.tryAgainClicksMechHand, clickAmounts);
                //DocumentFound.Add(DataIds.tryAgainClicksAfterWait, ExtraClickAmounts);
            }
            else if (DocumentFound.TryGetValue(DataIds.frustPersBoostClicks, out var otherValueFound))
			{
				DocumentFound.Add(DataIds.tryAgainClicksBoosters, clickAmounts);
                //DocumentFound.Add(DataIds.tryAgainClicksAfterWait, ExtraClickAmounts);
            }
            /*
            foreach (var item in DocumentFound)
			{
				Debug.Log("frust " + item.Key + " " + item.Value);
			}*/
		}
    }

    public override void ResetCurrentAnalytics()
    {
        clickAmounts = 0;
        base.ResetCurrentAnalytics();
    }
}
