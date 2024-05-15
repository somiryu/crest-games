using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FrustrationTermometer", menuName = "GameSequencesList/FrustrationTermometer")]
public class FrustrationTermometer : SimpleGameSequenceItem
{

    public static FrustrationLevel LastFrustrationLevelPicked = FrustrationLevel.NONE;

    public FrustrationLevel selectedFrustrationLevel;
    public FrustrationLevel defaultFrustrationLevel;
    public SimpleGameSequenceItem frustrationGameItem;
    public static float timerToPickEmotion;
    public override void SaveAnalytics()
    {
        //If there's no IDs, then there isn't a previous game on which we could write, so we don't store anything
        if (UserDataManager.LastDocumentIDsStored == null || string.IsNullOrEmpty(UserDataManager.LastCollectionIDStored)) return;

		if (!UserDataManager.userAnayticsPerGame.TryGetValue(UserDataManager.LastCollectionIDStored, out var collectionFound))
		{
			if (!DatabaseManager.pendingSessionsToUpload.TryGetValue(UserDataManager.LastCollectionIDStored, out collectionFound)) return;
		}

		for (int i = 0; i < UserDataManager.LastDocumentIDsStored.Count; i++)
        {
			if (!collectionFound.TryGetValue(UserDataManager.LastDocumentIDsStored[i], out var DocumentFound)) continue;

			Debug.Log(selectedFrustrationLevel.ToString());
			if (DocumentFound.TryGetValue(DataIds.mechHandThrown, out var valueFound))
			{
				DocumentFound.Add(DataIds.mechHandFeelAnswer, selectedFrustrationLevel.ToString());
				DocumentFound.Add(DataIds.mechHandFeelCode, selectedFrustrationLevel.GetAnalyticValue());
				DocumentFound.Add(DataIds.mechHandFeelTiming, timerToPickEmotion);
			}
			else if (DocumentFound.TryGetValue(DataIds.frustPersBoostClicks, out var otherValueFound))
			{
				DocumentFound.Add(DataIds.frustPersFeelAnswer, selectedFrustrationLevel.ToString());
				DocumentFound.Add(DataIds.frustPersFeelCode, (int)selectedFrustrationLevel.GetAnalyticValue());
				DocumentFound.Add(DataIds.frustPersFeelTiming, timerToPickEmotion);
			}
		}
    }
}
