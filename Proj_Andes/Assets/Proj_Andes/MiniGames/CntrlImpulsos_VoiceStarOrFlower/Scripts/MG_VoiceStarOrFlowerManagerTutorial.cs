using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_VoiceStarOrFlowerManagerTutorial : MonoBehaviour, IEndOfGameManager
{
	[SerializeField] MG_VoiceStarOrFlowerGameConfigs gameConfigs;
	[Space(20)]
	[SerializeField] Sprite leftTargetSprite;
    [SerializeField] Sprite rightTargetSprite;
    [Space(20)]
    [SerializeField] Image currTargetImg;
	[Space(20)]
	[SerializeField] Button leftBtn;
    [SerializeField] Image leftBtnHighlightImg;
    [SerializeField] Button rightBtn;
    [SerializeField] Image rightBtnHighlightImg;
    [SerializeField] Button discardBtn;
    [SerializeField] Image discardBtnHighlightImg;
    [SerializeField] Color disabledBtnColor;
    [SerializeField] Color enabledBtnColor;

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
    [SerializeField] GameObject StarsCounter;
    [SerializeField] TMP_Text currCoinsValueTxt;
    [SerializeField] TMP_Text afterActionFinalCoinsTxt;
    [SerializeField] Button retryBtn;
    [SerializeField] Button retryBtn2;
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

    private bool gameoverFlag = false;
    float totalGameTime;

    int currStepTutorial;
    bool intervalQuestion;
    bool soundAndImgSameQuestion;
    bool hasTimer;
    bool hasScore;
    bool hasDiscardButtton;
    bool hasButtonHelp;
    int currScoreStepTutorial;
    int goalScoreStepTutorial;

    public void Awake()
	{
        Init();
	}

	public void Init()
    {
        currStepTutorial = 0;
        intervalQuestion = true;
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

        retryBtn2.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single));
        
        InitTutorialStep();

        InitRound();
	}


    private void InitTutorialStep()
    {
        Debug.Log("tutorial" + currStepTutorial);

        if (currStepTutorial == 0)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = true;
            soundAndImgSameQuestion = false;
            hasDiscardButtton = false;
            hasButtonHelp = true;
            goalScoreStepTutorial = 2;
            
        }

        if (currStepTutorial == 1)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = false;
            soundAndImgSameQuestion = false;
            hasDiscardButtton = false;
            hasButtonHelp = true;
            goalScoreStepTutorial = 2;
        }

        if (currStepTutorial == 2)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = true;
            soundAndImgSameQuestion = true;
            hasDiscardButtton = true;
            hasButtonHelp = true;
            goalScoreStepTutorial = 2;
        }

        if (currStepTutorial == 3)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = false;
            soundAndImgSameQuestion = false;
            hasDiscardButtton = true;
            hasButtonHelp = true;
            goalScoreStepTutorial = 2;
        }

        if (currStepTutorial == 4)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = false;
            soundAndImgSameQuestion = false;
            hasDiscardButtton = true;
            hasButtonHelp = false;
            goalScoreStepTutorial = 5;
        }

        if (currStepTutorial == 5)
        {
            //load new scene
        }

        currScoreStepTutorial = 0;

        timerUI.gameObject.SetActive(hasTimer);
        StarsCounter.SetActive(hasScore);
        discardBtn.gameObject.SetActive(hasDiscardButtton);
        
    }

    void InitRound()
    {
        timerPerChoice = 0;
    

        if (intervalQuestion)
        {
            currSoundIsLeft = !currSoundIsLeft;
            if (soundAndImgSameQuestion) currImgIsLeft = currSoundIsLeft;
            else currImgIsLeft = !currSoundIsLeft;
        }
        else
        {
            if (hasDiscardButtton)
            {
                currSoundIsLeft = Random.Range(0f, 1f) > 0.5f;
                currImgIsLeft = Random.Range(0f, 1f) > 0.5f;
            }
            else
            {
                currSoundIsLeft = Random.Range(0f, 1f) > 0.5f;
                currImgIsLeft = !currSoundIsLeft;
            }
        }

        if (hasButtonHelp)
        {
            if (currSoundIsLeft && !currImgIsLeft) SetButtonState(leftBtn, leftBtnHighlightImg, enabledBtnColor, true);
            else SetButtonState(leftBtn, leftBtnHighlightImg, disabledBtnColor, false);
           
            if (!currSoundIsLeft && currImgIsLeft) SetButtonState(rightBtn, rightBtnHighlightImg, enabledBtnColor, true);
            else SetButtonState(rightBtn, rightBtnHighlightImg, disabledBtnColor, false);

            if (currSoundIsLeft == currImgIsLeft) SetButtonState(discardBtn, discardBtnHighlightImg, enabledBtnColor, true);
            else SetButtonState(discardBtn, discardBtnHighlightImg, disabledBtnColor, false);
        }
        else
        {
            SetButtonState(leftBtn, leftBtnHighlightImg, enabledBtnColor, false);
            SetButtonState(rightBtn, rightBtnHighlightImg, enabledBtnColor, false);
            SetButtonState(discardBtn, discardBtnHighlightImg, enabledBtnColor, false);
        }


        var imgToUse = currImgIsLeft ? leftTargetSprite : rightTargetSprite;
        var soundToUse = currSoundIsLeft ? leftAudio : rightAudio;
        var textToUse = currSoundIsLeft ? leftObjTxt : rightObjTxt;

        currTargetImg.sprite = imgToUse;
        audioPlayer.clip = soundToUse;
        audioPlayer.Play();
        eogManager.OnGameStart();
    }

    void SetButtonState(Button button, Image highlightImg, Color color, bool highlight)
    {
        button.image.color = color;
        highlightImg.gameObject.SetActive(highlight);
    }
    private void Update()
	{
        if (gameoverFlag) return;
        if (!hasTimer) return;

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
        audioPlayer.clip = discardAudio;
        audioPlayer.Play();
        if (currSoundIsLeft == currImgIsLeft) OnCorrectChoice();
		else OnWrongChoice();
	}


	private void OnWrongChoice()
    {
        currScoreStepTutorial = 0;

        incorrectParticles.Stop();
        correctParticles.Stop();

        gameConfigs.roundResultWins.Add(false);

        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        lostRoundsCount++;
        audioPlayer.clip = wrongAudio;
        incorrectParticles.Play();
        audioPlayer.Play();
        OnRoundEnded();
    }

    private void OnCorrectChoice()
    {
        currScoreStepTutorial += 1;

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

        audioPlayer.clip = rightAudio;
        audioPlayer.Play();
        OnRoundEnded();
    }

    void OnRoundEnded()
    {        
        currCoinsValueTxt.text = currCoins.ToString();
        gameConfigs.timeToMakeAChoice.Add(timerPerChoice);

        if (lostRoundsCount >= gameConfigs.maxRounds ||
            wonLeftCount >= gameConfigs.maxRounds ||
            wonRightCount >= gameConfigs.maxRounds)
        {
            GameOver();
            return;
        }

        if (currScoreStepTutorial == goalScoreStepTutorial)
        {
            currStepTutorial += 1;
            InitTutorialStep();
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
