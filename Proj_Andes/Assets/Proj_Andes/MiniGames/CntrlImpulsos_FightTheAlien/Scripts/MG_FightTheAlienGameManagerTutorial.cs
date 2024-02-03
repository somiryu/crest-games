using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_FightTheAlienManagerTutorial : MonoBehaviour, IEndOfGameManager
{

    int currentTutorialStep;
    int currPointsAmount;
    MG_FightTheAlienTutorialStep currStepConfigTutorial;

    [SerializeField] MG_FightTheAlienGameConfigsTutorial tutorialStepsConfigs;
    [Space(20)]
    [SerializeField] MG_FightTheAlienGameConfigs gameConfigs;
    [Space(20)]
    [SerializeField] Image alienAttackImage;
    [Space(20)]
    [SerializeField] MG_FightTheAlienAnswerBtnTutorial [] answerBtns;
    [Space(20)]
    [SerializeField] MG_FightTheAlienPopUP tutorialPopUp;

    [SerializeField] GameObject afterActionPanel;

    [Header("Game UI")]
    [SerializeField] GameObject startCounter;
    [SerializeField] TMP_Text currCoinsValueTxt;
    [SerializeField] TMP_Text currRoundValueTxt;
    [SerializeField] Slider timerUI;
    [SerializeField] Slider playerHealthUI;
    [SerializeField] Slider enemyHealthUI;
    [Header("After Action UI")]
    [SerializeField] TMP_Text afterActionFinalCoinsTxt;
    [SerializeField] Button retryBtn;
    [SerializeField] GameObject inGameUIPaneltoDissapear;


    [Header("Game Audio")]
    [SerializeField] AudioClip correctAudio;
    [SerializeField] AudioClip wrongAudio;
    [SerializeField] AudioClip finishAudio;
    [SerializeField] AudioClip equalFeedbackAudio;
    [SerializeField] AudioClip differentFeedbackAudio;
    [SerializeField] Color disabledBtnColor;
    [SerializeField] Color enabledBtnColor;

    [Header("GameParticles")]
    [SerializeField] ParticleSystem correctParticles;
    [SerializeField] ParticleSystem incorrectParticles;
    [SerializeField] GameObject skinObj;
    [SerializeField] GameObject skinObjPopUp;
    [SerializeField] Animator[] skinObjAnim;
    [SerializeField] Animator[] skinObjAnimPopUp;

    [Header("Posible Answer")]
    [SerializeField] AlienAttackConfig[] alienAttacksConfigsMatch;
    [SerializeField] AlienAttackConfig[] alienAttacksConfigsNoMatch;
    AlienAttackConfig[] currAlienAttacksConfigs;
    AlienAttackConfig currAttack;
    bool isMatchAttack = true;

    bool isCorrect;
    bool wrongColor;
    bool wrongShape;
    MG_FightTheAlienAnswerBtnTutorial playerAnswer;
    MG_FightTheAlienAnswerBtnTutorial currCorrectButtonAnswer;

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;

    private float timerPerChoice = 0;
    private int currCoins;
    private int currCorrectAnswerIdx;
    private int currPlayerHealth;
    private int currEnemyHealth;
    private AudioSource audiosource;

    private bool gameoverFlag = false;

    public void Awake()
    {
        Init();
        currentTutorialStep = 0;
    }

    private void Init()
    {

        audiosource = GetComponent<AudioSource>();
        skinObjAnim = skinObj.GetComponentsInChildren<Animator>(true);
        skinObjAnimPopUp = skinObjPopUp.GetComponentsInChildren<Animator>(true);
        currCoins = gameConfigs.initialCoins;
        currPlayerHealth = gameConfigs.PlayerHealth;
        currEnemyHealth = gameConfigs.EnemyHealth;

        afterActionPanel.SetActive(false);
        inGameUIPaneltoDissapear.SetActive(true);
        gameoverFlag = false;

        timerUI.minValue = 0;
        timerUI.maxValue = gameConfigs.timePerChoice;

        enemyHealthUI.maxValue = gameConfigs.EnemyHealth;
        playerHealthUI.maxValue = gameConfigs.PlayerHealth;

        enemyHealthUI.value = gameConfigs.EnemyHealth;
        playerHealthUI.value = gameConfigs.PlayerHealth;

        currCoinsValueTxt.text = 0.ToString();

        for (int i = 0; i < answerBtns.Length; i++)
        {
            var currIdx = i;
            answerBtns[i].button.onClick.AddListener(() => OnAnswerBtnClicked(currIdx, answerBtns[currIdx]));
        }

        InitTutorialStep();
        InitRound();
    }

    private void InitTutorialStep()
    {
        currStepConfigTutorial = tutorialStepsConfigs.mG_FightTheAlienTutorialSteps[currentTutorialStep];
        currPointsAmount = 0;
        timerUI.gameObject.SetActive(currStepConfigTutorial.time);
        enemyHealthUI.gameObject.SetActive(currStepConfigTutorial.life);
        playerHealthUI.gameObject.SetActive(currStepConfigTutorial.life);
        startCounter.SetActive(currStepConfigTutorial.score);
        SetAlienAttackConfig();


    }

    private void SetAlienAttackConfig()
    {        
        switch (currStepConfigTutorial.alienAttacksConfigsType)
        {
            case alienAttacksConfigsType.Match:
                isMatchAttack = true;
                currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsMatch;
                break;
            case alienAttacksConfigsType.NoMatch:
                isMatchAttack = false;
                currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsNoMatch;
                break;
            case alienAttacksConfigsType.Interval:
                isMatchAttack = !isMatchAttack;
                if (isMatchAttack)
                    currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsMatch;                
                else
                    currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsNoMatch;
                break;
            case alienAttacksConfigsType.Random:
                isMatchAttack = Random.Range(0, 2) == 0;
                if (isMatchAttack) currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsMatch;
                else currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsNoMatch;
                break;
            default:
                break;
        }
    }

    void InitRound()
    {        
        timerPerChoice = 0;
        eogManager.OnGameStart();

        if (currStepConfigTutorial.alienAttacksConfigsType == alienAttacksConfigsType.Interval 
            || currStepConfigTutorial.alienAttacksConfigsType == alienAttacksConfigsType.Random)
        {
            SetAlienAttackConfig();
        }

        var randomAttack = Random.Range(0, currAlienAttacksConfigs.Length);
        currAttack = currAlienAttacksConfigs[randomAttack];

        alienAttackImage.sprite = currAttack.alienAtack.attackSprite;

        currCorrectAnswerIdx = Random.Range(0, 3);

        var firstWrongImageUsedFlag = false;

        for (int i = 0; i < 3; i++)
        {
            var currBtnImage = answerBtns[i];
            currBtnImage.ShowHighlightImg(false);
            var helpButton = tutorialStepsConfigs.mG_FightTheAlienTutorialSteps[currentTutorialStep].helpButton;
            if (i == currCorrectAnswerIdx)
            {
                currBtnImage.SetAnswerImage(currAttack.rightAnswer.attackSprite);
                currBtnImage.ShowHighlightImg(helpButton);
				currBtnImage.button.image.color = enabledBtnColor;
				currBtnImage.alienAttackOption = currAttack.rightAnswer;
                currCorrectButtonAnswer = currBtnImage;
            }
            else
            {
				if (helpButton) currBtnImage.button.image.color = disabledBtnColor;
				else currBtnImage.button.image.color = enabledBtnColor;

				if (!firstWrongImageUsedFlag)
				{
					currBtnImage.SetAnswerImage(currAttack.wrongAnswer1.attackSprite);
					currBtnImage.alienAttackOption = currAttack.wrongAnswer1;
					firstWrongImageUsedFlag = true;
				}
				else
				{
					currBtnImage.SetAnswerImage(currAttack.wrongAnswer2.attackSprite);
					currBtnImage.alienAttackOption = currAttack.wrongAnswer2;
				}
			}
        }
    }

    private void Update()
    {
        if (gameoverFlag) return;
        if (!currStepConfigTutorial.time) return;

        timerUI.value = timerPerChoice;
        timerPerChoice += Time.deltaTime;
        if (timerPerChoice >= gameConfigs.timePerChoice)
        {
            timerPerChoice = 0;
            OnWrongChoice();
        }
    }

    void OnAnswerBtnClicked(int idx, MG_FightTheAlienAnswerBtnTutorial button)
    {
        playerAnswer = button;
        if (idx == currCorrectAnswerIdx) OnCorrectChoice();
        else OnWrongChoice();     

    }
    private void OnWrongChoice()
    {
        isCorrect = false;        

        correctParticles.Stop();
        incorrectParticles.Stop();

        audiosource.clip = wrongAudio;
        audiosource.Play();

        currPointsAmount = 0;

        for (int i = 0; i < skinObjAnim.Length; i++)
        {

            skinObjAnim[i].SetTrigger("Incorrect");
            skinObjAnimPopUp[i].SetTrigger("Incorrect");

        }

        incorrectParticles.Play();
        incorrectParticles.Play();

        if (currStepConfigTutorial.helpAudioFeedback && !currStepConfigTutorial.wrongChoices) StartCoroutine(PlayAudioFeedback());

        if (!currStepConfigTutorial.wrongChoices) return;
        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        currPlayerHealth += gameConfigs.playerHealthLostOnWrongAnswer;

        OnRoundEnded();
    }

    IEnumerator PlayAudioFeedback()
    {
        currCorrectButtonAnswer.ShowHighlightImg(true);

        AudioClip feedbackAudio = isMatchAttack ? equalFeedbackAudio : differentFeedbackAudio;

        audiosource.clip = feedbackAudio;
        audiosource.Play();

        var waitTime = feedbackAudio.length + 0.1f;        
        yield return new WaitForSeconds(waitTime);        
    }

    private void OnCorrectChoice()
    {
        isCorrect = true;

        correctParticles.Stop();
        incorrectParticles.Stop();

        audiosource.clip = correctAudio;
        audiosource.Play();

        correctParticles.Play();
        for (int i = 0; i < skinObjAnim.Length; i++)
        {

            skinObjAnim[i].SetTrigger("Correct");
            skinObjAnimPopUp[i].SetTrigger("Correct");
        }


        currCoins += gameConfigs.coinsOnCorrectAnswer;
        if (currStepConfigTutorial.alienAttacksConfigsType == alienAttacksConfigsType.Random)
            currEnemyHealth += gameConfigs.EnemyHealthLostOnRightAnswer;
        currPointsAmount += 1;
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        currCoinsValueTxt.text = currCoins.ToString();
        playerHealthUI.value = currPlayerHealth;
        enemyHealthUI.value = currEnemyHealth;
        

        if(currPointsAmount == currStepConfigTutorial.stepsAmount)
        {
			audiosource.clip = finishAudio;
			audiosource.Play();
			currentTutorialStep += 1;
			if (currentTutorialStep >= tutorialStepsConfigs.mG_FightTheAlienTutorialSteps.Count)
			{
				GameOver();
				return;
			}
			InitTutorialStep();
        }

        InitRound();
    }

    void GameOver()
    {
        audiosource.clip = finishAudio;
        audiosource.Play();

        gameoverFlag = true;

        StartCoroutine(NextSceneAfterTime());   

    }

    IEnumerator NextSceneAfterTime()
    {
        UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.FightTheAlienDone.ToString());
        yield return new WaitForSeconds(1);
        GameSequencesList.Instance.GoToNextSequence();
    }

    private void ActivePopUp()
    {
        if (playerAnswer.alienAttackOption.shapeAlienAttack == currAttack.alienAtack.shapeAlienAttack) wrongShape = false;
        else wrongShape = true;
        if (playerAnswer.alienAttackOption.colorAlienAttack == currAttack.alienAtack.colorAlienAttack) wrongColor = false;
        else wrongColor = true;
        tutorialPopUp.SetAlienAttackImage(currAttack.alienAtack.attackSprite);
        tutorialPopUp.SetAnswerSelectedImage(playerAnswer.alienAttackOption.attackSprite);
        tutorialPopUp.SetColorImage(playerAnswer.alienAttackOption.colorAlienAttack, wrongColor);
        tutorialPopUp.SetShapeImage(playerAnswer.alienAttackOption.shapeAlienAttack,wrongShape);
        tutorialPopUp.SetFaceFeedbackImage(isCorrect);
        tutorialPopUp.gameObject.SetActive(true);



    }

   
}


