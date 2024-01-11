using System;
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
    [SerializeField] AudioClip finishAudio;

    [Header("UI")]
    [SerializeField] TMP_Text currRoundValueTxt;

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
        currTutoStep.InitTutoStep();

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


        currRequiresSameDirection = Random.Range(0f, 1f) > 0.5f;
        var spriteToShow = currRequiresSameDirection ? sameDirectionSprite : opositeDirectionSprite;
        currShowingRight = Random.Range(0f, 1f) > 0.5f;
        var imgToUse = currShowingRight ? rightImg : leftImg;
        if (!currRequiresSameDirection && !currShowingRight || currRequiresSameDirection && currShowingRight) 
        {
            rightBtn.image.sprite = currTutoStep.ifRightBtnIsTheRightChoice;
            rightBtn.interactable = true;
            leftBtn.image.sprite = leftBtnPaled;
            leftBtn.interactable = currTutoStep.activeItemIfIncorrect;
        }
        if (currRequiresSameDirection && !currShowingRight || !currRequiresSameDirection && currShowingRight)
        {
            rightBtn.image.sprite = rightBtnPaled;
            rightBtn.interactable = currTutoStep.activeItemIfIncorrect;
            leftBtn.image.sprite = currTutoStep.ifLeftBtnIsTheRightChoice;
            leftBtn.interactable = true;
        }
        imgToUse.gameObject.SetActive(true);
        imgToUse.sprite = spriteToShow;
    }

    private void Update()
    {
        if(currTutoStep.passedTuto.Count >= currTutoStep.trialsAmt)
        {
            if(tutorialSteps.IndexOf(currTutoStep) + 1 <= tutorialSteps.Count)
            {
                currTutoStep = tutorialSteps[tutorialSteps.IndexOf(currTutoStep) + 1];
                currTutoStep.InitTutoStep();
            }
            else TutorialOver();
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

        currTutoStep.passedTuto.Add(true);
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        currRound++;
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
        return false;
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
    public bool activeItemIfIncorrect;
    public List<bool> passedTuto = new List<bool>();

    public void InitTutoStep()
    {
        passedTuto.Clear();
        Debug.Log("starting " + tutorialSteps.ToString());
    }
}
public enum TutorialStepsHandS
{
    HighlightedPass,
    HighlightedFree,
    Free
}