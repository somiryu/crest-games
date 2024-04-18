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
    static MG_VoiceStarOrFlowerManagerTutorial instance;
    public static MG_VoiceStarOrFlowerManagerTutorial Instance => instance;

    [SerializeField] MG_VoiceStarOrFlowerGameConfigs gameConfigs;
    [Space(20)]
    public static int currTutoStepIdx;
    public List<VoiceOrImageTutoConfig> tutoConfigs = new List<VoiceOrImageTutoConfig>();
    VoiceOrImageTutoConfig currTutoConfig => tutoConfigs[currTutoStepIdx];
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
    private int wonLeftCount;
    private int wonRightCount;
    private int lostRoundsCount;
    IEnumerator currInstruction;
    private bool currImgIsLeft = false;
    private bool currSoundIsLeft = false;

    private bool gameoverFlag = false;

    bool hasWrongChoice;
    int consecutiveWinsTuto;
    int goalScoreStepTutorial;
    int trialsPerTutoCount;
    int failurePerTutoCount;
    int tutoStage;
    bool isPaused;

    public void Awake()
	{
        if (instance != null && instance != this) DestroyImmediate(this);
        instance = this;
        Init();
	}

	public void Init()
    {
        var useVoiceAsCorrectAnswer = true;

        switch (currTutoConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                useVoiceAsCorrectAnswer = true;
                break;
            case VoiceOrImageGameType.Image:
                useVoiceAsCorrectAnswer = false;
                break;
            case VoiceOrImageGameType.Mixed:
                useVoiceAsCorrectAnswer = Random.Range(0f, 1f) > 0.5f;
                break;
        }

        MG_VoiceStarOrFlowerGameConfigs.UseVoiceAsTheCorrectAnswer = useVoiceAsCorrectAnswer;

        Debug.Log("Will use voice as correct answer: " + useVoiceAsCorrectAnswer);

        currCoins = gameConfigs.initialCoins;
        wonLeftCount = 0;
        wonRightCount = 0;
        lostRoundsCount = 0;
        tutoStage = 0;
        currTutoConfig.completedFirstPart = false;

		afterActionPanel.SetActive(false);
		inGameUiPanel.SetActive(true);
        gameoverFlag = false;


        timerUI.minValue = 0;
        timerUI.maxValue = currTutoConfig.roundTime;

		leftBtn.onClick.AddListener(OnClickedLeft);
		rightBtn.onClick.AddListener(OnClickedRight);
		discardBtn.onClick.AddListener(OnClickedDiscard);

        discardBtn.gameObject.SetActive(false);
        leftWonItemsPool.Init(gameConfigs.maxRounds);
        rightWonItemsPool.Init(gameConfigs.maxRounds);

        audioPlayer.Play();
        eogManager.OnGameStart();

        InitTutorialStep();

    }
    private void InitTutorialStep()
    {
        // audioPlayer.clip = UseVoiceAsCorrectAnswer ? firstInstrucAudioForVoiceAsCorrectAnswer : firstInstrucAudioForImgAsCorrectAnswer;
        Debug.Log("Starring" + currTutoConfig.gameType);
        currTargetImg.gameObject.SetActive(false);
        trialsPerTutoCount = 0;
        failurePerTutoCount = 0;
        consecutiveWinsTuto = 0;
        tutoStage++;

        if (tutoStage >= 2) currTutoConfig.completedFirstPart = true;

        if (!currTutoConfig.completedFirstPart) currInstruction = ActionsBeforeTutoStageStarts(currTutoConfig.firstInstructionAudio, currTutoConfig.firstInstructionAudio2);
        else
        {
            if (currTutoConfig.gameType != VoiceOrImageGameType.Mixed) currInstruction = ActionsBeforeTutoStageStarts(currTutoConfig.secondInstructionAudio1, currTutoConfig.secondInstructionAudio2);
        }
        StartCoroutine(currInstruction);
    }
    IEnumerator ActionsBeforeTutoStageStarts(AudioClip audio1, AudioClip audio2 = null)
    {
		isPaused = true;
		blockPanel.gameObject.SetActive(true);
        if(currTutoConfig.completedFirstPart && currTutoConfig.gameType != VoiceOrImageGameType.Mixed)
        {
            audioPlayer.clip = youDidGoodAudio;
            audioPlayer.Play();
            yield return new WaitForSeconds(youDidGoodAudio.length);
        }
        audioPlayer.clip = audio1;
        audioPlayer.Play();
        yield return new WaitForSeconds(audio1.length);
        if(audio2 != null)
        {
            audioPlayer.clip = audio2;
            audioPlayer.Play();
            yield return new WaitForSeconds(audio2.length);
        }
        blockPanel.gameObject.SetActive(false);
		isPaused = false;
		InitRound();
    }
	void Update()
	{
		if (isPaused) return;

		timerPerChoice += Time.deltaTime;
		timerUI.value = timerPerChoice;

		if (timerPerChoice >= currTutoConfig.roundTime)
		{
			timerPerChoice = 0;
			OnWrongChoice();
		}
	}


    void InitRound()
    {
        timerPerChoice = 0;

        trialsPerTutoCount++;
        
        audioPlayer.volume = 1;

        SetButtonState(discardBtn, discardBtnHighlightImg, enabledBtnColor, false);

        currSoundIsLeft = Random.Range(0f, 1f) > 0.5f;
        currImgIsLeft = Random.Range(0f, 1f) > 0.5f;

        var imgToUse = currImgIsLeft ? leftTargetSprite : rightTargetSprite;
        var soundToUse = currSoundIsLeft ? leftAudio : rightAudio;
        var textToUse = currSoundIsLeft ? leftObjTxt : rightObjTxt;

        currTargetImg.sprite = imgToUse;

        effectPlayer.clip = soundToUse;

        switch (currTutoConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                currTargetImg.gameObject.SetActive(false);
                currImgIsLeft = currSoundIsLeft;
                effectPlayer.Play();
                break;
            case VoiceOrImageGameType.Image:
                currSoundIsLeft = currImgIsLeft;
                currTargetImg.gameObject.SetActive(true);
                break;
            case VoiceOrImageGameType.Mixed:
                effectPlayer.Play();
                break;
        }
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

    void SetButtonState(Button button, Image highlightImg, Color color, bool highlight)
    {
        button.image.color = color;
        highlightImg.gameObject.SetActive(highlight);
    }

	private void OnClickedLeft()
    {
        switch (currTutoConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                if (currSoundIsLeft && !currTutoConfig.completedFirstPart 
                    || !currSoundIsLeft && currTutoConfig.completedFirstPart && currTutoConfig.switchesToAnswerIsDifferentInSecondPart) OnCorrectChoice();
                else OnWrongChoice();
                break;
            case VoiceOrImageGameType.Image:
                if (currImgIsLeft && !currTutoConfig.completedFirstPart 
                    || !currImgIsLeft && currTutoConfig.completedFirstPart && currTutoConfig.switchesToAnswerIsDifferentInSecondPart) OnCorrectChoice();
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
        switch (currTutoConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                if (!currSoundIsLeft && tutoStage < 2 || currSoundIsLeft && tutoStage >= 2) OnCorrectChoice();
                else OnWrongChoice();
                break;
            case VoiceOrImageGameType.Image:
                if (!currImgIsLeft && tutoStage < 2 || currImgIsLeft && tutoStage >= 2) OnCorrectChoice();
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
        consecutiveWinsTuto = 0;
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

        
        if (currTutoConfig.gameType != VoiceOrImageGameType.Mixed) StartCoroutine(OnRoundEnded(wrongAudio.length));
        else StartCoroutine(PlayOnWrongChoice());        
    }


    IEnumerator PlayOnWrongChoice()
    {
		isPaused = true;
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
		isPaused = false;
		StartCoroutine(OnRoundEnded(0));
	}


    private void OnCorrectChoice()
    {
        consecutiveWinsTuto += 1;
        failurePerTutoCount = 0;

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

        StartCoroutine(OnRoundEnded(correctAudio.length));
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
		isPaused = false;

		if (consecutiveWinsTuto >= currTutoConfig.consecutiveWinsToPass || failurePerTutoCount >= currTutoConfig.consecutiveWinsToPass)
        {
            if(!currTutoConfig.completedFirstPart) 
            {
                InitTutorialStep();
                yield break;
            }
            MG_VoiceStarOrFlowerGameConfigs.passedTuto = false;
            StartCoroutine(CompleteTuto());
            yield break;
        }
        InitRound();
    }

    IEnumerator CompleteTuto()
    {
        isPaused = true;
        effectPlayer.Stop();
        discardBtn.gameObject.SetActive(false);
        leftBtn.gameObject.SetActive(false);
        rightBtn.gameObject.SetActive(false);
        audioPlayer.clip = currTutoConfig.endAudio;
        audioPlayer.Play();
        blockPanel.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(currTutoConfig.endAudio.length);
        switch (currTutoConfig.gameType)
        {
            case VoiceOrImageGameType.Voice:
                UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.VoiceOfImageVoice.ToString());
                break;
            case VoiceOrImageGameType.Image:
                UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.VoiceOfImageImage.ToString());
                break;
            case VoiceOrImageGameType.Mixed:
                UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.VoiceOfImageMixed.ToString());
                break;
        }

        blockPanel.gameObject.SetActive(false);
        GameSequencesList.Instance.GoToNextItemInList();
    }
}
