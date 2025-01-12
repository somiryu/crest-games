using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.EventSystems;


#if UNITY_EDITOR
using UnityEditor;
#endif

public interface iMonsterMarketButton
{    
    public void SetMarketButtonToCurrState();
}
public class MonsterMarketManager : MonoBehaviour, ITimeManagement
{
    static MonsterMarketManager instance;

    public static MonsterMarketManager Instance => instance;
    [SerializeField] MonsterMarketConfig marketConfig;

    [SerializeField] Transform chestOpenedContainer;
    [SerializeField] Image chestOpenedContainerImg;
    [SerializeField] GameObject chestNoEnoughCoins;
    [SerializeField] Button closeNoResourcesBtn;

    [SerializeField] Button regularChest;
    [SerializeField] Button rareChest;
    [SerializeField] Button legendaryChest;

    [SerializeField] List<MonsterMarketButtonBehaviour> userMonsterButtonBehaviours;

    [SerializeField] Button saveForLaterButton;
    [SerializeField] Button confirmButton;
    [SerializeField] Button collectBtn;
    [SerializeField] TextMeshProUGUI coinsAmtTxt;
    [SerializeField] Button chestOpenButton;
    [SerializeField] GameObject chestOpenButtonParent;
    [SerializeField] Image chestOpenImg;
    [SerializeField] Image chestclosedImg;

    [SerializeField] MyCollectionManager myCollectionManager;

    public Pool<MonsterItemUI> monstersUIInChestOpenning;
    List<Monsters> totalDataCollection => MyCollectionManager.totalDataCollection;
    List<Monsters> currentMonstersFound = new List<Monsters>();
    public List<Image> replaceableImg = new List<Image>();

    MonsterMarketButtonBehaviour currButton;

    Button currSelectedButton;
    AudioSource audioSource;
    [SerializeField] AudioClip openChestSound;
    [SerializeField] AudioClip titleAudio;
    [SerializeField] AudioClip introAudio;
    [SerializeField] AudioClip expensiveChestsAudio;
    [SerializeField] AudioClip intermidiateGamesIntro;
    [SerializeField] AudioClip selectRedBoxAudio;
    [SerializeField] AudioClip uHaveAmtStarsAudio;
    [SerializeField] AudioClip openItAndGetAGiftAudio;
    [SerializeField] AudioClip uWonAMonsterAudio;
    [SerializeField] AudioClip wantMoreAudio;
    [SerializeField] AudioClip clicContinue;
    [SerializeField] AudioClip noResourcesAudio;
    [SerializeField] AudioClip noStarsAudio;
    [SerializeField] AudioClip continueAudio;
    [SerializeField] AudioClip checkAudio;
    [SerializeField] AudioClip lastChanceAudio;
    [SerializeField] Transform blockButtons;
    [SerializeField] Transform tutoHand;
    [SerializeField] MonsterMarketButtonBehaviour continueBtn;
    IEnumerator marketIntro;
    IEnumerator noResources;
    IEnumerator openChest;

    //Analytics
    float timerPerChestOpenning;
    bool openedAtLeastOneChest;
    float totalTime;
    string chestTypeOpenedString;
    int chestTypeOpenned;
    int initialStars;
    int finalStars;
    int starsSpent;

	[SerializeField] public List<MonsterMarketRoundAnalytic> AllRoundsAnalytics;

    public MonsterMarketRoundAnalytic currAnalyticRound => AllRoundsAnalytics[AllRoundsAnalytics.Count - 1];




	private void Awake()
    {
        if(instance != null)
        {
            if(instance != this)
            {
                DestroyImmediate(this);
            }
        }
        instance = this;
        MonsterMarketConfig.marketAppearTimes++;
        MonsterMarketConfig.isLastMarket = GameSequencesList.Instance.IsLastMarket(marketConfig);
        Debug.LogWarning("awake monster market");
		Init();
	}

	private void Start()
	{
        if (marketIntro != null) StopCoroutine(marketIntro);
        marketIntro = MarketIntro();
		StartCoroutine(marketIntro);
		//Init analytics
		GeneralGameAnalyticsManager.Instance.Init(DataIds.monsterMarket);
		initialStars = UserDataManager.CurrUser.Coins;
        chestOpennedOrRoundCount = 0;
        timerPerChestOpenning = 0;
	}

