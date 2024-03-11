using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_FightTheAlienManager : MonoBehaviour, IEndOfGameManager
{
    static MG_FightTheAlienManager instance;
    public static MG_FightTheAlienManager Instance => instance;
    [SerializeField] MG_FightTheAlienGameConfigs gameConfigs;
    [Space(20)]
    [SerializeField] Image alienAttackImage;
    [Space(20)]
    [SerializeField] Button[] answerBtns;
    [Space(20)]
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
    [SerializeField] GameObject inGameUIPaneltoDissapear;
    [SerializeField] GameObject inGameGameObjtoDissapear;


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
    [SerializeField] AlienAttackConfig[] alienAttacksConfigsMatch;
    [SerializeField] AlienAttackConfig[] alienAttacksConfigsNoMatch;
    AlienAttackConfig[] currAlienAttacksConfigs;

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;

    private float timerPerChoice = 0;
    private int currCoins;
    private int currCorrectAnswerIdx;
    private int currPlayerHealth;
    private int currEnemyHealth;
    private AudioSource audiosource;

    private bool gameoverFlag = false;
    float totalTime;

    MG_FightTheAlien_RoundAnalytics roundAnalytics;
    public List<MG_FightTheAlien_RoundAnalytics> AllRoundsAnalytics;

    public void Awake()
	{
        if (instance != null && instance != this) DestroyImmediate(instance);
        instance = this;
        currAlienAttacksConfigs = alienAttacksConfigsMatch.Concat(alienAttacksConfigsNoMatch).ToArray();
        Init();
	}

	public void Init()
    {
        audiosource = GetComponent<AudioSource>();
        skinObjAnim = skinObj.GetComponentsInChildren<Animator>(true);

        AllRoundsAnalytics = new List<MG_FightTheAlien_RoundAnalytics>(gameConfigs.EnemyHealth);

        currCoins = gameConfigs.initialCoins;
        currPlayerHealth = gameConfigs.PlayerHealth;
        currEnemyHealth = gameConfigs.EnemyHealth;

		afterActionPanel.SetActive(false);
		inGameUIPaneltoDissapear.SetActive(true);
		inGameGameObjtoDissapear.SetActive(true);
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
            answerBtns[i].onClick.AddListener(() => OnAnswerBtnClicked(currIdx));
        }

        totalTime = 0;
		InitRound();
	}

    void Start()
    {
        GeneralGameAnalyticsManager.Instance.Init(DataIds.fightTheAlienGame);
    }

	void InitRound()
    {
        timerPerChoice = 0;
        eogManager.OnGameStart();

        roundAnalytics = new MG_FightTheAlien_RoundAnalytics();
        AllRoundsAnalytics.Add(roundAnalytics);
        roundAnalytics.clicks = 0;

        var randomAttack = Random.Range(0, currAlienAttacksConfigs.Length);
        var currConfig = currAlienAttacksConfigs[randomAttack];

        alienAttackImage.sprite = currConfig.alienAtack.attackSprite;

		currCorrectAnswerIdx = Random.Range(0, 3);

        var firstWrongImageUsedFlag = false;

        for (int i = 0; i < 3; i++)
        {
            var currBtnImage = answerBtns[i].targetGraphic as Image;
            if (i == currCorrectAnswerIdx) currBtnImage.sprite = currConfig.rightAnswer.attackSprite;
            else if (!firstWrongImageUsedFlag)
            {
                currBtnImage.sprite = currConfig.wrongAnswer1.attackSprite;
                firstWrongImageUsedFlag=true;
            }
            else currBtnImage.sprite = currConfig.wrongAnswer2.attackSprite;
		}
        if (currConfig.rightAnswer.shapeAlienAttack == currConfig.alienAtack.shapeAlienAttack) roundAnalytics.challengeOrder = "Same";
        else roundAnalytics.challengeOrder = "Different";
    }

	private void Update()
	{
        if (gameoverFlag) return;

        totalTime += Time.deltaTime;

        timerUI.value = timerPerChoice;
        timerPerChoice += Time.deltaTime;
        if(Input.GetMouseButtonDown(0)) roundAnalytics.clicks++;
        if (timerPerChoice >= gameConfigs.timePerChoice)
        {
            timerPerChoice = 0;
            roundAnalytics.ranOutOfTime = true;
            OnWrongChoice();
        }
    }

	void OnAnswerBtnClicked(int idx)
	{
        roundAnalytics.ranOutOfTime = false;
        if (idx == currCorrectAnswerIdx) OnCorrectChoice();
        else OnWrongChoice();
	}
    private void OnWrongChoice()
    {
        correctParticles.Stop();
        incorrectParticles.Stop();

        GeneralGameAnalyticsManager.RegisterLose();
        GameUIController.Instance.StarLost();
        audiosource.clip = wrongAudio;
        audiosource.Play();

        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        currPlayerHealth += gameConfigs.playerHealthLostOnWrongAnswer;

        for (int i = 0; i < skinObjAnim.Length ; i++)
        {

            skinObjAnim[i].SetTrigger("Correct");

        }
        roundAnalytics.wonRound = false;

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

        GeneralGameAnalyticsManager.RegisterWin();
		GameUIController.Instance.StarEarned(Input.mousePosition);


		correctParticles.Play();
        for (int i = 0; i < skinObjAnim.Length; i++)
        {

            skinObjAnim[i].SetTrigger("Incorrect");
        }
        roundAnalytics.wonRound = true;

        currCoins += gameConfigs.coinsOnCorrectAnswer;
        currEnemyHealth += gameConfigs.EnemyHealthLostOnRightAnswer;
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        roundAnalytics.timeToMakeAChoice = timerPerChoice;

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
        gameConfigs.SaveAnalytics();
        gameoverFlag = true;
        afterActionPanel.SetActive(true);
        inGameUIPaneltoDissapear.SetActive(false);
        inGameGameObjtoDissapear.SetActive(false);
		afterActionFinalCoinsTxt.SetText(currCoins.ToString());
        gameConfigs.SaveCoins(currCoins);
        eogManager.OnGameOver();
	}
}

[Serializable]
public struct AlienAttackConfig
{
    public AlienAttackOption alienAtack;
    public AlienAttackOption wrongAnswer1;
    public AlienAttackOption wrongAnswer2;
    public AlienAttackOption rightAnswer;
}

[Serializable]
public class AlienAttackOption
{
    public Sprite attackSprite;
    public colorAlienAttackConfig colorAlienAttack;
    public shapeAlienAttackConfig shapeAlienAttack;
}

public enum colorAlienAttackConfig
{
    blue,
    red,
    green
}

public enum shapeAlienAttackConfig
{
    Circle,
    Heart,
    Star
}

public class MG_FightTheAlien_RoundAnalytics
{
    public string challengeOrder = "NONE";
    public bool wonRound = false;
    public float timeToMakeAChoice = 0;
    public int clicks = 0;
    public bool ranOutOfTime = false;
}