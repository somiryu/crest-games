using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HeartsAndStarts_Manager_Tutorial : MonoBehaviour
{
    [Space(20)]
    [SerializeField] Sprite sameDirectionSprite;
    [SerializeField] Sprite opositeDirectionSprite;
    [Space(20)]
    [SerializeField] Image leftImg;
    [SerializeField] Image rightImg;
    [Space(20)]
    [SerializeField] Button leftBtn;
    [SerializeField] Button rightBtn;
	[SerializeField] Image leftBtnOnRightChoiceBG;
	[SerializeField] Image rightBtnOnRightChoiceBG;

    [SerializeField] Color disabledBtnColor;
    [SerializeField] Color enabledBtnColor;

    [SerializeField] ParticleSystem LCorrectparticle;
    [SerializeField] ParticleSystem RCorrectparticle;

    [SerializeField] ParticleSystem LIncorrectparticle;
    [SerializeField] ParticleSystem RIncorrectparticle;

    [SerializeField] GameObject afterActionPanel;
    [SerializeField] GameObject inGameUIPanel;
    [SerializeField] Slider roundSlider;
    GameUIController gameUi => GameUIController.Instance;

    [SerializeField] AudioClip reminderAudio;
    [SerializeField] AudioClip letsTryAudio;
    [SerializeField] AudioClip correctionIfStarAudio;
    [SerializeField] AudioClip correctionIfHeartAudio;
    [SerializeField] AudioClip letsPlayAudio;
    [SerializeField] AudioClip correctAudio;
    [SerializeField] AudioClip wrongAudio;
    [SerializeField] AudioClip succeedStepAudio;
    [SerializeField] AudioClip finishAudio;
    [SerializeField] AudioClip ifHeartAudio;
    [SerializeField] AudioClip ifStarAudio;

    [SerializeField] GameObject blockScreenPanel;

    [Header("UI")]
    [SerializeField] TMP_Text currRoundValueTxt;

    [SerializeField] List<TutorialConfigHeartsAndStars> myTutorialSteps = new List<TutorialConfigHeartsAndStars>();
    [SerializeField] TutorialConfigHeartsAndStars currTutoStep;
    int heartCount;
    int starCount;
    public static int currTutoStepIdx;
    private AudioSource audiosource;

    private int currRound;
    bool tutoBegan;
    bool onHold;
    float timerPerChoice;
    private bool currShowingRight = false;
    private bool currRequiresSameDirection = false;

    private bool allTutorialsDoneFlag = false;

    private float currConsecutiveWins = 0;
    private float currConsecutiveLoses = 0;

    private bool wasShowingHelpHighlights = false;


    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        currRound = 0;
        currTutoStep = myTutorialSteps[currTutoStepIdx];
        currConsecutiveLoses = 0;
        currConsecutiveWins = 0;        
        allTutorialsDoneFlag = false;
        tutoBegan = false;
        timerPerChoice = 0;

        audiosource = GetComponent<AudioSource>();
        StartCoroutine(Introduction());

        afterActionPanel.SetActive(false);
        inGameUIPanel.SetActive(true);
        allTutorialsDoneFlag = false;

        leftBtn.onClick.AddListener(OnClickedLeft);
        rightBtn.onClick.AddListener(OnClickedRight);
    }

	void InitRound()
    {
		rightImg.gameObject.SetActive(false);
        leftImg.gameObject.SetActive(false);

        currTutoStep.tutoRoundsCount++;
        if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedHearths) currRequiresSameDirection = true;
        else if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedFlowers) currRequiresSameDirection = false;
        else if (currTutoStep.tutorialSteps == TutorialStepsHandS.Mixed)
        {
            currRequiresSameDirection = Random.Range(0f, 1f) > 0.5f;
            if (heartCount >= currTutoStep.maxRoundsBeforeLosing / 2) currRequiresSameDirection = false;
            else if (starCount >= currTutoStep.maxRoundsBeforeLosing / 2) currRequiresSameDirection = true;

			if (currRequiresSameDirection) heartCount++;
			else starCount++;
		}

        currShowingRight = Random.Range(0f, 1f) > 0.5f;

        var spriteToShow = currRequiresSameDirection ? sameDirectionSprite : opositeDirectionSprite;
        var imgToUse = currShowingRight ? rightImg : leftImg;

        imgToUse.gameObject.SetActive(true);
        imgToUse.sprite = spriteToShow;

		if (currTutoStep.tutoRoundsCount == 1) StartCoroutine(RunPostInitRoundInstructions());

		if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedFlowers || currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedHearths)
        {
            TurnOnHighlightHelps();
        }
        else if (currTutoStep.tutorialSteps == TutorialStepsHandS.Mixed)
        {
            TurnOffHighlightHelps();
		}
    }
    private void Update()
    {
        if (!tutoBegan) return;
        if (onHold) return;
        timerPerChoice += Time.deltaTime;
        var progress = Mathf.Clamp01(timerPerChoice / currTutoStep.timePerChoiceTuto);
        roundSlider.value = progress;
        if (timerPerChoice >= currTutoStep.timePerChoiceTuto)
        {
            timerPerChoice = 0;
            OnWrongChoice();
        }
    }
    IEnumerator Introduction()
    {
        blockScreenPanel.SetActive(true);

		rightBtn.interactable = false;
        leftBtn.interactable = false;
        leftImg.gameObject.SetActive(false);
        rightImg.gameObject.SetActive(false);
        for (int i = 0; i < currTutoStep.preInitRoundInstructions.Length; i++)
        {
            audiosource.clip = currTutoStep.preInitRoundInstructions[i];
            audiosource.Play();
            yield return new WaitForSeconds(audiosource.clip.length);
        }
        rightBtn.interactable = true;
        leftBtn.interactable = true;
        leftImg.gameObject.SetActive(true);
        rightImg.gameObject.SetActive(true);
		blockScreenPanel.SetActive(false);
		InitRound();
    }
    IEnumerator RunPostInitRoundInstructions()
    {
		blockScreenPanel.SetActive(true);
		rightBtn.interactable = false;
        leftBtn.interactable = false;
		for (int i = 0; i < currTutoStep.postInitRoundInstructions.Length; i++)
		{
			audiosource.clip = currTutoStep.postInitRoundInstructions[i];
			audiosource.Play();
			yield return new WaitForSeconds(audiosource.clip.length);
        }
        tutoBegan = true;
        rightBtn.interactable = true;
        leftBtn.interactable = true;
		blockScreenPanel.SetActive(false);
	}
	void TurnOnHighlightHelps()
    {
        wasShowingHelpHighlights = true;
		if (currRequiresSameDirection && currShowingRight || !currRequiresSameDirection && !currShowingRight)
		{
            leftBtnOnRightChoiceBG.gameObject.SetActive(false);
			leftBtn.image.color = disabledBtnColor;

			rightBtnOnRightChoiceBG.gameObject.SetActive(true);
			rightBtn.image.color = enabledBtnColor;
        }
        else if (!currRequiresSameDirection && currShowingRight || currRequiresSameDirection && !currShowingRight)
		{
            rightBtnOnRightChoiceBG.gameObject.SetActive(false);
			rightBtn.image.color = disabledBtnColor;

			leftBtnOnRightChoiceBG.gameObject.SetActive(true);
			leftBtn.image.color = enabledBtnColor;
        }
    }

    void TurnOffHighlightHelps()
    {
        wasShowingHelpHighlights = false;
		leftBtnOnRightChoiceBG.gameObject.SetActive(false);
		rightBtnOnRightChoiceBG.gameObject.SetActive(false);
		leftBtn.image.color = enabledBtnColor;
		rightBtn.image.color = enabledBtnColor;
	}

    void EndOfRoundCheck()
    {
        if(currConsecutiveWins >= currTutoStep.roundsNeededToWinTutorial)
        {
			PlayVictoryAudio();
			MG_HearthAndStarsGameConfigs.passedTuto = 1;
			allTutorialsDoneFlag = true;
			currConsecutiveWins = 0;
			currConsecutiveLoses = 0;
		}

		if (currRound >= currTutoStep.maxRoundsBeforeLosing)
        {
            allTutorialsDoneFlag = true;
            MG_HearthAndStarsGameConfigs.passedTuto = 0;
			currConsecutiveWins = 0;
			currConsecutiveLoses = 0;

		}
        Debug.Log("passed " + MG_HearthAndStarsGameConfigs.passedTuto);
    }
    private void OnClickedLeft()
    {
        var succed = false;
        if (!currRequiresSameDirection && currShowingRight) succed = true;
        if (currRequiresSameDirection && !currShowingRight) succed = true;
        if (succed) OnCorrectChoice();
        else OnWrongChoice();
    }

    private void OnClickedRight()
    {
        var succed = false;
        if (currRequiresSameDirection && currShowingRight) succed = true;
        if (!currRequiresSameDirection && !currShowingRight) succed = true;
        if (succed) OnCorrectChoice();
        else OnWrongChoice();
    }

    private void OnWrongChoice()
    {
        StartCoroutine(OnWrongChoiceRoutine());
    }

    private IEnumerator OnWrongChoiceRoutine()
    {
		blockScreenPanel.SetActive(true);
        onHold = true;
		LIncorrectparticle.Stop();
		RIncorrectparticle.Stop();
		RCorrectparticle.Stop();
		LCorrectparticle.Stop();

		rightBtn.interactable = false;
		leftBtn.interactable = false;

		audiosource.clip = currRequiresSameDirection? correctionIfHeartAudio : correctionIfStarAudio;
		audiosource.Play();
        yield return new WaitForSeconds(audiosource.clip.length);

		rightBtn.interactable = true;
		leftBtn.interactable = true;
        timerPerChoice = 0;

        if (currShowingRight) RIncorrectparticle.Play();
		else LIncorrectparticle.Play();
		gameUi.StarLost();
        onHold = false;

        currConsecutiveWins = 0;
        currConsecutiveLoses += 1;

		blockScreenPanel.SetActive(false);
		OnRoundEnded();
	}

    private void OnCorrectChoice()
    {
        StartCoroutine(OnRightChoiceCorroutine());
    }
    IEnumerator OnRightChoiceCorroutine()
    {
        onHold = true;
        blockScreenPanel.gameObject.SetActive(true);
        LIncorrectparticle.Stop();
        RIncorrectparticle.Stop();
        RCorrectparticle.Stop();
        LCorrectparticle.Stop();

        audiosource.clip = correctAudio;
        audiosource.Play();

        if (currShowingRight)
        {
            RCorrectparticle.Play();
        }
        else
        {
            LCorrectparticle.Play();
        }

        currConsecutiveWins += 1;
        yield return new WaitForSeconds(currTutoStep.intermidiateHold);
        blockScreenPanel.gameObject.SetActive(false);
        timerPerChoice = 0;
        onHold = false;
        OnRoundEnded();

    }
    void OnRoundEnded()
    {
        currRound++;
        timerPerChoice = 0;
        EndOfRoundCheck();
        currRoundValueTxt.text = currRound.ToString();
        if (allTutorialsDoneFlag)
        {
            TutorialOver();
            return;
        }

        Animator animatorImg = inGameUIPanel.GetComponent<Animator>();
        animatorImg.ResetTrigger("Appear");
        animatorImg.SetTrigger("Appear");
        InitRound();
    }

    public void PlayVictoryAudio()
    {
        audiosource.clip = succeedStepAudio; 
        audiosource.Play();
    }

    void TutorialOver()
    {
        StartCoroutine(OnTutorialOverRoutine());
    }

    IEnumerator OnTutorialOverRoutine()
    {
		inGameUIPanel.SetActive(false);
		afterActionPanel.SetActive(true);
		currTutoStepIdx++;
        onHold = true;
		switch (currTutoStep.tutorialSteps)
		{
			case TutorialStepsHandS.HighlightedHearths:
				UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.HeartsAndFlowersHeartsDone.ToString());
				break;
			case TutorialStepsHandS.HighlightedFlowers:
				UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.HeartsAndFlowersFlowersDone.ToString());
				break;
			case TutorialStepsHandS.Mixed:
				UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.HeartsAndFlowersMixedDone.ToString());
				break;
		}

		blockScreenPanel.SetActive(true);

		for (int i = 0; i < currTutoStep.finishTutorialStepAudios.Length; i++)
        {
			audiosource.clip = currTutoStep.finishTutorialStepAudios[i];
			audiosource.Play();
			yield return new WaitForSeconds(audiosource.clip.length);
		}

        if (MG_HearthAndStarsGameConfigs.passedTuto == 1)
        {
			audiosource.clip = succeedStepAudio;
			audiosource.Play();
			yield return new WaitForSeconds(audiosource.clip.length);
		}

        //Delay asked by the client
		yield return new WaitForSeconds(1);

		blockScreenPanel.SetActive(false);
		GameSequencesList.Instance.GoToNextItemInList();

	}
}

[Serializable]
public class TutorialConfigHeartsAndStars
{
    public TutorialStepsHandS tutorialSteps;
    [FormerlySerializedAs("trialsAmt")]
    public int roundsNeededToWinTutorial;
    public int maxRoundsBeforeLosing;
	public float timePerChoiceTuto;
	public float intermidiateHold;
    [Space]
	public Sprite ifRightBtnIsTheRightChoice;
    public Sprite ifLeftBtnIsTheRightChoice;
    public List<bool> passedTuto = new List<bool>();
    public Color background;
    public AudioClip audioInstruction;
    public AudioClip[] preInitRoundInstructions;
    public AudioClip[] postInitRoundInstructions;
    public AudioClip[] finishTutorialStepAudios;
    public int tutoRoundsCount;

    public void InitTutoStep()
    {
        tutoRoundsCount = 0;
        Debug.Log("starting " + tutorialSteps.ToString());
    }
}
public enum TutorialStepsHandS
{
    HighlightedHearths,
    HighlightedFlowers,
    Mixed
}