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

    [SerializeField] GameObject afterActionPanel;
    [SerializeField] GameObject inGameUiPanel;
    [Space(20)]
    [Header("Game Audio")]
    [SerializeField] AudioClip correctAudio;
    [SerializeField] AudioClip wrongAudio;
    [SerializeField] AudioClip finishAudio;
    [SerializeField] AudioClip leftAudio;
    [SerializeField] AudioClip rightAudio;
    [SerializeField] AudioClip discardAudio;
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
    float totalGameTime;

    public void Awake()
	{
        Init();
	}

	public void Init()
    {
        currCoins = gameConfigs.initialCoins;
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
        GetRandomSoundImage();

        var imgToUse = currImgIsLeft ? leftTargetSprite: rightTargetSprite;
        var soundToUse = currSoundIsLeft ? leftAudio: rightAudio;
        var textToUse = currSoundIsLeft ? leftObjTxt: rightObjTxt;

        currTargetImg.sprite = imgToUse;
        audioPlayer.clip = soundToUse;
        audioPlayer.Play();
        eogManager.OnGameStart();
	}

	private void Update()
	{
        if (gameoverFlag) return;

        timerUI.value = timerPerChoice;
        totalGameTime += Time.deltaTime;
        timerPerChoice += Time.deltaTime;
        if (timerPerChoice >= gameConfigs.timePerChoice)
        {
            OnWrongChoice();
            timerPerChoice = 0;
        }
    }

	private void OnClickedLeft()
    {
        if(currSoundIsLeft && !currImgIsLeft) OnCorrectChoice();
        else OnWrongChoice();
    }

    private void OnClickedRight()
    {
		if (!currSoundIsLeft && currImgIsLeft) OnCorrectChoice();
		else OnWrongChoice();
	}

	private void OnClickedDiscard()
    {
        audioPlayer.PlayOneShot(discardAudio);
        if (currSoundIsLeft == currImgIsLeft) OnCorrectChoice();
		else OnWrongChoice();
	}


	private void OnWrongChoice()
    {
        incorrectParticles.Stop();
        correctParticles.Stop();

        gameConfigs.roundResultWins.Add(false);

        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        lostRoundsCount++;
        incorrectParticles.Play();
        audioPlayer.PlayOneShot(wrongAudio);

        OnRoundEnded();
    }

    private void OnCorrectChoice()
    {
        incorrectParticles.Stop();
        correctParticles.Stop();

        gameConfigs.roundResultWins.Add(true);

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

        correctParticles.Play();

        audioPlayer.PlayOneShot(correctAudio);
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        currCoinsValueTxt.text = currCoins.ToString();
        gameConfigs.timeToMakeAChoice.Add(timerPerChoice);

        var totalRounds = lostRoundsCount + wonLeftCount + wonRightCount;

        if (totalRounds >= gameConfigs.maxRounds)
        {
            GameOver();
            return;
        }

        InitRound();
    }

    void GameOver()
    {
        gameConfigs.totalGameTime = totalGameTime;
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
