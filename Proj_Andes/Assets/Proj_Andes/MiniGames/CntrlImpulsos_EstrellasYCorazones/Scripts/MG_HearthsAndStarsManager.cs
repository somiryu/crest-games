using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Numerics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;
using Random = UnityEngine.Random;

public class MG_HearthsAndStarsManager : MonoBehaviour, IEndOfGameManager
{
    private static MG_HearthsAndStarsManager instance;
    public static MG_HearthsAndStarsManager Instance => instance;

    [SerializeField] MG_HearthAndStarsGameConfigs gameConfigs;
    public static HeartsAndFlowersGameType currGameType;
    [Space(20)]
    [SerializeField] Sprite sameDirectionSprite;
    [SerializeField] Sprite opositeDirectionSprite;
    [Space(20)]
    [SerializeField] Image leftImg;
    [SerializeField] Image rightImg;
    [Space(20)]
    [SerializeField] Button leftBtn;
    [SerializeField] Button rightBtn;

    [SerializeField] ParticleSystem LCorrectparticle;
    [SerializeField] ParticleSystem RCorrectparticle;

    [SerializeField] ParticleSystem LIncorrectparticle;
    [SerializeField] ParticleSystem RIncorrectparticle;

    [SerializeField] GameObject afterActionPanel;
    [SerializeField] GameObject inGameUIPanel;

    [SerializeField] AudioClip correctAudio;
    [SerializeField] AudioClip wrongAudio;
    [SerializeField] AudioClip finishAudio;

    [Header("UI")]
    [SerializeField] TMP_Text currCoinsValueTxt;
    [SerializeField] TMP_Text currRoundValueTxt;
    [SerializeField] TMP_Text afterActionFinalCoinsTxt;
    [SerializeField] Slider timerUI;
    GameUIController gameUi => GameUIController.Instance;

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;
    private AudioSource audiosource;

    private float timerPerChoice = 0;
    private int currCoins;
    private int currRound;

    private bool currShowingRight = false;
    private bool currRequiresSameDirection = false;

    private bool gameoverFlag = false;

    private MG_HearthAndStars_RoundAnalytics roundAnalytics;

    [SerializeField] public List<MG_HearthAndStars_RoundAnalytics> AllRoundsAnalytics;

    public void Awake()
    {
        if(instance != null && instance != this) DestroyImmediate(instance);
        instance = this;
        Init();
    }

    public void Init()
    {
        AllRoundsAnalytics = new List<MG_HearthAndStars_RoundAnalytics>(gameConfigs.maxRounds);
        currCoins = gameConfigs.initialCoins;
        currRound = 0;
        audiosource = GetComponent<AudioSource>();

        afterActionPanel.SetActive(false);
        inGameUIPanel.SetActive(true);
        gameoverFlag = false;

        timerUI.minValue = 0;
        timerUI.maxValue = gameConfigs.timePerChoice;

        leftBtn.onClick.AddListener(OnClickedLeft);
        rightBtn.onClick.AddListener(OnClickedRight);
        eogManager.OnGameStart();

        InitRound();
    }

	private void Start()
	{
        GeneralGameAnalyticsManager.Instance.Init(DataIds.heartsAndStarsGame);
	}

	void InitRound()
    {
		roundAnalytics = new MG_HearthAndStars_RoundAnalytics();
        AllRoundsAnalytics.Add(roundAnalytics);
		
        timerPerChoice = 0;

        rightImg.gameObject.SetActive(false);
        leftImg.gameObject.SetActive(false);

        switch (currGameType)
        {
            case HeartsAndFlowersGameType.Hearts:
                currRequiresSameDirection = true;
                break;            
            case HeartsAndFlowersGameType.Flowers:
                currRequiresSameDirection = false;
                break;            
            case HeartsAndFlowersGameType.Mixed:
                currRequiresSameDirection = Random.Range(0f, 1f) > 0.5f;
                break;
        }

        var spriteToShow = currRequiresSameDirection ? sameDirectionSprite : opositeDirectionSprite;
        currShowingRight = Random.Range(0f, 1f) > 0.5f;
        var imgToUse = currShowingRight ? rightImg : leftImg;
        imgToUse.gameObject.SetActive(true);
        imgToUse.sprite = spriteToShow;
    }

