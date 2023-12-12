using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NarrativeSceneItem", menuName = "GameSequencesList/NarrativeSceneItem")]
public class NarrativeSceneItem : GameSequenceItem
{

}

[CreateAssetMenu(fileName = "NarrativeScenesGroup", menuName = "GameSequencesList/NarrativeScenesGroup")]
public class NarrativeScenesGroup : GameSequence
{
    public List<NarrativeSceneItem> narrativeScenes;
    [NonSerialized] List<NarrativeSceneItem> scenesPlayed;
    public override GameSequenceItem GetNextItem()
    {
        return narrativeScenes[0];
    }

    public override void OnReset()
    {
        scenesPlayed.Clear();
    }

    public override void OnSequenceOver()
    {
        GameSequencesList.Instance.GoToNextSequence();
    }
}
