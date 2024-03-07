using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] AudioClip lookAtCampsAudio;
    [SerializeField] AudioClip firstInstrucAudio;
    [SerializeField] AudioClip discardAdvice;
    [SerializeField] AudioClip youDidGoodAudio;
    [SerializeField] AudioClip youDidGoodNowTryAudio;
    [SerializeField] AudioClip onFailSelectDiscardAdvice;
    [SerializeField] AudioClip onFailSelectInstructionAdvice;
    [SerializeField] AudioClip finalInstruction;
    [SerializeField] AudioSource effectPlayer;
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
    [SerializeField] Slider timerUI;
    [SerializeField] Transform blockPanel;

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
    bool hasWrongChoice;
    int currScoreStepTutorial;
    int goalScoreStepTutorial;
    int trialsPerTutoCount;
    int failurePerTutoCount;

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


        timerUI.minValue = 0;
        timerUI.maxValue = gameConfigs.timePerChoice;

		leftBtn.onClick.AddListener(OnClickedLeft);
		rightBtn.onClick.AddListener(OnClickedRight);
		discardBtn.onClick.AddListener(OnClickedDiscard);


        leftWonItemsPool.Init(gameConfigs.maxRounds);
        rightWonItemsPool.Init(gameConfigs.maxRounds);

        audioPlayer.Play();
        eogManager.OnGameStart();

        StartCoroutine(InitTutorialAfterTime());
	}

    IEnumerator InitTutorialAfterTime()
    {
        blockPanel.gameObject.SetActive(true);
        leftBtn.gameObject.SetActive(false);
        rightBtn.gameObject.SetActive(false);
        discardBtn.gameObject.SetActive(false);
        audioPlayer.clip = lookAtCampsAudio;
        audioPlayer.Play();
        yield return new WaitForSeconds(lookAtCampsAudio.length-0.3f);
        leftBtn.gameObject.SetActive(true);
		rightBtn.gameObject.SetActive(true);
		discardBtn.gameObject.SetActive(true);
        leftBtn.gameObject.SetActive(false);
        rightBtn.gameObject.SetActive(false);
        discardBtn.gameObject.SetActive(false);
        audioPlayer.clip = firstInstrucAudio;
        audioPlayer.Play();
        yield return new WaitForSeconds(firstInstrucAudio.length);
        leftBtn.gameObject.SetActive(true);
        rightBtn.gameObject.SetActive(true);
        discardBtn.gameObject.SetActive(true);
        blockPanel.gameObject.SetActive(false);
        InitTutorialStep();
        InitRound();
    }

    private void InitTutorialStep()
    {
        trialsPerTutoCount = 0;
        failurePerTutoCount = 0;

        if (currStepTutorial == 0)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = true;
            soundAndImgSameQuestion = false;
            hasDiscardButtton = false;
            hasButtonHelp = true;
            goalScoreStepTutorial = 4;
            hasWrongChoice = false;
			ResetScore();
		}

		if (currStepTutorial == 1)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = false;
            soundAndImgSameQuestion = false;
            hasDiscardButtton = false;
            hasButtonHelp = true;
            goalScoreStepTutorial = 6;
            hasWrongChoice = false;
            ResetScore();
        }

        if (currStepTutorial == 2)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = true;
            soundAndImgSameQuestion = true;
            hasDiscardButtton = true;
            hasButtonHelp = true;
            hasWrongChoice = false;
            goalScoreStepTutorial = 4;
			ResetScore();
		}

		if (currStepTutorial == 3)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = false;
            soundAndImgSameQuestion = false;
            hasDiscardButtton = true;
            hasButtonHelp = true;
            hasWrongChoice = false;
            goalScoreStepTutorial = 4;
			ResetScore();
		}

		if (currStepTutorial == 4)
        {
            hasTimer = false;
            hasScore = false;
            intervalQuestion = false;
            soundAndImgSameQuestion = false;
            hasDiscardButtton = true;
            hasWrongChoice = true;
            hasButtonHelp = false;
            goalScoreStepTutorial = 5;
			ResetScore();
		}

		if (currStepTutorial == 5) StartCoroutine( CompleteTuto());

        currScoreStepTutorial = 0;

        timerUI.gameObject.SetActive(hasTimer);
        StarsCounter.SetActive(hasScore);
        discardBtn.gameObject.SetActive(hasDiscardButtton);
    }
    IEnumerator CompleteTuto()
    {
        discardBtn.gameObject.SetActive(false);
        leftBtn.gameObject.SetActive(false);
        rightBtn.gameObject.SetActive(false);
        audioPlayer.clip = finalInstruction;
        audioPlayer.Play();
        yield return new WaitForSeconds(finalInstruction.length);
        UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.VoiceStarOrFlowerDone.ToString());
        GameSequencesList.Instance.GoToNextItemInList();
    }
    void ResetScore()
    {
        leftWonItemsPool.RecycleAll();
        rightWonItemsPool.RecycleAll();
        currCoins = 0;
        currCoinsValueTxt.SetText(0.ToString());
    }

    void InitRound()
    {
        timerPerChoice = 0;

        trialsPerTutoCount++;

        audioPlayer.volume = 1;
        Debug.Log(currStepTutorial + " " + trialsPerTutoCount);
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

        effectPlayer.clip = soundToUse;

        if (currStepTutorial == 2 && trialsPerTutoCount == 1) StartCoroutine(DiscardInstruction());
        else if (currStepTutorial == 4 && trialsPerTutoCount == 1) StartCoroutine(PlaySingleAudio(youDidGoodNowTryAudio));
        else effectPlayer.Play();   
    }
    IEnumerator DiscardInstruction()
    {
        blockPanel.gameObject.SetActive(true);
        discardBtn.interactable = false;
        rightBtn.interactable = false;
        leftBtn.interactable = false;
        audioPlayer.clip = youDidGoodAudio;
        audioPlayer.Play();
        yield return new WaitForSeconds(youDidGoodAudio.length);
        audioPlayer.clip = discardAdvice;
        audioPlayer.Play();
        yield return new WaitForSeconds(discardAdvice.length);
        blockPanel.gameObject.SetActive(false);
        discardBtn.interactable = true;
        rightBtn.interactable = true;
        leftBtn.interactable = true;
        effectPlayer.Play();
    }
    IEnumerator PlaySingleAudio(AudioClip audioClip)
    {
        blockPanel.gameObject.SetActive(true);
        discardBtn.interactable = false;
        rightBtn.interactable = false;
        leftBtn.interactable = false;
        audioPlayer.clip = audioClip;
        audioPlayer.Play();
        yield return new WaitForSeconds(audioClip.length);
        discardBtn.interactable = true;
        rightBtn.interactable = true;
        leftBtn.interactable = true;
        effectPlayer.Play();
        blockPanel.gameObject.SetActive(false);
    }

    void SetButtonState(Button button, Image highlightImg, Color color, bool highlight)
    {
        button.image.color = color;
        highlightImg.gameObject.SetActive(highlight);
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
        currScoreStepTutorial = 0;
        failurePerTutoCount++;
        incorrectParticles.Stop();
        correctParticles.Stop();

        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        lostRoundsCount++;
        incorrectParticles.Play();

        audioPlayer.volume = 0.8f;

        effectPlayer.clip = wrongAudio;
        effectPlayer.Play();

        if (!hasWrongChoice) return;

        StartCoroutine(PlayOnWrongChoice());
    }


    IEnumerator PlayOnWrongChoice()
    {
        AudioClip clipToPlay = onFailSelectInstructionAdvice;
        if (currImgIsLeft && currSoundIsLeft || !currImgIsLeft && !currSoundIsLeft) clipToPlay = onFailSelectDiscardAdvice;
        else clipToPlay = onFailSelectInstructionAdvice;

        audioPlayer.clip = clipToPlay;
        audioPlayer.Play();
        var timeToWait = clipToPlay.length;
		rightBtn.interactable = false;
		leftBtn.interactable = false;
		discardBtn.interactable = false;

		yield return new WaitForSeconds(timeToWait);

		rightBtn.interactable = true;
		leftBtn.interactable = true;
		discardBtn.interactable = true;
		OnRoundEnded();
	}


    private void OnCorrectChoice()
    {
        currScoreStepTutorial += 1;

        incorrectParticles.Stop();
        correctParticles.Stop();

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
        effectPlayer.volume = 0.8f;

        effectPlayer.clip = correctAudio;
        effectPlayer.Play();

        OnRoundEnded();
    }

    void OnRoundEnded()
    {        
        currCoinsValueTxt.text = currCoins.ToString();

        if (currScoreStepTutorial == goalScoreStepTutorial)
        {
            currStepTutorial += 1;
            InitTutorialStep();
        }
        else if (currStepTutorial == 4 && failurePerTutoCount >= gameConfigs.finalTutoStepMaxFailuresBeforeSkipping)
        {
            CompleteTuto();
            return;
        }
        InitRound();
    }
}
