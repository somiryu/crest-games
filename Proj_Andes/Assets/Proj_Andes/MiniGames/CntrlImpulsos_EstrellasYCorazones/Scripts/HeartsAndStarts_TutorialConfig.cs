using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HearthAndStarsTutorialConfigs", menuName = "TutorialConfig/HearthAndStarsTutorialConfigs")]
public class HeartsAndStarts_TutorialConfig : SimpleGameSequenceItemTutorial
{
    public HeartsAndFlowersGameType gameType;
    public static int gameIdx;

    public override SimpleGameSequenceItem GetNextItem()
    {
        var currItem = base.GetNextItem();
        if (currItem != null)
        {
            HeartsAndStarts_Manager_Tutorial.currTutoStepIdx = gameIdx;
        }
        return currItem;
    }

	public override void OnReset()
	{
		HeartsAndStarts_Manager_Tutorial.currTutoStepIdx = -1;
	}
}
