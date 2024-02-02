using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tymski;

public class GameUIController : MonoBehaviour
{
    static GameUIController instance;
    public static GameUIController Instance => instance;
    [SerializeField] Button homeBtn;
    [SerializeField] Transform menuContainer;
    [SerializeField] Button continueBtn;
    [SerializeField] Button exitBtn;
    [SerializeField] Button musicBtn;
    [SerializeField] Sprite soundActivated;
    [SerializeField] Sprite soundDeactivated;
    int soundActive;
    [SerializeField] Image tutorialImg;
    private void Awake()
    {
        if (instance != null && instance != this) DestroyImmediate(this);
    }
    void Start()
    {
        homeBtn.onClick.AddListener(OpenMenu);
        continueBtn.onClick.AddListener(Continue);
        exitBtn.onClick.AddListener(ExitGame);

        soundActive = (int)PlayerPrefs.GetInt(UserDataManager.CurrUser.id + " isTheSoundActive", soundActive);
        var findConfig = soundActive == 1 ? 0 : 1;
        ActivateSound(findConfig);
        musicBtn.onClick.AddListener(() => ActivateSound((int)AudioListener.volume));

        menuContainer.gameObject.SetActive(false);
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
    public void ExitGame()
    {
        Time.timeScale = 1;
        Debug.Log("exiting");

        GameSequencesList.Instance.EndSequence();
    }
    void ActivateSound(int activated)
    {
        if(activated == 0)
        {
            AudioListener.volume = 1;
            musicBtn.image.sprite = soundActivated;
        }
        else
        {
            AudioListener.volume = 0;
            musicBtn.image.sprite = soundDeactivated;
        }
        PlayerPrefs.SetInt(UserDataManager.CurrUser.id + " isTheSoundActive", (int)AudioListener.volume);
        Debug.Log("THIS IS WHATS BEING SAVED " + PlayerPrefs.GetInt(UserDataManager.CurrUser.id + " isTheSoundActive", (int)AudioListener.volume));
    }
}
