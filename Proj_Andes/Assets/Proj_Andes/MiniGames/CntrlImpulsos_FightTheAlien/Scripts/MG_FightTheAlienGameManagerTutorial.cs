using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] Button retryBtn2;
    [SerializeField] GameObject inGameUIPaneltoDissapear;


    [Header("Game Audio")]
    [SerializeField] AudioClip correctAudio;
    [SerializeField] AudioClip wrongAudio;
    [SerializeField] AudioClip finishAudio;

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

        retryBtn2.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single));
        InitTutorialStep();
        InitRound();
    }

    private void InitTutorialStep()
    {
        Debug.Log("tutorial" + currentTutorialStep);
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
                currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsMatch;
                break;
            case alienAttacksConfigsType.NoMatch:
                currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsNoMatch;
                break;
            case alienAttacksConfigsType.Interval:
                if (isMatchAttack)
                    currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsMatch;                
                else
                    currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsNoMatch;
                break;
            case alienAttacksConfigsType.Random:
                currAlienAttacksConfigs = tutorialStepsConfigs.alienAttacksConfigsMatch.Concat(tutorialStepsConfigs.alienAttacksConfigsNoMatch).ToArray();
                break;
            default:
                break;
        }
    }

    void InitRound()
    {        
        timerPerChoice = 0;
        eogManager.OnGameStart();

        if (currStepConfigTutorial.alienAttacksConfigsType == alienAttacksConfigsType.Interval)
        {
            SetAlienAttackConfig();
            isMatchAttack = !isMatchAttack;
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
                currBtnImage.alienAttackOption = currAttack.rightAnswer;
            }
            else if (!firstWrongImageUsedFlag)
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

        if (currStepConfigTutorial.helpPopUp)
            ActivePopUp();

        for (int i = 0; i < skinObjAnim.Length; i++)
        {

            skinObjAnim[i].SetTrigger("Incorrect");
            skinObjAnimPopUp[i].SetTrigger("Incorrect");

        }

        incorrectParticles.Play();
        incorrectParticles.Play();

        


        if (!currStepConfigTutorial.wrongChoices) return;
        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        currPlayerHealth += gameConfigs.playerHealthLostOnWrongAnswer;

        OnRoundEnded();
    }

    private void OnCorrectChoice()
    {
        isCorrect = true;
        if (currStepConfigTutorial.helpPopUp)
            ActivePopUp();

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
        if ((currPlayerHealth <= 0 || currEnemyHealth <= 0) && currStepConfigTutorial.alienAttacksConfigsType == alienAttacksConfigsType.Random)
        {
            GameOver();
            return;
        }

        if(currPointsAmount == currStepConfigTutorial.stepsAmount)
        {
            currentTutorialStep += 1;
            InitTutorialStep();
        }

        InitRound();
    }

    void GameOver()
    {
        audiosource.clip = finishAudio;
        audiosource.Play();

        gameoverFlag = true;
        afterActionPanel.SetActive(true);
        inGameUIPaneltoDissapear.SetActive(false);
        afterActionFinalCoinsTxt.SetText(currCoins.ToString());
        gameConfigs.SaveCoins(currCoins);
        eogManager.OnGameOver();
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


