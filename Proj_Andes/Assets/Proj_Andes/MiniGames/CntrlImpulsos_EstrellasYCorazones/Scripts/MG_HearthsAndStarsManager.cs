using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MG_HearthsAndStarsManager : MonoBehaviour
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

    [SerializeField] ParticleSystem LCorrectparticle;
    [SerializeField] ParticleSystem RCorrectparticle;

    [SerializeField] GameObject afterActionPanel;
    [SerializeField] GameObject inGameUIPanel;
   
    [SerializeField] AudioClip correctAudio;
    [SerializeField] AudioClip wrongAudio;
    [SerializeField] AudioClip finishAudio;

    [Header("UI")]
    [SerializeField] TMP_Text currCoinsValueTxt;
    [SerializeField] TMP_Text currRoundValueTxt;
    [SerializeField] TMP_Text afterActionFinalCoinsTxt;
    [SerializeField] Button retryBtn;
    [SerializeField] Button retryBtn2;
    [SerializeField] Slider timerUI;

    private AudioSource audiosource; 

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
        audiosource = GetComponent<AudioSource>();


        afterActionPanel.SetActive(false);
		inGameUIPanel.SetActive(true);
        gameoverFlag = false;

        timerUI.minValue = 0;
        timerUI.maxValue = gameConfigs.timePerChoice;

		leftBtn.onClick.AddListener(OnClickedLeft);
		rightBtn.onClick.AddListener(OnClickedRight);
		retryBtn.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single));
		retryBtn2.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single));

		InitRound();
	}

	void InitRound()
    {
        timerPerChoice = 0;

		rightImg.gameObject.SetActive(false);
		leftImg.gameObject.SetActive(false);


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
        if (succed) OnCorrectChoice();
        else OnWrongChoice();
    }

    private void OnClickedRight()
    {

        var succed = false;
		if (currRequiresSameDirection && currShowingRight) succed = true;
		if (!currRequiresSameDirection && !currShowingRight) succed = true;
        if (succed)OnCorrectChoice();
     

        else OnWrongChoice();
	}

    private void OnWrongChoice()
    {
        audiosource.clip = wrongAudio;
        audiosource.Play();
        currCoins += gameConfigs.coinsOnWrongAnswer;
        currCoins = Mathf.Max(currCoins, gameConfigs.initialCoins);
        OnRoundEnded();
    }

    private void OnCorrectChoice()
    {
        RCorrectparticle.Stop();
        LCorrectparticle.Stop();
        
        audiosource.clip = correctAudio;
        audiosource.Play();
        currCoins += gameConfigs.coinsOnCorrectAnswer;
        if (currShowingRight)RCorrectparticle.Play();
        else LCorrectparticle.Play();
            
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

        Animator animatorImg = inGameUIPanel.GetComponent<Animator>();
        animatorImg.ResetTrigger("Appear");
        animatorImg.SetTrigger("Appear");

        InitRound();
    }

    void GameOver()
    {
        audiosource.clip = finishAudio;
        audiosource.Play();
        gameoverFlag = true;
		inGameUIPanel.SetActive(false);
        afterActionPanel.SetActive(true);
        afterActionFinalCoinsTxt.SetText(currCoins.ToString());
	}
}
