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
        
        for (int i = 0; i < frustlevelButtons.Length; i++)
        {
            int idx = i;
            frustlevelButtons[i].onClick.AddListener(() => GetFrustationLevel((FrustrationLevel)idx, frustlevelButtons[idx], idx));
        }
        continueBtn.onClick.AddListener(Continue);
        continueBtn.gameObject.SetActive(false);
    }

    void GetFrustationLevel(FrustrationLevel frustrationLevel, Button button, int idx)
    {
        currFrustratioNlevel = frustrationLevel;
        ButtonPressed(button);
        for (int i = 0; i < frustlevelButtons.Length; i++)
        {
            if (idx == i) continue;
            else ButtonUnpressed(frustlevelButtons[i]);
        }
        continueBtn.gameObject.SetActive(true);
    }

    void Continue()
    {
        frustrationTermometer.selectedFrustrationLevel = currFrustratioNlevel;
        frustrationTermometer.OnSequenceOver();
    }

    void ButtonPressed(Button button)
    {
        button.image.color = Color.magenta;
        Debug.Log("pressed");
    }

    void ButtonUnpressed(Button button)
    {
        button.image.color = Color.white;
        Debug.Log("unpressed");
    }
}
public enum FrustrationLevel
{
    Muy_Frustrado,
    Frustrado,
    Un_Poco_Tranquilo,
    Muy_Tranquilo,
}
