using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleGameSequenceItemTutorial", menuName = "GameSequencesList/SimpleGameSequenceItemTutorial")]
public class SimpleGameSequenceItemTutorial : SimpleGameSequenceItem
{
    [SerializeField] tutorialSteps tutorialID;

    public override SimpleGameSequenceItem GetNextItem()
    {
        if (GameSequencesList.Instance.prevGame != this && !UserDataManager.CurrUser.IsTutorialStepDone(tutorialID)) return this;
        else return null;
    }
}