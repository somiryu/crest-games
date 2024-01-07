using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class TutorialManager : MonoBehaviour
{
    public Dictionary<string, bool> stepsTutorialNarrativeScenes;

    [SerializeField] List<TutorialUser> usersTutorial = new List<TutorialUser>(10);

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

    public void TurnOffTutorial(tutorialSteps tutorialStep)
    {
        var user = usersTutorial.Find(x => x.tutorialStep == tutorialStep); 
        user.OffTutorial();
        UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialStep.ToString());
    }
}

public enum tutorialSteps
{
    stepSkipButton,
    stepResponseButton,
    stepConfirmedButton
}