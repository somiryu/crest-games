using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrustrationThermometerController : MonoBehaviour
{
    [SerializeField] FrustrationTermometer frustrationTermometer;
    [SerializeField] Slider frustSlider;
    [SerializeField] Transform buttonsContainer;
    Button[] frustlevelButtons;
    FrustrationLevel currFrustratioNlevel;
    [SerializeField] Button continueBtn;
    void Start()
    {
        frustlevelButtons = buttonsContainer.GetComponentsInChildren<Button>();
        /*
        for (int i = 0; i < frustlevelButtons.Length; i++)
        {
            frustlevelButtons[i].onClick.AddListener(() => GetFrustationLevel((FrustrationLevel)i));
            Debug.Log(frustlevelButtons.Length + " " + (FrustrationLevel)i);
        }*/
        frustlevelButtons[0].onClick.AddListener(() => GetFrustationLevel((FrustrationLevel)0));
        frustlevelButtons[1].onClick.AddListener(() => GetFrustationLevel((FrustrationLevel)1));
        frustlevelButtons[2].onClick.AddListener(() => GetFrustationLevel((FrustrationLevel)2));
        frustlevelButtons[3].onClick.AddListener(() => GetFrustationLevel((FrustrationLevel)3));
        frustlevelButtons[4].onClick.AddListener(() => GetFrustationLevel((FrustrationLevel)4));
        continueBtn.onClick.AddListener(Continue);
    }

    void GetFrustationLevel(FrustrationLevel frustrationLevel)
    {
        currFrustratioNlevel = frustrationLevel;
        Debug.Log(currFrustratioNlevel);

    }

    void Continue()
    {
        frustrationTermometer.selectedFrustrationLevel = currFrustratioNlevel;
        Debug.Log(currFrustratioNlevel);
        frustrationTermometer.OnSequenceOver();
    }
}
public enum FrustrationLevel
{
    Tranquile,
    Uneasy,
    Unsetlled,
    Stressed,
    Frustrated,
}
