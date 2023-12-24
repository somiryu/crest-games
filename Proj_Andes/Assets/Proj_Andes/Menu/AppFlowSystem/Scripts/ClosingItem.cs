using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClosingItem", menuName = "GameSequencesList/ClosingItem")]
public class ClosingItem : SimpleGameSequenceItem
{
    public override Dictionary<string, object> GetAnalytics()
    {
        return genericDictionary;
    }
}
