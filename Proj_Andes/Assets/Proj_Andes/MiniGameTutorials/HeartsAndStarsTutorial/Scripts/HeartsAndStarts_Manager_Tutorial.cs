using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
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
    
    [SerializeField] Sprite leftBtnPaled;
    [SerializeField] Sprite rightBtnPaled;    
    [SerializeField] Sprite leftBtnReg;
    [SerializeField] Sprite rightBtnReg;

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

    [SerializeField] List<TutorialConfigHeartsAndStars> tutorialSteps = new List<TutorialConfigHeartsAndStars>();
    [SerializeField] TutorialConfigHeartsAndStars currTutoStep;
    private AudioSource audiosource;

    private int currRound;

    private bool currShowingRight = false;
    private bool currRequiresSameDirection = false;

    private bool tutorialPassFlag = false;

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        currRound = 0;
        currTutoStep = tutorialSteps[0];
        currTutoStep.InitTutoStep(bkColor);
        tutorialPassFlag = false;

        audiosource = GetComponent<AudioSource>();


        afterActionPanel.SetActive(false);
        inGameUIPanel.SetActive(true);
        tutorialPassFlag = false;

        leftBtn.onClick.AddListener(OnClickedLeft);
        rightBtn.onClick.AddListener(OnClickedRight);

        InitRound();
    }

    void InitRound()
    {
            
        rightImg.gameObject.SetActive(false);
        leftImg.gameObject.SetActive(false);

        if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedRight) currRequiresSameDirection = true;
        else if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedLeft) currRequiresSameDirection = false;
        else if (currTutoStep.tutorialSteps == TutorialStepsHandS.Free || currTutoStep.tutorialSteps == TutorialStepsHandS.FreeWSupport) currRequiresSameDirection = Random.Range(0f, 1f) > 0.5f;

        currShowingRight = Random.Range(0f, 1f) > 0.5f;

        var spriteToShow = currRequiresSameDirection ? sameDirectionSprite : opositeDirectionSprite;
        var imgToUse = currShowingRight ? rightImg : leftImg;

        if (!currRequiresSameDirection && !currShowingRight || currRequiresSameDirection && currShowingRight) 
        {
            rightBtn.image.sprite = currTutoStep.ifRightBtnIsTheRightChoice;
            leftBtn.image.sprite = leftBtnPaled;
        }
        if (currRequiresSameDirection && !currShowingRight || !currRequiresSameDirection && currShowingRight)
        {
            rightBtn.image.sprite = rightBtnPaled;
            leftBtn.image.sprite = currTutoStep.ifLeftBtnIsTheRightChoice;
        }

        imgToUse.gameObject.SetActive(true);
        imgToUse.sprite = spriteToShow;
    }

    private void Update()
    {
        EndOfRoundCheck();
    }
    void EndOfRoundCheck()
    {
        if (currTutoStep.passedTuto.Count >= currTutoStep.trialsAmt)
        {
            if (TutorialStepPassed())
            {
                Debug.Log("tuto step passed");
                Victory();
                if (currTutoStep.tutorialSteps == TutorialStepsHandS.FreeWSupport) currTutoStep = tutorialSteps[tutorialSteps.IndexOf(currTutoStep) - 1];
                else currTutoStep = tutorialSteps[tutorialSteps.IndexOf(currTutoStep) + 1];
            }
            else
            {
                Debug.Log("tuto step failed");
                if (currTutoStep.tutorialSteps == TutorialStepsHandS.Free) currTutoStep = tutorialSteps[tutorialSteps.IndexOf(currTutoStep) + 1];
            }
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

        if (currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedRight || currTutoStep.tutorialSteps == TutorialStepsHandS.HighlightedLeft)
        {
            if (currTutoStep.passedTuto.Count > 0) if (!currTutoStep.passedTuto[currTutoStep.passedTuto.Count - 1]) return;
        }
        currTutoStep.passedTuto.Add(false);

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

        for (int i = 0; i < currTutoStep.passedTuto.Count; i++)
        {
            if (!currTutoStep.passedTuto[i]) currTutoStep.InitTutoStep(bkColor);
        }

        currTutoStep.passedTuto.Add(true);
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        currRound++;
        EndOfRoundCheck();
        currRoundValueTxt.text = currRound.ToString();
        if (tutorialPassFlag)
        {
            TutorialOver();
            return;
        }

        Animator animatorImg = inGameUIPanel.GetComponent<Animator>();
        animatorImg.ResetTrigger("Appear");
        animatorImg.SetTrigger("Appear");
        InitRound();
    }
    bool TutorialStepPassed()
    {
        for (int i = 0; i < currTutoStep.passedTuto.Count; i++)
        {
            if (!currTutoStep.passedTuto[i]) return false;
        }
        if (currTutoStep.tutorialSteps == TutorialStepsHandS.Free) tutorialPassFlag = true;
        return true;
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
        tutorialPassFlag = true;
        inGameUIPanel.SetActive(false);
        afterActionPanel.SetActive(true);
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
        passedTuto.Clear();
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