using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FrustrationTermometer", menuName = "GameSequencesList/FrustrationTermometer")]
public class FrustrationTermometer : SimpleGameSequenceItem
{
    public FrustrationLevel selectedFrustrationLevel;
    public FrustrationLevel defaultFrustrationLevel;
    public SimpleGameSequenceItem frustrationGameItem;
    public override void SaveAnalytics()
    {
        frustrationGameItem.itemAnalytics.Add(DataIds.frustrationLevel, selectedFrustrationLevel.ToString());
        Debug.Log("saving frust " + frustrationGameItem.itemAnalytics[DataIds.frustrationLevel]);
    }
}
