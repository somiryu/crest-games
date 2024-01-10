using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_FightTheAlienManagerTutorial : MonoBehaviour, IEndOfGameManager
{
    public Dictionary<string, bool> tutorialStepsDone = new Dictionary<string, bool>()
    {
        {tutorialFightTheAlien.match.ToString(), false},
        {tutorialFightTheAlien.noMatch.ToString(), false},
        {tutorialFightTheAlien.interval.ToString(), false},
        {tutorialFightTheAlien.winAlien.ToString(), false},
    };

    tutorialFightTheAlien currentTutorialStep;

    [SerializeField] MG_FightTheAlienGameConfigs gameConfigs;
    [Space(20)]
    [SerializeField] Image alienAttackImage;
    [Space(20)]
    [SerializeField] MG_FightTheAlienAnswerBtnTutorial [] answerBtns;

    [SerializeField] GameObject afterActionPanel;

    [Header("Game UI")]
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
    [SerializeField] Animator[] skinObjAnim;

    [Header("Posible Answer")]
    [SerializeField] AlienAttackConfig[] alienAttacksConfigs;
    [SerializeField] AlienAttackConfig[] alienAttacksConfigsMatch;
    [SerializeField] AlienAttackConfig[] alienAttacksConfigsNoMatch;


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
    }

    public void Init()
    {
        currentTutorialStep = tutorialFightTheAlien.match;
        audiosource = GetComponent<AudioSource>();
        skinObjAnim = skinObj.GetComponentsInChildren<Animator>(true);
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
            answerBtns[i].button.onClick.AddListener(() => OnAnswerBtnClicked(currIdx));
        }

        retryBtn2.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single));

        InitRound();
    }



    void InitRound()
    {
        timerPerChoice = 0;
        eogManager.OnGameStart();
        var randomAttack = Random.Range(0, alienAttacksConfigs.Length);
        var currConfig = alienAttacksConfigs[randomAttack];

        alienAttackImage.sprite = currConfig.attackSprite;

        currCorrectAnswerIdx = Random.Range(0, 3);

        var firstWrongImageUsedFlag = false;

        for (int i = 0; i < 3; i++)
        {
            var currBtnImage = answerBtns[i];
            if (i == currCorrectAnswerIdx)
            {
                currBtnImage.SetAnswerImage(currConfig.rightAnswer);
                if (currentTutorialStep == tutorialFightTheAlien.match || currentTutorialStep == tutorialFightTheAlien.noMatch)
                    currBtnImage.ShowHighlightImg(true);
            }
            else if (!firstWrongImageUsedFlag)
            {
                currBtnImage.SetAnswerImage(currConfig.wrongAnswer1);
                firstWrongImageUsedFlag = true;
                if (currentTutorialStep == tutorialFightTheAlien.match || currentTutorialStep == tutorialFightTheAlien.noMatch)
                    currBtnImage.ShowHighlightImg(false);
            }
            else
            { 
                currBtnImage.SetAnswerImage(currConfig.wrongAnswer2);
                if (currentTutorialStep == tutorialFightTheAlien.match || currentTutorialStep == tutorialFightTheAlien.noMatch)
                    currBtnImage.ShowHighlightImg(false);
            }
        }
    }

    private void Update()
    {
        if (gameoverFlag) return;

        timerUI.value = timerPerChoice;
        timerPerChoice += Time.deltaTime;
        if (timerPerChoice >= gameConfigs.timePerChoice)
        {
            timerPerChoice = 0;
            OnWrongChoice();
        }
    }

    void OnAnswerBtnClicked(int idx)
    {
        if (idx == currCorrectAnswerIdx) OnCorrectChoice();
        else OnWrongChoice();
    }
    private void OnWrongChoice()
    {
        correctParticles.Stop();
        incorrectParticles.Stop();

        audiosource.clip = wrongAudio;
        audiosource.Play();

        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        currPlayerHealth += gameConfigs.playerHealthLostOnWrongAnswer;

        for (int i = 0; i < skinObjAnim.Length; i++)
        {

            skinObjAnim[i].SetTrigger("Incorrect");

        }

        incorrectParticles.Play();
        incorrectParticles.Play();
        OnRoundEnded();
    }

    private void OnCorrectChoice()
    {
        correctParticles.Stop();
        incorrectParticles.Stop();

        audiosource.clip = correctAudio;
        audiosource.Play();

        correctParticles.Play();
        for (int i = 0; i < skinObjAnim.Length; i++)
        {

            skinObjAnim[i].SetTrigger("Correct");
        }


        currCoins += gameConfigs.coinsOnCorrectAnswer;
        currEnemyHealth += gameConfigs.EnemyHealthLostOnRightAnswer;
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        currCoinsValueTxt.text = currCoins.ToString();
        playerHealthUI.value = currPlayerHealth;
        enemyHealthUI.value = currEnemyHealth;
        if (currPlayerHealth <= 0 || currEnemyHealth <= 0)
        {
            GameOver();
            return;
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
}

public enum tutorialFightTheAlien
{
    match,
    noMatch,
    interval,
    winAlien,
}
