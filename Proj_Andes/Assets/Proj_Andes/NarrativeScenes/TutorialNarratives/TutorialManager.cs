using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
public interface iTutorialUser
{
    
    public void OffTutorial(tutorialSteps tutorialSteps);
}
public class TutorialManager : MonoBehaviour
{
    public Dictionary<string, bool> stepsTutorialNarrativeScenes;

    [SerializeField] List<iTutorialUser> usersTutorial = new List<iTutorialUser>(10);

    private static TutorialManager instance;
    public static TutorialManager Instance
    {
        get
        {
            return instance;
        }
    }
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


        stepsTutorialNarrativeScenes = new Dictionary<string, bool>()
        {
            {DataIds.stepConfirmedButton, UserDataManager.CurrUser.tutorialNarrative },
            {DataIds.stepResponseButton, UserDataManager.CurrUser.tutorialNarrative },
            {DataIds.stepSkipButton, UserDataManager.CurrUser.tutorialNarrative }
        };   


        var rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjs.Length; i++)
        {
            var curr = rootObjs[i];
            GetIUsers(curr, usersTutorial);
        }
    }

    public void GetIUsers(GameObject currObj, List<iTutorialUser> usersList)
    {
        if (currObj.TryGetComponent(out iTutorialUser user))
        {
            usersList.Add(user);
        }
        for (int i = 0; i < currObj.transform.childCount; i++)
        {
            var currChild = currObj.transform.GetChild(i);
            GetIUsers(currChild.gameObject, usersList);
        }
    }
    public void AddNewUser(iTutorialUser user)
    {
        if (!usersTutorial.Contains(user))
            usersTutorial.Add(user);
    }
    public void RemoveUser(iTutorialUser user)
    {
        if (usersTutorial.Contains(user))
            usersTutorial.Remove(user);
    }



    private void Update()
    {
        
    }


    public void TurnOffTutorial(tutorialSteps tutorialStep)
    {
        if (!UserDataManager.CurrUser.tutorialNarrative)
        {
            for (int i = 0; i < usersTutorial.Count; i++)
            {
                usersTutorial[i].OffTutorial(tutorialStep);
            }

            UserDataManager.CurrUser.tutorialNarrative = AreAllTutorialStepsComplete();
        }
    }

    public bool AreAllTutorialStepsComplete()
    {
        return stepsTutorialNarrativeScenes.All(kv => kv.Value);
    }
}

public enum tutorialSteps
{
    stepSkipButton,
    stepResponseButton,
    stepConfirmedButton
}
