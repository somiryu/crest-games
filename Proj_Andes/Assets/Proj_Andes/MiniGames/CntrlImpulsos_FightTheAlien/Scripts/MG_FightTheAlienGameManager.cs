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

	public void Awake()
	{
        currAlienAttacksConfigs = alienAttacksConfigsMatch.Concat(alienAttacksConfigsNoMatch).ToArray();
        Init();
	}

	public void Init()
    {
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
            answerBtns[i].onClick.AddListener(() => OnAnswerBtnClicked(currIdx));
        }

		InitRound();
	}

   

	void InitRound()
    {
        timerPerChoice = 0;
        eogManager.OnGameStart();
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

        for (int i = 0; i < skinObjAnim.Length ; i++)
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