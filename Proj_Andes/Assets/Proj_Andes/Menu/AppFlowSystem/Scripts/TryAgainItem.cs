using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TryAgainItem", menuName = "GameSequencesList/TryAgainItem")]
public class TryAgainItem : SimpleGameSequenceItem
{
    public override Dictionary<string, object> GetAnalytics()
    {
        return genericDictionary;
    }
}
