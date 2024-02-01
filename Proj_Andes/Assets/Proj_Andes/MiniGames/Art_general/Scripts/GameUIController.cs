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

        Debug.Log(AudioListener.volume +  "vol");
        soundActive = (int)PlayerPrefs.GetInt("isTheSoundActive", (int)AudioListener.volume);

        ActivateSound(soundActive);
        musicBtn.onClick.AddListener(() => ActivateSound((int)AudioListener.volume));

        menuContainer.gameObject.SetActive(false);
    }
    private void Update()
    {
        Debug.Log("vol "+ AudioListener.volume);
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
        if(activated >= 0 && activated < 1)
        {
            AudioListener.volume = 1;
            musicBtn.image.sprite = soundActivated;
        }
        else
        {
            AudioListener.volume = 0;
            musicBtn.image.sprite = soundDeactivated;
        }
        PlayerPrefs.SetInt(UserDataManager.CurrUser.name + " isTheSoundActive", (int)AudioListener.volume);
    }
}
