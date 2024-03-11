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
    public override void SaveAnalytics()
    {
        //If there's no IDs, then there isn't a previous game on which we could write, so we don't store anything
        if (string.IsNullOrEmpty(UserDataManager.LastDocumentIDStored) || string.IsNullOrEmpty(UserDataManager.LastCollectionIDStored)) return;

        if (!UserDataManager.userAnayticsPerGame.TryGetValue(UserDataManager.LastCollectionIDStored, out var collectionFound)) return;
        if (!collectionFound.TryGetValue(UserDataManager.LastDocumentIDStored, out var DocumentFound)) return;

        Debug.Log(selectedFrustrationLevel.ToString());
        DocumentFound.Add(DataIds.frustrationLevel, selectedFrustrationLevel.ToString());
    }
}
