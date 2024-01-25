using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SkipTutorialManager : MonoBehaviour
{
    [SerializeField] SimpleGameSequenceItemTutorial itemTutorial;
    [SerializeField] Button skipTutorialBtn;
    void Start()
    {
        skipTutorialBtn.gameObject.SetActive(UserDataManager.CurrUser.IsTutorialStepDone(itemTutorial.tutorialID));
        skipTutorialBtn.onClick.AddListener(itemTutorial.OnSequenceOver);
        Debug.Log("is completed " + UserDataManager.CurrUser.IsTutorialStepDone(itemTutorial.tutorialID));
    }

}
