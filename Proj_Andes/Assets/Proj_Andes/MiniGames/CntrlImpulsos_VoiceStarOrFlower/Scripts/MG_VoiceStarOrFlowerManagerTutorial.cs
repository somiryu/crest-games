using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_VoiceStarOrFlowerManagerTutorial : MonoBehaviour, IEndOfGameManager, ITimeManagement
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
    [SerializeField] AudioClip lookAtCampsAudio;
    [FormerlySerializedAs("firstInstrucAudio")]
    [SerializeField] AudioClip firstInstrucAudioForVoiceAsCorrectAnswer;
    [SerializeField] AudioClip firstInstrucAudioForImgAsCorrectAnswer;
    [SerializeField] AudioClip discardAdvice;
    [SerializeField] AudioClip youDidGoodAudio;
    [SerializeField] AudioClip youDidGoodNowTryAudio;
    [SerializeField] AudioClip onFailSelectDiscardAdvice;
    [SerializeField] AudioClip onFailSelectInstructionAdvice;
    [SerializeField] AudioClip finalInstruction;
    [SerializeField] AudioClip letsPlayAudio;
    [SerializeField] AudioSource effectPlayer;
    [SerializeField] AudioSource audioPlayer;

    [Header("Wrong choice instructions")]
	[SerializeField] AudioClip wrongAudioForImgAndCloud;
	[SerializeField] AudioClip wrongAudioForSoundAndCloud;
	[SerializeField] AudioClip wrongAudioForImgAndFlower;
	[SerializeField] AudioClip wrongAudioForSoundAndFlower;


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
    public bool UseVoiceAsCorrectAnswer => MG_VoiceStarOrFlowerGameConfigs.UseVoiceAsTheCorrectAnswer;

    private float timerPerChoice = 0;
    private int currCoins;

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
    bool canFailTutorial;
    int currScoreStepTutorial;
    int goalScoreStepTutorial;
    int trialsPerTutoCount;
    int failurePerTutoCount;

    public float overridenRoundTime = 5;
    float delayBetweenRounds = 1f;
    bool isPaused = false;

    public void Awake()
	{
        Init();
	}

	public void Init()
    {
        var useVoiceAsCorrectAnswer = Random.Range(0f, 1f) > 0.5f;

		MG_VoiceStarOrFlowerGameConfigs.UseVoiceAsTheCorrectAnswer = useVoiceAsCorrectAnswer;

        Debug.Log("Will use voice as correct answer: " + useVoiceAsCorrectAnswer);

        currStepTutorial = 0;
        intervalQuestion = true;
        currCoins = gameConfigs.initialCoins;

		afterActionPanel.SetActive(false);
		inGameUiPanel.SetActive(true);
        gameoverFlag = false;


        timerUI.minValue = 0;
        timerUI.maxValue = overridenRoundTime;

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
        isPaused = true;
        blockPanel.gameObject.SetActive(true);
        leftBtn.gameObject.SetActive(false);
        rightBtn.gameObject.SetActive(false);
        discardBtn.gameObject.SetActive(false);
        audioPlayer.clip = lookAtCampsAudio;
        audioPlayer.Play();
        yield return new WaitForSeconds(audioPlayer.clip.length - 0.3f);
        leftBtn.gameObject.SetActive(true);
		rightBtn.gameObject.SetActive(true);
		discardBtn.gameObject.SetActive(true);
        leftBtn.gameObject.SetActive(false);
        rightBtn.gameObject.SetActive(false);
        discardBtn.gameObject.SetActive(false);
        audioPlayer.clip = UseVoiceAsCorrectAnswer? firstInstrucAudioForVoiceAsCorrectAnswer : firstInstrucAudioForImgAsCorrectAnswer;
        audioPlayer.Play();
        yield return new WaitForSeconds(audioPlayer.clip.length);
        leftBtn.gameObject.SetActive(true);
        rightBtn.gameObject.SetActive(true);
        discardBtn.gameObject.SetActive(true);
        blockPanel.gameObject.SetActive(false);
        InitTutorialStep();
        InitRound();
		isPaused = false;
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
            hasButtonHelp = false;
            goalScoreStepTutorial = 3;
            hasWrongChoice = true;
            canFailTutorial = true;
			ResetScore();
		}
		if (currStepTutorial == 1)
		{
			MG_VoiceStarOrFlowerGameConfigs.passedTuto = true;
			StartCoroutine(CompleteTuto());
		}
		/*
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
			canFailTutorial = false;
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
			canFailTutorial = false;
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
			canFailTutorial = false;
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
			canFailTutorial = true;
            goalScoreStepTutorial = 5;
			ResetScore();
		}

        if (currStepTutorial == 5)
        {
			MG_VoiceStarOrFlowerGameConfigs.passedTuto = true;
			StartCoroutine(CompleteTuto());
        }
        */

		currScoreStepTutorial = 0;

        StarsCounter.SetActive(hasScore);
        discardBtn.gameObject.SetActive(hasDiscardButtton);
    }

    void Update()
    {
        if (isPaused) return;
		
        timerPerChoice += Time.deltaTime;
		timerUI.value = timerPerChoice;

		if (timerPerChoice >= overridenRoundTime)
		{
			timerPerChoice = 0;
			OnWrongChoice();
		}
	}


    IEnumerator CompleteTuto()
    {
        isPaused = true;
        effectPlayer.Stop();
        discardBtn.gameObject.SetActive(false);
        leftBtn.gameObject.SetActive(false);
        rightBtn.gameObject.SetActive(false);
        audioPlayer.clip = finalInstruction;
        audioPlayer.Play();
        blockPanel.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(finalInstruction.length);
        audioPlayer.clip = letsPlayAudio;
        audioPlayer.Play();
        yield return new WaitForSecondsRealtime(letsPlayAudio.length);
        UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.VoiceStarOrFlowerDone.ToString());
		blockPanel.gameObject.SetActive(false);
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
        if (currStepTutorial == 1) return;
        timerPerChoice = 0;

        trialsPerTutoCount++;

        audioPlayer.volume = 1;
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
            if (UseVoiceAsCorrectAnswer)
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
				if (!currSoundIsLeft && currImgIsLeft) SetButtonState(leftBtn, leftBtnHighlightImg, enabledBtnColor, true);
				else SetButtonState(leftBtn, leftBtnHighlightImg, disabledBtnColor, false);

				if (currSoundIsLeft && !currImgIsLeft) SetButtonState(rightBtn, rightBtnHighlightImg, enabledBtnColor, true);
				else SetButtonState(rightBtn, rightBtnHighlightImg, disabledBtnColor, false);

				if (currSoundIsLeft == currImgIsLeft) SetButtonState(discardBtn, discardBtnHighlightImg, enabledBtnColor, true);
				else SetButtonState(discardBtn, discardBtnHighlightImg, disabledBtnColor, false);
			}
            
        }
        else
        {
            SetButtonState(leftBtn, leftBtnHighlightImg, enabledBtnColor, false);
            SetButtonState(rightBtn, rightBtnHighlightImg, enabledBtnColor, false);
            SetButtonState(discardBtn, discardBtnHighlightImg, enabledBtnColor, false);
        }

        var imgToUse = currImgIsLeft ? leftTargetSprite : rightTargetSprite;
        var soundToUse = currSoundIsLeft ? leftAudio : rightAudio;


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
        incorrectParticles.Play();

        StartCoroutine(PlayOnWrongChoice());
    }


    IEnumerator PlayOnWrongChoice()
    {
        isPaused = true;
		rightBtn.interactable = false;
		leftBtn.interactable = false;
		discardBtn.interactable = false;

		effectPlayer.clip = wrongAudio;
		effectPlayer.Play();
		yield return new WaitForSeconds(delayBetweenRounds);

		if (hasWrongChoice)
        {
            AudioClip clipToPlay;

            if (UseVoiceAsCorrectAnswer)
            {
                if (currSoundIsLeft) clipToPlay = wrongAudioForSoundAndFlower;
                else clipToPlay = wrongAudioForSoundAndCloud;
            }
            else
            {
                if (currImgIsLeft) clipToPlay = wrongAudioForImgAndFlower;
                else clipToPlay = wrongAudioForImgAndCloud;
            }

            audioPlayer.clip = clipToPlay;
            audioPlayer.Play();
            yield return new WaitForSeconds(audioPlayer.clip.length);
        }

		rightBtn.interactable = true;
		leftBtn.interactable = true;
		discardBtn.interactable = true;
        isPaused = false;
		StartCoroutine(OnRoundEnded(0));
	}


	private void OnCorrectChoice()
    {
        currScoreStepTutorial += 1;
        failurePerTutoCount = 0;

        incorrectParticles.Stop();
        correctParticles.Stop();

		currCoins += gameConfigs.coinsOnCorrectAnswer;
        if (currSoundIsLeft && !currImgIsLeft)
        {
            leftWonItemsPool.GetNewItem();
        }
        else if(!currSoundIsLeft && currImgIsLeft)
        {
			rightWonItemsPool.GetNewItem();
        }

        correctParticles.Play();
        effectPlayer.volume = 0.8f;

        effectPlayer.clip = correctAudio;
        effectPlayer.Play();

        StartCoroutine(OnRoundEnded(delayBetweenRounds));
    }

    IEnumerator OnRoundEnded(float waitTime)
    {
        isPaused = true;
        discardBtn.interactable = false;
        leftBtn.interactable = false;
        rightBtn.interactable = false;

        yield return new WaitForSeconds(waitTime);

		discardBtn.interactable = true;
		leftBtn.interactable = true;
		rightBtn.interactable = true;

		currCoinsValueTxt.text = currCoins.ToString();

        if (currScoreStepTutorial == goalScoreStepTutorial)
        {
            currStepTutorial += 1;
            InitTutorialStep();
        }
        else if (canFailTutorial && failurePerTutoCount >= gameConfigs.finalTutoStepMaxFailuresBeforeSkipping)
        {
            MG_VoiceStarOrFlowerGameConfigs.passedTuto = false;
            StartCoroutine(CompleteTuto());
            yield break;
        }
		isPaused = false;
		InitRound();
    }
}
