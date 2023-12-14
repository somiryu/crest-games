using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinSelectorManager", menuName = "GameSequencesList/SkinSelectorManager")]
public class SkinSelectorItem : GameSequence
{
    public override GameSequenceItem GetNextItem()
    {
        return this;
    }

    public override void OnReset()
    {
    }

    public override void OnSequenceOver()
    {
        GameSequencesList.Instance.GoToNextSequence();
    }
}
