using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MG_HearthsAndStarsManager : MonoBehaviour, IEndOfGameManager
{
	[SerializeField] MG_HearthAndStarsGameConfigs gameConfigs;
	[Space(20)]
	[SerializeField] Sprite sameDirectionSprite;
    [SerializeField] Sprite opositeDirectionSprite;
    [Space(20)]
    [SerializeField] Image leftImg;
    [SerializeField] Image rightImg;
	[Space(20)]
	[SerializeField] Button leftBtn;
    [SerializeField] Button rightBtn;

    [SerializeField] GameObject afterActionPanel;

    [Header("UI")]
    [SerializeField] TMP_Text currCoinsValueTxt;
    [SerializeField] TMP_Text currRoundValueTxt;
    [SerializeField] TMP_Text afterActionFinalCoinsTxt;
    [SerializeField] Slider timerUI;

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;

    private float timerPerChoice = 0;
    private int currCoins;
    private int currRound;

    private bool currShowingRight = false;
    private bool currRequiresSameDirection = false;

    private bool gameoverFlag = false;

	public void Awake()
	{
        Init();
	}

	public void Init()
    {
        currCoins = gameConfigs.initialCoins;
        currRound = 0;

		afterActionPanel.SetActive(false);
        gameoverFlag = false;

        timerUI.minValue = 0;
        timerUI.maxValue = gameConfigs.timePerChoice;

		leftBtn.onClick.AddListener(OnClickedLeft);
		rightBtn.onClick.AddListener(OnClickedRight);

        InitRound();
	}

	void InitRound()
    {
        timerPerChoice = 0;

		rightImg.gameObject.SetActive(false);
		leftImg.gameObject.SetActive(false);

        eogManager.OnGameStart();

		currRequiresSameDirection = Random.Range(0f, 1f) > 0.5f;
        var spriteToShow = currRequiresSameDirection ? sameDirectionSprite : opositeDirectionSprite;
		currShowingRight = Random.Range(0f, 1f) > 0.5f;
        var imgToUse = currShowingRight? rightImg: leftImg;
        imgToUse.gameObject.SetActive(true);
        imgToUse.sprite = spriteToShow;
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

	private void OnClickedLeft()
    {
        var succed = false;
        if (!currRequiresSameDirection && currShowingRight) succed = true;
        if (currRequiresSameDirection && !currShowingRight) succed = true;
        if(succed) OnCorrectChoice();
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
        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        OnRoundEnded();
    }

    private void OnCorrectChoice()
    {
        currCoins += gameConfigs.coinsOnCorrectAnswer;
        OnRoundEnded();
    }

    void OnRoundEnded()
    {
        currRound++;
        currCoinsValueTxt.text = currCoins.ToString();
        currRoundValueTxt.text = currRound.ToString();
        if(currRound >= gameConfigs.maxRounds)
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
        eogManager.OnGameOver();
	}
}
