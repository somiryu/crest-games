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
    bool UseVoiceAsCorrectAnswer => MG_VoiceStarOrFlowerGameConfigs.UseVoiceAsTheCorrectAnswer;
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
    [SerializeField] Transform blockingPanel;

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;

    private float timerPerChoice = 0;
    private int currCoins;
    private int wonLeftCount;
    private int wonRightCount;
    private int lostRoundsCount;
    int roundCount = 0;

    int cloudCount;
    int flowerCount;

    private bool currImgIsLeft = false;
    private bool currSoundIsLeft = false;
    
    private int amountImgIsLeft = 0;
    private int amountSoundIsLeft = 0;
    private int amountDiscardButton = 0;

    bool isPaused;
    private bool gameoverFlag = false;
    float totalGameTime;

    MG_FieldOfFlowers_RoundAnalytics roundAnalytics;
    public List<MG_FieldOfFlowers_RoundAnalytics> AllRoundsAnalytics;

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

        flowerCount = 0;
        cloudCount = 0;

        leftWonItemsPool.Init(gameConfigs.maxRounds);
        rightWonItemsPool.Init(gameConfigs.maxRounds);
        eogManager.OnGameStart();

        InitRound();
	}

	private void Start()
	{
        GeneralGameAnalyticsManager.Instance.Init(DataIds.voiceStarGame);
	}

	int repeatedPuzzleCounter = 0;

    void GetRandomSoundImage()
    {
        var previousSoundIsLeft = currSoundIsLeft;
        var previousImgIsLeft = currImgIsLeft;

        currSoundIsLeft = Random.Range(0f, 1f) > 0.5f;
        currImgIsLeft = Random.Range(0f, 1f) > 0.5f;

        if(previousSoundIsLeft == currSoundIsLeft &&  previousImgIsLeft == currImgIsLeft)
        {
            repeatedPuzzleCounter++;
            if(repeatedPuzzleCounter >= 2)
            {
                GetRandomSoundImage();
                return;
            }
        }
        else
        {
            repeatedPuzzleCounter = 0;
        }
    }

	void InitRound()
    {
        timerPerChoice = 0;
        roundCount++;

        GetRandomSoundImage();
        roundAnalytics = new MG_FieldOfFlowers_RoundAnalytics();

        AllRoundsAnalytics.Add(roundAnalytics);

        switch (currGameTypeConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                if (cloudCount > currGameTypeConfig.maxRounds / 2) currSoundIsLeft = false;
                else if (flowerCount > currGameTypeConfig.maxRounds / 2) currSoundIsLeft = true;
                if (currSoundIsLeft) flowerCount++;
                else cloudCount++;

                currTargetImg.gameObject.SetActive(false);
                currImgIsLeft = currSoundIsLeft;
                break;
            case VoiceOrImageGameType.Image:
                if (cloudCount > currGameTypeConfig.maxRounds / 2) currImgIsLeft = false;
                else if (flowerCount > currGameTypeConfig.maxRounds / 2) currImgIsLeft = true;
                if (currImgIsLeft) flowerCount++;
                else cloudCount++;
                audioPlayer.clip = null;
                currSoundIsLeft = currImgIsLeft;
                currTargetImg.gameObject.SetActive(true);
                break;
            case VoiceOrImageGameType.Mixed:
                break;
        

        }

        var imgToUse = currImgIsLeft ? leftTargetSprite : rightTargetSprite;
        var soundToUse = currSoundIsLeft ? leftAudio : rightAudio;
        var textToUse = currSoundIsLeft ? leftObjTxt : rightObjTxt;

        currTargetImg.sprite = imgToUse;
        audioPlayer.clip = soundToUse;

        if(currGameTypeConfig.gameType != VoiceOrImageGameType.Image) audioPlayer.Play();

        if (currSoundIsLeft) roundAnalytics.audio = 1;
        else roundAnalytics.audio = 0;
        if (currImgIsLeft) roundAnalytics.image = 1;
        else roundAnalytics.image = 0;
    }

	private void Update()
	{
        if (gameoverFlag) return;

        timerUI.value = timerPerChoice;
        totalGameTime += Time.deltaTime;
        timerPerChoice += Time.deltaTime;
        if (timerPerChoice >= gameConfigs.timePerChoice)
        {
            timerPerChoice = 0;
            OnWrongChoice();
        }
    }

	private void OnClickedLeft()
    {
        roundAnalytics.selection = 1;
        switch (currGameTypeConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                if (currSoundIsLeft && !currGameTypeConfig.testIsOppositeToStimuli) OnCorrectChoice();
                else OnWrongChoice();
                break;
            case VoiceOrImageGameType.Image:
                if (currImgIsLeft && !currGameTypeConfig.testIsOppositeToStimuli) OnCorrectChoice();
                else OnWrongChoice();
                break;
            case VoiceOrImageGameType.Mixed:
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
                break;
        }
    }

    private void OnClickedRight()
    {
        roundAnalytics.selection = 0;
        switch (currGameTypeConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                if (!currSoundIsLeft && currGameTypeConfig.testIsOppositeToStimuli) OnCorrectChoice();
                else OnWrongChoice();
                break;
            case VoiceOrImageGameType.Image:
                if (!currImgIsLeft && currGameTypeConfig.testIsOppositeToStimuli) OnCorrectChoice();
                else OnWrongChoice();
                break;
            case VoiceOrImageGameType.Mixed:
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
                break;
        }
    }

	private void OnClickedDiscard()
    {
        if (currSoundIsLeft == currImgIsLeft) OnCorrectChoice();
		else OnWrongChoice();
	}


	private void OnWrongChoice()
    {
        GeneralGameAnalyticsManager.RegisterLose();
        incorrectParticles.Stop();
        correctParticles.Stop();

        roundAnalytics.wonRound = 0;

        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        lostRoundsCount++;
        incorrectParticles.Play();
        audioPlayer.PlayOneShot(wrongAudio);
        GameUIController.Instance.StarLost();

        StartCoroutine(OnRoundEnded(wrongAudio.length));
        StartCoroutine(OnRoundEnded(currGameTypeConfig.intermediateRoundHold));
    }

    private void OnCorrectChoice()
    {
		GeneralGameAnalyticsManager.RegisterWin();
		incorrectParticles.Stop();
        correctParticles.Stop();

        roundAnalytics.wonRound = 1;

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
        StartCoroutine(OnRoundEnded(currGameTypeConfig.intermediateRoundHold));
    }

    IEnumerator OnRoundEnded(float waitTime)
    {
        blockingPanel.gameObject.SetActive(true);
        isPaused = true;
        leftBtn.interactable = false;
        rightBtn.interactable = false;
        discardBtn.interactable = false;

        yield return new WaitForSeconds(waitTime);

		leftBtn.interactable = true;
		rightBtn.interactable = true;
		discardBtn.interactable = true;

        blockingPanel.gameObject.SetActive(false);

        currCoinsValueTxt.text = currCoins.ToString();
        roundAnalytics.timeToMakeAChoice = timerPerChoice;


        var totalRounds = lostRoundsCount + wonLeftCount + wonRightCount;

        if (totalRounds >= gameConfigs.maxRounds)
        {
            GameOver();
            yield break;
        }

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
    public int roundCount = 0;
    public int image = 0;
    public int audio = 0;
    public int selection = 0;
    public int wonRound = 0;
    public float timeToMakeAChoice = 0;
}