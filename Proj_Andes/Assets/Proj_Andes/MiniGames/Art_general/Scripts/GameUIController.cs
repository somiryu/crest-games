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

        //Audio active by default
        soundActive = 1;
        soundActive = (int)PlayerPrefs.GetInt(UserDataManager.CurrUser.id + " isTheSoundActive", soundActive);
        ActivateSound(soundActive == 1);
        musicBtn.onClick.AddListener(SwitchAudioActive);

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

    void SwitchAudioActive()
    {
		var newValue = !AudioManager.Instance.currentBkMusic.isPlaying;
        ActivateSound(newValue);
	}

    void ActivateSound(bool activated)
    {
        if (activated)
        {
            AudioManager.Instance.currentBkMusic.Play();
            musicBtn.image.sprite = soundActivated;
        }
        else
        {
			AudioManager.Instance.currentBkMusic.Stop();
			musicBtn.image.sprite = soundDeactivated;
        }

        PlayerPrefs.SetInt(UserDataManager.CurrUser.id + " isTheSoundActive", (int)AudioListener.volume);
    }
}
