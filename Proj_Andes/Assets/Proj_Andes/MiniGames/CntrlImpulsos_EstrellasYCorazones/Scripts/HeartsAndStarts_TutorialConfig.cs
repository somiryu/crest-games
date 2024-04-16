using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HearthAndStarsTutorialConfigs", menuName = "TutorialConfig/HearthAndStarsTutorialConfigs")]
public class HeartsAndStarts_TutorialConfig : SimpleGameSequenceItemTutorial
{
    public static HeartsAndFlowersGameType gameType;
    public int gameIdx;
    public float timePerChoiceTuto;
    public float intermidiateHold;
    public override SimpleGameSequenceItem GetNextItem()
    {
        var currItem = base.GetNextItem();
        if (currItem != null) HeartsAndStarts_Manager_Tutorial.currTutoStepIdx = gameIdx;
        return currItem;
    }
}
