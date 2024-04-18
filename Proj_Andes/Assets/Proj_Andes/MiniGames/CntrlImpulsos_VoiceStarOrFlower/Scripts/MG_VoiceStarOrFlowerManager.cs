using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_VoiceStarOrFlowerManager : MonoBehaviour, IEndOfGameManager
{
    static MG_VoiceStarOrFlowerManager instance; 
    public static MG_VoiceStarOrFlowerManager Instance => instance;

	[SerializeField] MG_VoiceStarOrFlowerGameConfigs gameConfigs;
	[Space(20)]
	[SerializeField] Sprite leftTargetSprite;
    [SerializeField] Sprite rightTargetSprite;
    [Space(20)]
    [SerializeField] Image currTargetImg;
	[Space(20)]
	[SerializeField] Button leftBtn;
    [SerializeField] Button rightBtn;
    [SerializeField] Button discardBtn;
    [Space(20)]
    public static int currTutoStepIdx;
    public List<MG_VoiceStarOrFlowerGameConfigs> gameTypeConfigs = new List<MG_VoiceStarOrFlowerGameConfigs>();
    MG_VoiceStarOrFlowerGameConfigs currGameTypeConfig => gameTypeConfigs[currTutoStepIdx];
    [Space(20)]
    [SerializeField] GameObject afterActionPanel;
    [SerializeField] GameObject inGameUiPanel;
    [Space(20)]
    [Header("Game Audio")]
    [SerializeField] AudioClip correctAudio;
    [SerializeField] AudioClip wrongAudio;
    [SerializeField] AudioClip finishAudio;
    [SerializeField] AudioClip leftAudio;
    [SerializeField] AudioClip rightAudio;
    [SerializeField] AudioSource audioPlayer;
    [Space(20)]
    [SerializeField] Pool<Transform> leftWonItemsPool;
    [SerializeField] Pool<Transform> rightWonItemsPool;

    [Header("GameParticles")]
    [SerializeField] ParticleSystem correctParticles;
    [SerializeField] ParticleSystem incorrectParticles;

    [Header("UI")]
    [SerializeField] TMP_Text leftObjTxt;
    [SerializeField] TMP_Text rightObjTxt;
    [SerializeField] TMP_Text currCoinsValueTxt;
    [SerializeField] TMP_Text afterActionFinalCoinsTxt;
    [SerializeField] Button retryBtn;
    [SerializeField] Slider timerUI;

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;

    private float timerPerChoice = 0;
    private int currCoins;
    private int wonLeftCount;
    private int wonRightCount;
    private int lostRoundsCount;

    private bool currImgIsLeft = false;
    private bool currSoundIsLeft = false;
    
    private int amountImgIsLeft = 0;
    private int amountSoundIsLeft = 0;
    private int amountDiscardButton = 0;

    private bool gameoverFlag = false;
    private bool isPaused = false;
    float totalGameTime;

    MG_FieldOfFlowers_RoundAnalytics roundAnalytics;
    public List<MG_FieldOfFlowers_RoundAnalytics> AllRoundsAnalytics;

	public bool UseVoiceAsCorrectAnswer => MG_VoiceStarOrFlowerGameConfigs.UseVoiceAsTheCorrectAnswer;


	public void Awake()
	{
        if (instance != null && instance != this) DestroyImmediate(this);
        instance = this;
        Init();
	}

	public void Init()
    {
        var useVoiceAsCorrectAnswer = true;

        switch (currGameTypeConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                useVoiceAsCorrectAnswer = true;
                break;
            case VoiceOrImageGameType.Image:
                useVoiceAsCorrectAnswer = false;
                break;
            case VoiceOrImageGameType.Mixed:
                useVoiceAsCorrectAnswer = MG_VoiceStarOrFlowerGameConfigs.UseVoiceAsTheCorrectAnswer;
                break;
        }
        Debug.Log("starting " + currGameTypeConfig.gameType);
        MG_VoiceStarOrFlowerGameConfigs.UseVoiceAsTheCorrectAnswer = useVoiceAsCorrectAnswer;
        currCoins = gameConfigs.initialCoins;
        AllRoundsAnalytics = new List<MG_FieldOfFlowers_RoundAnalytics>(gameConfigs.maxRounds);

        wonLeftCount = 0;
        wonRightCount = 0;
        lostRoundsCount = 0;

		afterActionPanel.SetActive(false);
		inGameUiPanel.SetActive(true);
        gameoverFlag = false;

        gameConfigs.ResetCurrentAnalytics();

        timerUI.minValue = 0;
        timerUI.maxValue = gameConfigs.timePerChoice;

		leftBtn.onClick.AddListener(OnClickedLeft);
		rightBtn.onClick.AddListener(OnClickedRight);
		discardBtn.onClick.AddListener(OnClickedDiscard);


        leftWonItemsPool.Init(gameConfigs.maxRounds);
        rightWonItemsPool.Init(gameConfigs.maxRounds);

        InitRound();
	}

	private void Start()
	{
        GeneralGameAnalyticsManager.Instance.Init(DataIds.voiceStarGame);
	}

	int repeatedPuzzleCounter = 0;
    int flowersAppearedCount = 0;
    int cloudsAppearedCount = 0;

    void GetRandomSoundImage()
    {
        var previousSoundIsLeft = currSoundIsLeft;

        currSoundIsLeft = Random.Range(0f, 1f) > 0.5f;

        if (currSoundIsLeft && flowersAppearedCount >= gameConfigs.maxRounds / 2)
        {
            currSoundIsLeft = false;
        }
        if (!currSoundIsLeft && cloudsAppearedCount >= gameConfigs.maxRounds / 2)
        {
            currSoundIsLeft = true;
        }

        if (currSoundIsLeft) flowersAppearedCount++;
        else cloudsAppearedCount++;

        currImgIsLeft = !currSoundIsLeft;
    }

	void InitRound()
    {
        timerPerChoice = 0;
        
        GetRandomSoundImage();
        roundAnalytics = new MG_FieldOfFlowers_RoundAnalytics();

        AllRoundsAnalytics.Add(roundAnalytics);
        roundAnalytics.clicks = 0;
        var imgToUse = currImgIsLeft ? leftTargetSprite: rightTargetSprite;
        var soundToUse = currSoundIsLeft ? leftAudio: rightAudio;
        var textToUse = currSoundIsLeft ? leftObjTxt: rightObjTxt;

      

        if (currSoundIsLeft) roundAnalytics.audio = "Flower";
        else roundAnalytics.audio = "Star";
        if (currImgIsLeft) roundAnalytics.image = "Flower";
        else roundAnalytics.image = "Star";
        if (currImgIsLeft && currSoundIsLeft || !currSoundIsLeft && !currSoundIsLeft) roundAnalytics.challengeType = "Same";
        else roundAnalytics.challengeType = "Different";

        currTargetImg.sprite = imgToUse;
        audioPlayer.clip = soundToUse;
        audioPlayer.Play();
        eogManager.OnGameStart();
	}

	private void Update()
	{
        if (gameoverFlag) return;
        if (isPaused) return;

        timerUI.value = timerPerChoice;
        totalGameTime += Time.deltaTime;
        timerPerChoice += Time.deltaTime;
        if (Input.GetMouseButtonDown(0)) roundAnalytics.clicks++;
        if (timerPerChoice >= gameConfigs.timePerChoice)
        {
            roundAnalytics.ranOutOfTime = true;
            timerPerChoice = 0;
            OnWrongChoice();
        }
    }

	private void OnClickedLeft()
    {
        roundAnalytics.ranOutOfTime = false;
		if (UseVoiceAsCorrectAnswer)
		{
			if (currSoundIsLeft && !currImgIsLeft) OnCorrectChoice();
			else OnWrongChoice();
		}
		else //Img is correct answer
		{
			if (!currSoundIsLeft && currImgIsLeft) OnCorrectChoice();
			else OnWrongChoice();
		}
	}

    private void OnClickedRight()
    {
        roundAnalytics.ranOutOfTime = false;
		if (UseVoiceAsCorrectAnswer)
		{
			if (!currSoundIsLeft && currImgIsLeft) OnCorrectChoice();
			else OnWrongChoice();
		}
		else //Img is correct answer
		{
			if (currSoundIsLeft && !currImgIsLeft) OnCorrectChoice();
			else OnWrongChoice();
		}
	}

	private void OnClickedDiscard()
    {
        roundAnalytics.ranOutOfTime = false;
        if (currSoundIsLeft == currImgIsLeft) OnCorrectChoice();
		else OnWrongChoice();
	}


	private void OnWrongChoice()
    {
        GeneralGameAnalyticsManager.RegisterLose();
        incorrectParticles.Stop();
        correctParticles.Stop();

        roundAnalytics.wonRound = false;

        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        lostRoundsCount++;
        incorrectParticles.Play();
        audioPlayer.PlayOneShot(wrongAudio);
        GameUIController.Instance.StarLost();

        StartCoroutine(OnRoundEnded(1f));
    }

    private void OnCorrectChoice()
    {
		GeneralGameAnalyticsManager.RegisterWin();
		incorrectParticles.Stop();
        correctParticles.Stop();

        roundAnalytics.wonRound = true;

        currCoins += gameConfigs.coinsOnCorrectAnswer;
        if (currSoundIsLeft && !currImgIsLeft)
        {
            leftWonItemsPool.GetNewItem();
            
            wonLeftCount++;
        }
        else if(!currSoundIsLeft && currImgIsLeft)
        {
			rightWonItemsPool.GetNewItem();
			wonRightCount++;
        }

        GameUIController.Instance.StarEarned(currTargetImg.transform.position);
        correctParticles.Play();

        audioPlayer.PlayOneShot(correctAudio);
        StartCoroutine(OnRoundEnded(1f));
    }

    IEnumerator OnRoundEnded(float waitTime)
    {
        isPaused = true;
        leftBtn.interactable = false;
        rightBtn.interactable = false;
        discardBtn.interactable = false;

        yield return new WaitForSeconds(waitTime);

		leftBtn.interactable = true;
		rightBtn.interactable = true;
		discardBtn.interactable = true;


		currCoinsValueTxt.text = currCoins.ToString();
        roundAnalytics.timeToMakeAChoice = timerPerChoice;
        if (roundAnalytics.ranOutOfTime) roundAnalytics.timeToMakeAChoice = gameConfigs.timePerChoice;


        var totalRounds = lostRoundsCount + wonLeftCount + wonRightCount;

        if (totalRounds >= gameConfigs.maxRounds)
        {
            GameOver();
            yield break;
        }
		isPaused = false;
		InitRound();
    }

    void GameOver()
    {
        gameConfigs.SaveAnalytics();
        audioPlayer.clip = finishAudio;
        audioPlayer.Play();
        gameoverFlag = true;
        afterActionPanel.SetActive(true);
        inGameUiPanel.SetActive(false);
		afterActionFinalCoinsTxt.SetText(currCoins.ToString());
        gameConfigs.SaveCoins(currCoins);
        eogManager.OnGameOver();
	}
}
public class MG_FieldOfFlowers_RoundAnalytics
{
    public string challengeType = "NONE";
    public string image;
    public string audio;
    public bool wonRound = false;
    public float timeToMakeAChoice = 0;
    public int clicks = 0;
    public bool ranOutOfTime = false;
}