	public void Init()
	{
		TryGetComponent(out audioSource);
		myCollectionManager.Init(false);
		myCollectionManager.HideCollection();
		confirmButton.gameObject.SetActive(false);
		chestOpenButtonParent.gameObject.SetActive(false);
		RefreshCollectionFromData();

		monstersUIInChestOpenning.Init(5);
		monstersUIInChestOpenning.RecycleAll();

        openedAtLeastOneChest = false;
		chestOpenedContainer.gameObject.SetActive(false);

		confirmButton.onClick.AddListener(OpenButtonBeforeBuyChestOrContinuing);
		chestOpenButton.onClick.AddListener(BuyChest);
		collectBtn.onClick.AddListener(Collect);

		regularChest.onClick.AddListener(() => ActivateConfirmationButton(MonsterChestType.Regular, regularChest));
		rareChest.onClick.AddListener(() => ActivateConfirmationButton(MonsterChestType.Rare, rareChest));
		legendaryChest.onClick.AddListener(() => ActivateConfirmationButton(MonsterChestType.Legendary, legendaryChest));
		saveForLaterButton.onClick.AddListener(() => ActivateConfirmationButton(MonsterChestType.NONE, saveForLaterButton));

		coinsAmtTxt.text = marketConfig.AvailableCoins.ToString();
		noResources = NoResources();
		openChest = OpenChestAudios();
		closeNoResourcesBtn.onClick.AddListener(CloseNoResources);
		Debug.LogWarning("finish init monster market");
	}



