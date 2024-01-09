using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialPopTextType : MonoBehaviour, iTutorialType
{
    [SerializeField] TextMeshProUGUI textUI;
    [SerializeField] string textContent;
    public void StepStart(bool stepCompleted)
    {
        textUI = GetComponentInChildren<TextMeshProUGUI>();
        textUI.text = textContent;
        textUI.gameObject.SetActive(true);
    }
    public void StepDone()
    {
        textUI.gameObject.SetActive(false);
    }



}
