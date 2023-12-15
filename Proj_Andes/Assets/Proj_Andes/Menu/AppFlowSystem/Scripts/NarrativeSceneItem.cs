using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NarrativeSceneItem", menuName = "GameSequencesList/NarrativeSceneItem")]
public class NarrativeSceneItem : GameSequence
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

