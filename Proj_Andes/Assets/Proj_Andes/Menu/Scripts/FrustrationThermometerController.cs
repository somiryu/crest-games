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
    [SerializeField] Transform blockingPanel;
    [SerializeField] Button continueBtn;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip introAudio;
    bool startToChoose = false;
    float timer;

    void Start()
    {
        frustlevelButtons = buttonsContainer.GetComponentsInChildren<Button>();
        TryGetComponent(out audioSource);
        for (int i = 0; i < frustrationLevels.Count; i++)
        {
            int idx = i;
            frustrationLevels[idx].frustLevelButton.onClick.AddListener(() => GetFrustationLevel(frustrationLevels[idx]));

            if (UserDataManager.CurrUser.sexo == UserSex.Hombre) frustrationLevels[idx].FLabel.gameObject.SetActive(false);
            else frustrationLevels[idx].MLabel.gameObject.SetActive(false);

            buttonsSelectedImages[i].SetActive(false);
        }
        StartCoroutine(Intro());
        continueBtn.onClick.AddListener(Continue);
        continueBtn.gameObject.SetActive(false);
    }
    IEnumerator Intro()
    {
        blockingPanel.gameObject.SetActive(true);
        audioSource.clip = introAudio;
        audioSource.Play();
        yield return new WaitForSeconds(introAudio.length-0.4f);
        for (int i = 0; i < frustrationLevels.Count; i++)
        {
            var currDescription = frustrationLevels[i];
            GetFrustationLevel(currDescription);
            continueBtn.gameObject.SetActive(false);
            var currAudio = UserDataManager.CurrUser.sexo == UserSex.Mujer ? currDescription.FbuttonSound : currDescription.MbuttonSound;
            audioSource.clip = currAudio;
            audioSource.Play();
            yield return new WaitForSeconds(currAudio.length);
            buttonsSelectedImages[i].SetActive(false);
        }
        startToChoose = true;
        blockingPanel.gameObject.SetActive(false);
    }
    void GetFrustationLevel(FrustrationLevels level)
    {
        currFrustratioNlevel = level.level;
        Debug.Log(currFrustratioNlevel);
        ButtonPressed(level.frustLevelButton);
        for (int i = 0; i < frustlevelButtons.Length; i++)
        {
            buttonsSelectedImages[i].SetActive(level.idx == i);
            if (level.idx == i) continue;
            else ButtonUnpressed(frustlevelButtons[i]);
        }
        if(UserDataManager.CurrUser.sexo == UserSex.Hombre) audioSource.clip = level.MbuttonSound;
        else audioSource.clip = level.FbuttonSound;
        audioSource.Play();
        continueBtn.gameObject.SetActive(true);
    }
    void Update()
    {
        if(!startToChoose) return;
        timer += Time.deltaTime;
    }

    void Continue()
    {
        FrustrationTermometer.LastFrustrationLevelPicked = currFrustratioNlevel;
		FrustrationTermometer.selectedFrustrationLevel = currFrustratioNlevel;
        Debug.Log(currFrustratioNlevel);
        FrustrationTermometer.timerToPickEmotion = timer;
        frustrationTermometer.OnSequenceOver();
    }

    void ButtonPressed(Button button)
    {
        button.image.color = Color.magenta;
    }

    void ButtonUnpressed(Button button)
    {
        button.image.color = Color.white;
    }
}
public enum FrustrationLevel
{
    Muy_Frustrado,
    Un_Poco_Frustrado,
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
