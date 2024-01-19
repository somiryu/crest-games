using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class HeartsAndStarts_Manager_Tutorial : MonoBehaviour
{
    [SerializeField] HeartsAndStarts_TutorialConfig tutorialConfig;
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

    [SerializeField] AudioClip correctAudio;
    [SerializeField] AudioClip wrongAudio;
    [SerializeField] AudioClip succeedStepAudio;
    [SerializeField] AudioClip finishAudio;

    [Header("UI")]
    [SerializeField] TMP_Text currRoundValueTxt;
    [SerializeField] Image bkColor;

    [SerializeField] List<TutorialConfigHeartsAndStars> myTutorialSteps = new List<TutorialConfigHeartsAndStars>();
    [SerializeField] TutorialConfigHeartsAndStars currTutoStep;
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
        currTutoStep = myTutorialSteps[0];
        currConsecutiveLoses = 0;
        currConsecutiveWins = 0;
        currTutoStep.InitTutoStep(bkColor);
        allTutorialsDoneFlag = false;

        audiosource = GetComponent<AudioSource>();


        afterActionPanel.SetActive(false);
        inGameUIPanel.SetActive(true);
        allTutorialsDoneFlag = false;

        leftBtn.onClick.AddListener(OnClickedLeft);
        rightBtn.onClick.AddListener(OnClickedRight);

    }

	private void Start()
	{
        InitRound();
	}

	void InitRound()
    {
		rightImg.gameObject.SetActive(false);
        leftImg.gameObject.SetActive(false);

        if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedRight) currRequiresSameDirection = true;
        else if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedLeft) currRequiresSameDirection = false;
        else if (currTutoStep.tutorialSteps == TutorialStepsHandS.Free 
            || currTutoStep.tutorialSteps == TutorialStepsHandS.FreeWSupport) currRequiresSameDirection = Random.Range(0f, 1f) > 0.5f;

        currShowingRight = Random.Range(0f, 1f) > 0.5f;

        var spriteToShow = currRequiresSameDirection ? sameDirectionSprite : opositeDirectionSprite;
        var imgToUse = currShowingRight ? rightImg : leftImg;

        if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedLeft || currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedRight)
        {
            TurnOnHighlightHelps();
        }
		else if (currTutoStep.tutorialSteps == TutorialStepsHandS.Free)
        {
            TurnOffHighlightHelps();
		}
        else if(currTutoStep.tutorialSteps == TutorialStepsHandS.FreeWSupport)
        {
            if (currConsecutiveLoses > 2 && !wasShowingHelpHighlights) TurnOnHighlightHelps();
            else if (currConsecutiveWins == 0 && currConsecutiveLoses == 0) TurnOffHighlightHelps();
            else if (wasShowingHelpHighlights && currConsecutiveWins > 2) TurnOffHighlightHelps();
            else if (wasShowingHelpHighlights && currConsecutiveWins <= 2) TurnOnHighlightHelps();
        }


		imgToUse.gameObject.SetActive(true);
        imgToUse.sprite = spriteToShow;
    }

    void TurnOnHighlightHelps()
    {
        wasShowingHelpHighlights = true;
		if (!currRequiresSameDirection && !currShowingRight || currRequiresSameDirection && currShowingRight)
		{
			leftBtnOnRightChoiceBG.gameObject.SetActive(false);
			leftBtn.image.color = disabledBtnColor;

			rightBtnOnRightChoiceBG.gameObject.SetActive(true);
			rightBtn.image.color = enabledBtnColor;
		}
		if (currRequiresSameDirection && !currShowingRight || !currRequiresSameDirection && currShowingRight)
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
			Victory();
			var currIdx = myTutorialSteps.IndexOf(currTutoStep);
			if (currIdx + 1 >= myTutorialSteps.Count) allTutorialsDoneFlag = true;
			else currTutoStep = myTutorialSteps[currIdx + 1];
            currConsecutiveWins = 0;
            currConsecutiveLoses = 0;
            currTutoStep.InitTutoStep(bkColor);
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
        LIncorrectparticle.Stop();
        RIncorrectparticle.Stop();
        RCorrectparticle.Stop();
        LCorrectparticle.Stop();

        audiosource.clip = wrongAudio;
        audiosource.Play();

        if (currShowingRight) RIncorrectparticle.Play();
        else LIncorrectparticle.Play();

        if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedRight
            || currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedLeft)
        {
            currConsecutiveLoses = 0;
            currConsecutiveWins = 0;
			return;
        }

        currConsecutiveLoses += 1;
        currConsecutiveWins = 0;
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
        if (currShowingRight) RCorrectparticle.Play();
        else LCorrectparticle.Play();

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

    public void Victory()
    {
        audiosource.clip = succeedStepAudio; 
        audiosource.Play();
    }
    void TutorialOver()
    {
        audiosource.clip = finishAudio;
        audiosource.Play();
        inGameUIPanel.SetActive(false);
        afterActionPanel.SetActive(true);
        StartCoroutine(GoToNextScene());
    }

    IEnumerator GoToNextScene()
    {
        UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.heartsAndStarsDone.ToString());
        yield return new WaitForSeconds(1);
        GameSequencesList.Instance.GoToNextSequence();
    }

}
[Serializable]
public class TutorialConfigHeartsAndStars
{
    public TutorialStepsHandS tutorialSteps;
    public int trialsAmt;
    public Sprite ifRightBtnIsTheRightChoice;
    public Sprite ifLeftBtnIsTheRightChoice;
    public List<bool> passedTuto = new List<bool>();
    public Color background;

    public void InitTutoStep(Image bk)
    {
        bk.color = background;
        Debug.Log("starting " + tutorialSteps.ToString());
    }
}
public enum TutorialStepsHandS
{
    HighlightedRight,
    HighlightedLeft,
    Free,
    FreeWSupport
}