	IEnumerator MarketIntro()
    {
        bool alreadyReactivatedBlockButton = false;
		blockButtons.gameObject.SetActive(true);
		if (MonsterMarketConfig.isLastMarket)
        {
            audioSource.clip = lastChanceAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(lastChanceAudio.length);
            blockButtons.gameObject.SetActive(false);
            yield break;
        }


        audioSource.clip = titleAudio;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(titleAudio.length);


		if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction)) continueBtn.button.interactable = false;
        else
        {
			continueBtn.button.interactable = false;
			continueBtn.SetInactiveState();
            audioSource.clip = intermidiateGamesIntro;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(intermidiateGamesIntro.length);
            audioSource.clip = selectRedBoxAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(selectRedBoxAudio.length);
            blockButtons.gameObject.SetActive(false);
			alreadyReactivatedBlockButton = true;
            yield return new WaitForSecondsRealtime(2);
            continueBtn.SetActiveState();
			continueBtn.button.interactable = true;
		}

        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
        {
            audioSource.clip = introAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(introAudio.length);
            audioSource.clip = expensiveChestsAudio; 
            audioSource.Play();
            yield return new WaitForSecondsRealtime(expensiveChestsAudio.length);
            audioSource.clip = uHaveAmtStarsAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(uHaveAmtStarsAudio.length);

			tutoHand.gameObject.SetActive(true);
            audioSource.clip = openItAndGetAGiftAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(openItAndGetAGiftAudio.length);
        }

        marketIntro = null;
        if(!alreadyReactivatedBlockButton) blockButtons.gameObject.SetActive(false);
	}

	public void AddUserInterfaceMonsterButton(MonsterMarketButtonBehaviour monsterMarketButtonBehaviour)
    {
        userMonsterButtonBehaviours.Add(monsterMarketButtonBehaviour);
    }

    public void RemoveUserInterfaceMonsterButton(MonsterMarketButtonBehaviour monsterMarketButtonBehaviour)
    {
        userMonsterButtonBehaviours.Remove(monsterMarketButtonBehaviour);
    }

 
    IEnumerator PlaySelectMonsters()
    {
        blockButtons.gameObject.SetActive(true);
        continueBtn.SetInactiveState();
        continueBtn.button.interactable = false;
        audioSource.clip = intermidiateGamesIntro;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(intermidiateGamesIntro.length);
        audioSource.clip = selectRedBoxAudio;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(selectRedBoxAudio.length);
        blockButtons.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(2);
        continueBtn.SetActiveState();
		continueBtn.button.interactable = true;
	}

	void OnCollectionsClosed()
    {
        UpdateStateButtons();
        if (UserDataManager.CurrUser.Coins == 0) SaveForLater();
        myCollectionManager.OnClosedCollections -= OnCollectionsClosed;

        if (UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction) 
            && UserDataManager.CurrUser.Coins >= 5
            && !MonsterMarketConfig.isLastMarket)
        {
            StartCoroutine(PlaySelectMonsters());
        }

        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
		{
			StartCoroutine(OrSelectContinue());
		}
	}

    void ActivateConfirmationButton(MonsterChestType monsterChestType, Button chestBtn) 
    {
        if(currButton != null) currButton.SetHighlightState(false);
        currSelectedButton = chestBtn;
        if(monsterChestType == MonsterChestType.Regular) tutoHand.gameObject.SetActive(false);
        chestBtn.TryGetComponent<MonsterMarketButtonBehaviour>(out currButton);

        var btnToActive = userMonsterButtonBehaviours.Find(x => x.monsterMarketButton.monsterChestType == monsterChestType);
        if (btnToActive == null) return;
        if(btnToActive.monsterMarketButton.costChest > marketConfig.AvailableCoins)
        {
            ShowNoResourcesCorroutine();
            confirmButton.gameObject.SetActive(false);
            return;
        }
		currButton.SetHighlightState(true);
		currButton.SetActiveState();
        confirmButton.gameObject.SetActive(true);
    }

    void OpenButtonBeforeBuyChestOrContinuing()
    {
        audioSource.clip = checkAudio;
        audioSource.Play();
        if (currSelectedButton == saveForLaterButton)
        {
            SaveForLater();
            return;
        }

        chestOpenButtonParent.gameObject.SetActive(true);
        chestOpenImg.sprite = currButton.monsterMarketButton.chestCloseSprite;
        chestclosedImg.sprite = currButton.monsterMarketButton.chestCloseSprite;
        confirmButton.gameObject.SetActive(false);
    }

    void BuyChest()
    {
        chestOpenButtonParent.gameObject.SetActive(false);
        switch (currButton.monsterMarketButton.monsterChestType)
        {
            case MonsterChestType.Regular:

                if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
                {
                    continueBtn.button.interactable = true;
                }
				SaveNewChestOpenningnalytic(MonsterChestType.Regular, marketConfig.RegularChestPrice);
				marketConfig.ConsumeCoins(marketConfig.RegularChestPrice);
                starsSpent += marketConfig.RegularChestPrice;
                chestTypeOpenedString = "Peque�o";
                chestTypeOpenned = 1;
                OpenChest(1, 0, 0);
                GeneralGameAnalyticsManager.RegisterLose();
                break;
            case MonsterChestType.Rare:
				SaveNewChestOpenningnalytic(MonsterChestType.Rare, marketConfig.RareChestPrice);
				marketConfig.ConsumeCoins(marketConfig.RareChestPrice);
				starsSpent += marketConfig.RareChestPrice;
                chestTypeOpenedString = "Mediano";
                chestTypeOpenned = 2;
				OpenChest(1, 1, 0);
                break;
            case MonsterChestType.Legendary:
                SaveNewChestOpenningnalytic(MonsterChestType.Legendary,marketConfig.LegendaryChestPrice);
				marketConfig.ConsumeCoins(marketConfig.LegendaryChestPrice);
				starsSpent += marketConfig.LegendaryChestPrice;
                chestTypeOpenedString = "Grande";
				chestTypeOpenned = 3;
				OpenChest(1, 1, 1);
				GeneralGameAnalyticsManager.RegisterWin();
				break;
        }
        coinsAmtTxt.text = marketConfig.AvailableCoins.ToString();
         
        UpdateStateButtons();
    }

    void Update()
    {
        timerPerChestOpenning += Time.deltaTime;
    }

    int chestOpennedOrRoundCount = 0;

    void SaveNewChestOpenningnalytic(MonsterChestType type, int cost)
    {
        chestOpennedOrRoundCount++;
        var newAnalytic = new MonsterMarketRoundAnalytic();
        newAnalytic.time = timerPerChestOpenning;
        timerPerChestOpenning = 0;
        switch (type)
        {
            case MonsterChestType.Legendary:
                newAnalytic.chestTypeOpenedString = "Grande";
                newAnalytic.chestTypeOpenned = 3;
                break;
            case MonsterChestType.Rare:
				newAnalytic.chestTypeOpenedString = "Mediano";
				newAnalytic.chestTypeOpenned = 1;
				break;
            case MonsterChestType.Regular:
				newAnalytic.chestTypeOpenedString = "Peque�o";
				newAnalytic.chestTypeOpenned = 0;
				break;
        }
        newAnalytic.marketIndex = MonsterMarketConfig.marketAppearTimes;
        newAnalytic.chestCount = chestOpennedOrRoundCount;
        newAnalytic.initialStars = UserDataManager.CurrUser.Coins;
        newAnalytic.starsSpent = cost;
        newAnalytic.finalStars = newAnalytic.initialStars - cost;
        AllRoundsAnalytics.Add(newAnalytic);
    }

    void ShowNoResourcesCorroutine()
    {
        if(noResources != null) StopCoroutine(noResources);
        noResources = NoResources();
        StartCoroutine(noResources);
    }

    IEnumerator NoResources()
    {
        blockButtons.gameObject.SetActive(true);
        audioSource.clip = noResourcesAudio;
        audioSource.Play();
		yield return new WaitForSeconds(noResourcesAudio.length-0.5f);
        audioSource.clip = noStarsAudio;
        audioSource.Play();
		yield return new WaitForSeconds(noStarsAudio.length);
		blockButtons.gameObject.SetActive(false);
        UpdateStateButtons();
        noResources = null;
    }

    void CloseNoResources()
    {
        if(noResources != null) StopCoroutine(noResources);
        audioSource.Stop();
        chestNoEnoughCoins.SetActive(false);
    }

    private void UpdateStateButtons()
    {
        for (int i = 0; i < userMonsterButtonBehaviours.Count; i++)
        {
            userMonsterButtonBehaviours[i].SetMarketButtonToCurrState();
        }

    }

    IEnumerator OpenChestAudios()
    {
        blockButtons.gameObject.SetActive(!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction));
        audioSource.clip = openChestSound; 
        audioSource.Play();
        yield return new WaitForSeconds(openChestSound.length-0.4f);
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
        {
			audioSource.clip = uWonAMonsterAudio; 
            audioSource.Play();
            yield return new WaitForSeconds(uWonAMonsterAudio.length);
            audioSource.clip = wantMoreAudio;
            audioSource.Play();
            yield return new WaitForSeconds(wantMoreAudio.length);
            blockButtons.gameObject.SetActive(false);
        }
        openChest = null;
    }

    void OpenChest(int regularMonstersAmount, int rareMonstersAmount, int legendaryMonstersAmount)
    {
        openedAtLeastOneChest = true;

        chestOpenButtonParent.gameObject.SetActive(false);
        chestOpenedContainer.gameObject.SetActive(true);
        saveForLaterButton.gameObject.SetActive(true);
        chestOpenedContainerImg.sprite = currButton.monsterMarketButton.chestOpenSprite;
        currentMonstersFound.Clear();
        MonsterMarketConfig.openChestTrials++;
        if(openChest != null) StopCoroutine(openChest);
        openChest = OpenChestAudios();
        StartCoroutine(openChest);

        for (int i = 0; i < regularMonstersAmount; i++) currentMonstersFound.Add(GetNewRandomMonster(MonsterChestType.Regular, 0));
        for (int i = 0; i < rareMonstersAmount; i++) currentMonstersFound.Add(GetNewRandomMonster(MonsterChestType.Rare, 0));
        for (int i = 0; i < legendaryMonstersAmount; i++) currentMonstersFound.Add(GetNewRandomMonster(MonsterChestType.Legendary, 0));

        monstersUIInChestOpenning.RecycleAll();
        for (int i = 0; i < replaceableImg.Count; i++) replaceableImg[i].gameObject.SetActive(false);

        for (int i = 0; i < currentMonstersFound.Count; i++)
        {
            monstersUIInChestOpenning.GetNewItem();
            replaceableImg[i].sprite = currentMonstersFound[i].sprite;
            replaceableImg[i].gameObject.SetActive(true);
        }
    }

    public Monsters GetNewRandomMonster(MonsterChestType monsterChestType, int tryNumber)
    {
        tryNumber++;
        var newMonster = marketConfig.monstersLibrary.GetRandomMonster(monsterChestType);
        if (totalDataCollection.Exists(x => x.guid == newMonster.guid) && tryNumber < 100) return GetNewRandomMonster(monsterChestType, tryNumber);
        else return newMonster;
    }

    public void RefreshCollectionFromData() => myCollectionManager.RefreshCollectionFromData();

    public void ActivateContinueSound()
    {
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
        {
            UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.Market_Instruction.ToString());
        }
        audioSource.clip = continueAudio;
        audioSource.Play();
    }

    void Collect()
    {
        chestOpenedContainer.gameObject.SetActive(false);

        for (int i = 0; i < currentMonstersFound.Count; i++)
        {
            var alreadyIn = totalDataCollection.Exists(x => x.guid == currentMonstersFound[i].guid);
            if (!alreadyIn) marketConfig.AddMonsterToCollection(currentMonstersFound[i]);
        }
        RefreshCollectionFromData();
		myCollectionManager.ShowCollection();
		myCollectionManager.OnClosedCollections += OnCollectionsClosed;
		myCollectionManager.gameObject.SetActive(true);

    }
    public IEnumerator OrSelectContinue()
    {
        blockButtons.gameObject.SetActive(true);
        audioSource.clip = clicContinue;
        audioSource.Play();
        yield return new WaitForSeconds(clicContinue.length);
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
        {
            UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.Market_Instruction.ToString());
        }
        blockButtons.gameObject.SetActive(false);
    }

