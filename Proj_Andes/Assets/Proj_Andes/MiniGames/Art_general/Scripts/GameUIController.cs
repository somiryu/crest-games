using System;
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
    [Header("Exit confirmation (minigames): the facilitator must type the word to abandon the session")]
    [SerializeField] GameObject exitConfirmPanel;
    [SerializeField] TMP_InputField exitConfirmInput;
    [SerializeField] Button exitConfirmBtn;
    [SerializeField] Button exitCancelBtn;
    public const string ExitWord = "salir";
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
    public bool showHomeBtn;
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
        exitBtn.onClick.AddListener(OnExitPressed);
        if (exitConfirmPanel != null)
        {
            exitConfirmPanel.SetActive(false);
            exitConfirmInput.onValueChanged.AddListener(text => exitConfirmBtn.interactable = IsExitWord(text));
            exitConfirmBtn.onClick.AddListener(ConfirmAbandon);
            exitCancelBtn.onClick.AddListener(CancelExit);
        }
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
    void Update()
    {
        if (GameSequencesList.isTheNarrativeSequence) showHomeBtn = true;
        if (showHomeBtn) homeBtn.gameObject.SetActive(true);
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
    /// <summary>The word that has to be typed to leave a session (ignores case and spaces around it).</summary>
    public static bool IsExitWord(string text) => string.Equals(text?.Trim(), ExitWord, StringComparison.OrdinalIgnoreCase);

    void OnExitPressed()
    {
        //The narratives app keeps resuming where the child left, so it exits as before
        if (GameSequencesList.isTheNarrativeSequence || exitConfirmPanel == null)
        {
            ExitGame();
            return;
        }
        exitConfirmInput.text = string.Empty;
        exitConfirmBtn.interactable = false;
        exitConfirmPanel.SetActive(true);
    }

    void CancelExit()
    {
        exitConfirmInput.text = string.Empty;
        exitConfirmPanel.SetActive(false);
    }

    //Leaving a minigames session discards it: nothing of it is saved or uploaded
    void ConfirmAbandon()
    {
        if (!IsExitWord(exitConfirmInput.text)) return;
        exitConfirmPanel.SetActive(false);
        TimeManager.Instance.RemoveNewStopTimeUser(this);
        onPause = false;
        GameSequencesList.Instance.AbandonSequence();
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
