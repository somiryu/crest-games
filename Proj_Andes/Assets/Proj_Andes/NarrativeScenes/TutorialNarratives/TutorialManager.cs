using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface iTutorialUser
{
    public void ShowTutorial();
}
public class TutorialManager : MonoBehaviour
{
    public bool stepSkipButton = false;
    public bool stepResponseButton = false;
    public bool stepConfirmedButton = false;

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

        ShowTutorial();

    }
    public void RemoveUser(iTutorialUser user)
    {
        if (usersTutorial.Contains(user))
            usersTutorial.Remove(user);
    }


    void Start()
    {
        ShowTutorial(); 

    }

    private void Update()
    {
        
    }


    public void ShowTutorial()
    {
        if (!UserDataManager.CurrUser.tutorialNarrative)
        {
            for (int i = 0; i < usersTutorial.Count; i++)
            {
                usersTutorial[i].ShowTutorial();
            }
        }
    }
}

public enum tutorialSteps
{
    StepSkipButton,
    StepResponseButton,
    StepConfirmedButton
}