#if UNITY_EDITOR

    [MenuItem("Hi Hat Games/Give100Coins")]
    public static void Give100Coins()
    {
        UserDataManager.CurrUser.Coins += 100;
    }
#endif

    public void SaveForLater()
    {
        if (!openedAtLeastOneChest) chestTypeOpenedString = "Saltar"; 
        finalStars = UserDataManager.CurrUser.Coins;
        totalTime = GeneralGameAnalyticsManager.Instance.analytics.timePlayed;
        chestOpennedOrRoundCount++;
        var newAnalytic = new MonsterMarketRoundAnalytic();
		newAnalytic.time = timerPerChestOpenning;
		newAnalytic.chestTypeOpenedString = "Saltar";
		newAnalytic.chestTypeOpenned = 4;

		newAnalytic.marketIndex = MonsterMarketConfig.marketAppearTimes;
		newAnalytic.chestCount = chestOpennedOrRoundCount;
		newAnalytic.initialStars = initialStars;
		newAnalytic.starsSpent = starsSpent;
		newAnalytic.finalStars = finalStars;
		AllRoundsAnalytics.Add(newAnalytic);


        marketConfig.SetAnalyticsInfo(AllRoundsAnalytics);

		chestOpenedContainer.gameObject.SetActive(false);
        marketConfig.OnSequenceOver();
    }
}

public enum MonsterChestType
{
    Regular,
    Rare,
    Legendary,
    NONE
}

[Serializable]
public class Monsters
{
    public string Name;
    public string guid = new Guid().ToString();
    public MonsterChestType monsterType;
    public int monsterIndex;
    public Sprite sprite;
    public Sprite cardBk;

    public void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = Guid.NewGuid().ToString();  
        }
        var currName = monsterType.ToString() + " : " + sprite.name;
		if (Name != currName)
        {
            Name = currName;
        }
    }
}


[Serializable]
public class MonsterMarketRoundAnalytic
{
	public float time;
	public float marketIndex;
	public string chestTypeOpenedString;
	public int chestTypeOpenned;
	public int initialStars;
	public int finalStars;
	public int starsSpent;
    public int chestCount;
}