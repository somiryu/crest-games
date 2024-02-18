using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResponseBtnClickMeBehaviour : MonoBehaviour
{
    public GameObject clickMeObj;
    public GameObject WrongChoiceObj;
    public Button mainBtn;

  
    void Update()
    {
        clickMeObj.SetActive(mainBtn.interactable);
		WrongChoiceObj.SetActive(!mainBtn.interactable);
    }
}