    private void Update()
    {
        if (gameoverFlag) return;

        if (Input.GetMouseButtonDown(0))
        {
            roundAnalytics.clicks++;
        }

        timerUI.value = timerPerChoice;
        timerPerChoice += Time.deltaTime;
        if (timerPerChoice >= gameConfigs.timePerChoice)
        {
            roundAnalytics.ranOutOfTime = true;
            timerPerChoice = 0;
            OnWrongChoice();
        }
    }

    private void OnClickedLeft()
    {
        var succeed = false;

        if (!currRequiresSameDirection && currShowingRight) succeed = true;
        if (currRequiresSameDirection && !currShowingRight) succeed = true;
        if (succeed) OnCorrectChoice();
        else OnWrongChoice();
    }

    private void OnClickedRight()
    {
        var succeed = false;

        if (currRequiresSameDirection && currShowingRight) succeed = true;
        if (!currRequiresSameDirection && !currShowingRight) succeed = true;
        if (succeed) OnCorrectChoice();
        else OnWrongChoice();
    }

    private void OnWrongChoice()
    {
        LIncorrectparticle.Stop();
        RIncorrectparticle.Stop();
        RCorrectparticle.Stop();
        LCorrectparticle.Stop();

        GeneralGameAnalyticsManager.RegisterLose();


        audiosource.clip = wrongAudio;
        audiosource.Play();
        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        if (currShowingRight) RIncorrectparticle.Play();
        else LIncorrectparticle.Play();
        gameUi.StarLost();


        OnRoundEnded();
    }

    private void OnCorrectChoice()
    {
        LIncorrectparticle.Stop();
        RIncorrectparticle.Stop();
        RCorrectparticle.Stop();
        LCorrectparticle.Stop();


		GeneralGameAnalyticsManager.RegisterWin();
		roundAnalytics.wonRound = true;

        audiosource.clip = correctAudio;
        audiosource.Play();
        currCoins += gameConfigs.coinsOnCorrectAnswer;
        Vector3 starPos;
        if (currShowingRight)
        {
            starPos = rightImg.transform.position;
            RCorrectparticle.Play();
        }
        else
        {
            starPos = leftImg.transform.position;
            LCorrectparticle.Play();
        }
        gameUi.StarEarned(starPos);
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        currRound++;

        if (currRequiresSameDirection) roundAnalytics.challengeOrder = "Same side";
        else roundAnalytics.challengeOrder = "Different side";

        roundAnalytics.timeToMakeAChoice = timerPerChoice;

        currCoinsValueTxt.text = currCoins.ToString();
        currRoundValueTxt.text = currRound.ToString();

        if (currRound >= gameConfigs.maxRounds)
        {
            GameOver();
            return;
        }

        Animator animatorImg = inGameUIPanel.GetComponent<Animator>();
        animatorImg.ResetTrigger("Appear");
        animatorImg.SetTrigger("Appear");

        InitRound();
    }

    void GameOver()
    {
        timerUI.gameObject.SetActive(false);
        audiosource.clip = finishAudio;
        audiosource.Play();
        gameoverFlag = true;
        inGameUIPanel.SetActive(false);
        afterActionPanel.SetActive(true);
        afterActionFinalCoinsTxt.SetText(currCoins.ToString());
        gameConfigs.SaveCoins(currCoins);
        eogManager.OnGameOver();
    }
}

public class MG_HearthAndStars_RoundAnalytics
{
    public string challengeOrder = "NONE";
    public bool wonRound = false;
    public float timeToMakeAChoice = 0;
    public int clicks = 0;
    public bool ranOutOfTime = false;
}
