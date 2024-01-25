using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tymski;

public class GameUIController : MonoBehaviour
{
    [SerializeField] Button homeBtn;
    [SerializeField] Transform menuContainer;
    [SerializeField] Button continueBtn;
    [SerializeField] Button exitBtn;
    [SerializeField] Button musicBtn;
    [SerializeField] Sprite soundActivated;
    [SerializeField] Sprite soundDeactivated;
    [SerializeField] Image tutorialImg;
    [SerializeField] bool isaTuto;
    EndOfGameManager eogManager;
    void Start()
    {
        eogManager = Utility.FindObjectByType<EndOfGameManager>();

        tutorialImg.gameObject.SetActive(TurnOnTurboBtn(isaTuto));

        homeBtn.onClick.AddListener(OpenMenu);
        continueBtn.onClick.AddListener(Continue);
        exitBtn.onClick.AddListener(ExitGame);
        musicBtn.onClick.AddListener(ActivateSound);

        menuContainer.gameObject.SetActive(false);

    }
    public bool TurnOnTurboBtn(bool isATuto)
    {
        tutorialImg.gameObject.SetActive(isATuto);
        return isATuto;
    }
    void OpenMenu()
    {
        menuContainer.gameObject.SetActive(true);
        Time.timeScale = 0;
    }
    void Continue()
    {
        menuContainer.gameObject.SetActive(false);
        Time.timeScale = 1;
    }
    void ExitGame()
    {
        Time.timeScale = 1;
        GameSequencesList.Instance.EndSequence();
    }
    void ActivateSound()
    {
        if(musicBtn.image.sprite == soundActivated)
        {
            AudioListener.volume = 0;
            musicBtn.image.sprite = soundDeactivated;
        }
        else
        {
            AudioListener.volume = 1;
            musicBtn.image.sprite = soundActivated;
        }
    }
}
