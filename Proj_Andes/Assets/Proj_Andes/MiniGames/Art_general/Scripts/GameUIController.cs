using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tymski;

public class GameUIController : MonoBehaviour, ITimeManagement
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
    [SerializeField] Image coinBk;
    [SerializeField] Pool<ScoreStarsController> pool = new Pool<ScoreStarsController>();
    [SerializeField] Transform starsContainer;
    [SerializeField] Transform parent;
    [SerializeField] ScoreStarsController starPrefab;
    public bool onTuto = false;
    public bool onPause;
    Animator anim;
    IEnumerator sendStars;
    private void Awake()
    {
        if (instance != null && instance != this) DestroyImmediate(this);
        instance = this;
    }
    void Start()
    {
        TryGetComponent(out anim);
        homeBtn.onClick.AddListener(OpenMenu);
        continueBtn.onClick.AddListener(Continue);
        exitBtn.onClick.AddListener(ExitGame);
        onPause = false;
        //Audio active by default
        pool.Init(40);

        soundActive = 1;
        soundActive = (int)PlayerPrefs.GetInt(UserDataManager.CurrUser.id_jugador + " isTheSoundActive", soundActive);
        ActivateSound(soundActive == 1);
        musicBtn.onClick.AddListener(SwitchAudioActive);

        menuContainer.gameObject.SetActive(false);
        //StopCoroutine(sendStars);
    }
    void OpenMenu()
    {
        menuContainer.gameObject.SetActive(true);
        onPause = true;
        TimeManager.Instance.SetNewStopTimeUser(this);
        if (CatchCoinsAudioInstruction.Instance != null)
        {
            if (CatchCoinsAudioInstruction.Instance.startedCorr)
            {
                CatchCoinsAudioInstruction.Instance.StopAudioIns();
            }
        }
    }
    void Continue()
    {
        menuContainer.gameObject.SetActive(false);
        onPause = false;
        TimeManager.Instance.RemoveNewStopTimeUser(this);
        if (CatchCoinsAudioInstruction.Instance != null)
        {
            if (CatchCoinsAudioInstruction.Instance.startedCorr)
            {
                CatchCoinsAudioInstruction.Instance.RestartAudio();
            }
        }
    }
    public void ExitGame()
    {
        TimeManager.Instance.RemoveNewStopTimeUser(this);
        onPause = false;
        DialoguesDisplayerUI.SaveAnalytics();
        if(NarrativeSceneManager.Instance != null)
        {
            DialoguesDisplayerUI.Instance.UnloadAllImages();
        }
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

        PlayerPrefs.SetInt(UserDataManager.CurrUser.id_jugador + " isTheSoundActive", activated ? 1 : 0);
    }
    public void StarEarned(Vector3 initPos, int earnedStars = 1)
    {
        Debug.DrawLine(initPos, initPos + Vector3.one * 100, Color.yellow, 10);
        sendStars = SendStars(initPos, earnedStars);
        StartCoroutine(sendStars);
    }
    IEnumerator SendStars(Vector3 initPos, int starsAmt)
    {
        for (int i = 0; i < starsAmt; i++)
        {
            var newStar = pool.GetNewItem();
            var finalPos = starsContainer.transform.position;
            newStar.Init(initPos, finalPos, pool);
            yield return new WaitForSeconds(0.2f);
        }
    }
    public void StarLost()
    {
		anim.SetTrigger("OnCoinLost");
    }
}
