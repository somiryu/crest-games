using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public Dictionary<string, bool> stepsTutorialNarrativeScenes;

    public List<TutorialUser> usersTutorial = new List<TutorialUser>(10);

    private static TutorialManager instance;
    public static TutorialManager Instance => instance;


    private void Awake()
    {
        if (instance != this)
        {
            if (instance != null)
            {
                DestroyImmediate(instance.gameObject);
            }
        }
        instance = this;

        var rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjs.Length; i++)
        {
            var curr = rootObjs[i];
            GetIUsers(curr, usersTutorial);
        }
    }

    public void GetIUsers(GameObject currObj, List<TutorialUser> usersList)
    {
        if (currObj.TryGetComponent(out TutorialUser user))
        {
            usersList.Add(user);
        }
        for (int i = 0; i < currObj.transform.childCount; i++)
        {
            var currChild = currObj.transform.GetChild(i);
            GetIUsers(currChild.gameObject, usersList);
        }
    }

    public void AddNewUser(TutorialUser user)
    {
        if (!usersTutorial.Contains(user)) usersTutorial.Add(user);
	}

    public void RemoveUser(TutorialUser user)
    {
        if (usersTutorial.Contains(user)) usersTutorial.Remove(user);
	}

    public void TurnOffTutorialStep(tutorialSteps tutorialStep)
    {
		UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialStep.ToString());
		var users = usersTutorial.FindAll(x => x.tutorialStep == tutorialStep);
        for (int i = 0; i < usersTutorial.Count; i++)
        {
            if (usersTutorial[i].tutorialStep != tutorialStep) continue;
            usersTutorial[i].OffTutorial();
        }
    }

    public void TurnOnTutorialStep(tutorialSteps tutorialStep)
    {
        var users = usersTutorial.FindAll(x => x.tutorialStep == tutorialStep);
        for (int i = 0; i < usersTutorial.Count; i++)
        {
            if (usersTutorial[i].tutorialStep != tutorialStep) continue;
            usersTutorial[i].OnTutorial();
        }        
    }
}

public enum tutorialSteps
{
    stepStartPopUp = 0,
    stepSkipButton = 1,
    stepResponseButton = 2,
    stepConfirmedButton = 3,
    MG_Magnets_1NoClick = 4, 
    MG_Magnets_2FourItemEnergyClick = 5,
    HeartsAndStarsDone = 6,
    SizeRocketsDone = 7,
    FightTheAlienDone = 8,
    VoiceStarOrFlowerDone = 9,
    TurboRocketDone = 10,
	MG_BoostersAndScapeDone = 11,
    MG_MechanicHand_1HoldClickAndMove = 12,
    MG_MechanicHand_2JustClickToGrab = 13,
    Market_Instruction
}
