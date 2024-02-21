using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrustrationThermometerController : MonoBehaviour
{
    [SerializeField] FrustrationTermometer frustrationTermometer;
    [SerializeField] Transform buttonsContainer;
    Button[] frustlevelButtons;
    [SerializeField] List<FrustrationLevels> frustrationLevels = new List<FrustrationLevels>();
    [SerializeField] GameObject[] buttonsSelectedImages;
    FrustrationLevel currFrustratioNlevel;
    [SerializeField] Button continueBtn;

    [SerializeField] AudioSource audioSource;
    void Start()
    {
        frustlevelButtons = buttonsContainer.GetComponentsInChildren<Button>();
        TryGetComponent(out audioSource);
        for (int i = 0; i < frustrationLevels.Count; i++)
        {
            int idx = i;
            frustrationLevels[idx].frustLevelButton.onClick.AddListener(() => GetFrustationLevel(frustrationLevels[idx]));

            if (UserDataManager.CurrUser.gender == UserGender.Masculino) frustrationLevels[idx].FLabel.gameObject.SetActive(false);
            else frustrationLevels[idx].MLabel.gameObject.SetActive(false);

            buttonsSelectedImages[i].SetActive(false);
        }
        continueBtn.onClick.AddListener(Continue);
        continueBtn.gameObject.SetActive(false);
    }

    void GetFrustationLevel(FrustrationLevels level)
    {
        currFrustratioNlevel = level.level;
        ButtonPressed(level.frustLevelButton);
        for (int i = 0; i < frustlevelButtons.Length; i++)
        {
            buttonsSelectedImages[i].SetActive(level.idx == i);
            if (level.idx == i) continue;
            else ButtonUnpressed(frustlevelButtons[i]);
        }
        if(UserDataManager.CurrUser.gender == UserGender.Masculino) audioSource.clip = level.MbuttonSound;
        else audioSource.clip = level.FbuttonSound;
        audioSource.Play();
        continueBtn.gameObject.SetActive(true);
    }

    void Continue()
    {
        FrustrationTermometer.LastFrustrationLevelPicked = currFrustratioNlevel;
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
    NONE = -1,
}

[Serializable]
public class FrustrationLevels
{
    public FrustrationLevel level;
    public AudioClip MbuttonSound;
    public AudioClip FbuttonSound;
    public Button frustLevelButton;
    public TextMeshProUGUI MLabel;
    public TextMeshProUGUI FLabel;
    public int idx;
}
