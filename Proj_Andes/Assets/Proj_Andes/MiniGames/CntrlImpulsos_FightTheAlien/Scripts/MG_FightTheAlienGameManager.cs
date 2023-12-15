using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_FightTheAlienManager : MonoBehaviour
{
	[SerializeField] MG_FightTheAlienGameConfigs gameConfigs;
    [Space(20)]
    [SerializeField] Image alienAttackImage;
	[Space(20)]
	[SerializeField] Button[] answerBtns;

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

    [Header("Posible Answer")]
	[SerializeField] AlienAttackConfig[] alienAttacksConfigs;


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
        audiosource = GetComponent<AudioSource>();

        currCoins = gameConfigs.initialCoins;
        currPlayerHealth = gameConfigs.PlayerHealth;
        currEnemyHealth = gameConfigs.EnemyHealth;

		afterActionPanel.SetActive(false);
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

		retryBtn.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single));
		retryBtn2.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single));

		InitRound();
	}

   

	void InitRound()
    {
        timerPerChoice = 0;

        var randomAttack = Random.Range(0, alienAttacksConfigs.Length);
        var currConfig = alienAttacksConfigs[randomAttack];

        alienAttackImage.sprite = currConfig.attackSprite;

		currCorrectAnswerIdx = Random.Range(0, 3);

        var firstWrongImageUsedFlag = false;

        for (int i = 0; i < 3; i++)
        {
            var currBtnImage = answerBtns[i].targetGraphic as Image;
            if (i == currCorrectAnswerIdx) currBtnImage.sprite = currConfig.rightAnswer;
            else if (!firstWrongImageUsedFlag)
            {
                currBtnImage.sprite = currConfig.wrongAnswer1;
                firstWrongImageUsedFlag=true;
            }
            else currBtnImage.sprite = currConfig.wrongAnswer2;
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
        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        currPlayerHealth += gameConfigs.playerHealthLostOnWrongAnswer;
        OnRoundEnded();
    }

    private void OnCorrectChoice()
    {
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
        gameoverFlag = true;
        afterActionPanel.SetActive(true);
		afterActionFinalCoinsTxt.SetText(currCoins.ToString());
	}
}

[Serializable]
public struct AlienAttackConfig
{
    public Sprite attackSprite;
    public Sprite wrongAnswer1;
    public Sprite wrongAnswer2;
    public Sprite rightAnswer;
}
