using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HeartsAndStarts_Manager_Tutorial : MonoBehaviour
{
    [SerializeField] HeartsAndStarts_TutorialConfig tutoConfig;
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
    public static int currTutoStepIdx;
    private AudioSource audiosource;

    private int currRound;

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
        else if (currTutoStep.tutorialSteps == TutorialStepsHandS.Mixed) currRequiresSameDirection = Random.Range(0f, 1f) > 0.5f;

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
        if (currConsecutiveWins >= currTutoStep.trialsAmt)
        {
			Debug.Log("tuto step passed");
			PlayVictoryAudio();
            allTutorialsDoneFlag = true;
            currConsecutiveWins = 0;
            currConsecutiveLoses = 0;
        }
        else if(currRound >= currTutoStep.maxRoundsBeforeLosing)
        {
			Debug.Log("tuto step Failed");
			allTutorialsDoneFlag = true;
			currConsecutiveWins = 0;
			currConsecutiveLoses = 0;
		}
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

		if (currShowingRight) RIncorrectparticle.Play();
		else LIncorrectparticle.Play();
		gameUi.StarLost();

		currConsecutiveLoses += 1;
		currConsecutiveWins = 0;

		blockScreenPanel.SetActive(false);
		OnRoundEnded();
	}

    private void OnCorrectChoice()
    {
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

        currConsecutiveLoses = 0;
        currConsecutiveWins += 1;
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        currRound++;
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

		blockScreenPanel.SetActive(false);
		GameSequencesList.Instance.GoToNextItemInList();

	}
}

[Serializable]
public class TutorialConfigHeartsAndStars
{
    public TutorialStepsHandS tutorialSteps;
    public int trialsAmt;
    public int maxRoundsBeforeLosing;
    public int timePerChoice;
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