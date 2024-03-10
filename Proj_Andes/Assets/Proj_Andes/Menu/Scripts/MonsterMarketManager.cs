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
    public void SetLockedImage();
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
    [SerializeField] AudioClip uHaveAmtStarsAudio;
    [SerializeField] AudioClip openItAndGetAGiftAudio;
    [SerializeField] AudioClip uWonAMonsterAudio;
    [SerializeField] AudioClip wantMoreAudio;
    [SerializeField] AudioClip clicContinue;
    [SerializeField] AudioClip noResourcesAudio;
    [SerializeField] AudioClip noStarsAudio;
    [SerializeField] AudioClip continueAudio;
    [SerializeField] AudioClip checkAudio;
    [SerializeField] Transform blockButtons;
    [SerializeField] Transform tutoHand;
    [SerializeField] Button continueBtn;
    IEnumerator marketIntro;
    IEnumerator noResources;
    IEnumerator openChest;

    //Analytics
    float timeUntilFirstChestOpen;
    float totalTime;
    bool opennedAtLeastOneChest;
    int chestTypeOpenned;
    int initialStars;
    int finalStars;
    int starsSpent;


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
        Init();
    }

    IEnumerator MarketIntro()
    {
        audioSource.clip = titleAudio; 
        audioSource.Play();
        TimeManager.Instance.SetNewStopTimeUser(this);
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction)) continueBtn.interactable = false;
        blockButtons.gameObject.SetActive(!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction));
        yield return new WaitForSecondsRealtime(titleAudio.length);
        audioSource.clip = introAudio;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(introAudio.length);
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
        {
            audioSource.clip = uHaveAmtStarsAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(uHaveAmtStarsAudio.length);
            tutoHand.gameObject.SetActive(true);
            audioSource.clip = openItAndGetAGiftAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(openItAndGetAGiftAudio.length);
        }
        else
        {
            continueBtn.interactable = true;
            audioSource.clip = clicContinue;
            audioSource.Play();
            yield return new WaitForSeconds(clicContinue.length);
        }
        blockButtons.gameObject.SetActive(false);
        TimeManager.Instance.RemoveNewStopTimeUser(this);
    }

    public void AddUserInterfaceMonsterButton(MonsterMarketButtonBehaviour monsterMarketButtonBehaviour)
    {
        userMonsterButtonBehaviours.Add(monsterMarketButtonBehaviour);
    }

    public void RemoveUserInterfaceMonsterButton(MonsterMarketButtonBehaviour monsterMarketButtonBehaviour)
    {
        userMonsterButtonBehaviours.Remove(monsterMarketButtonBehaviour);
    }

    public void Init()
    {        
        myCollectionManager.Init(false);
        myCollectionManager.HideCollection();
        confirmButton.gameObject.SetActive(false);
        chestOpenButtonParent.gameObject.SetActive(false);
        RefreshCollectionFromData();

        monstersUIInChestOpenning.Init(5);
        monstersUIInChestOpenning.RecycleAll();

        chestOpenedContainer.gameObject.SetActive(false);
        TryGetComponent(out audioSource);

        confirmButton.onClick.AddListener(OpenButtonBeforeBuyChestOrContinuing);
        chestOpenButton.onClick.AddListener(BuyChest);
        collectBtn.onClick.AddListener(Collect);

        regularChest.onClick.AddListener(() => ActiveConfirmationButton(MonsterChestType.Regular, regularChest));
        rareChest.onClick.AddListener(() => ActiveConfirmationButton(MonsterChestType.Rare, rareChest));
        legendaryChest.onClick.AddListener(() => ActiveConfirmationButton(MonsterChestType.Legendary, legendaryChest));
        saveForLaterButton.onClick.AddListener(() => ActiveConfirmationButton(MonsterChestType.NONE, saveForLaterButton));

        coinsAmtTxt.text = marketConfig.AvailableCoins.ToString();
        marketIntro = MarketIntro();
        noResources = NoResources();
        openChest = OpenChestAudios();
        closeNoResourcesBtn.onClick.AddListener(CloseNoResources);


    }

    private void Start()
    {
        StartCoroutine(marketIntro);
		//Init analytics
		GeneralGameAnalyticsManager.Instance.Init(DataIds.monsterMarket);
		initialStars = UserDataManager.CurrUser.Coins;
	}

    private void Update()
    {
        if(UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
        {
            if (Input.GetMouseButtonDown(0))
            {
                var selectedElement = EventSystem.current.currentSelectedGameObject;
                if (selectedElement == null) return;
                if (selectedElement.TryGetComponent(out Button btn))
                {
                    StopCoroutine(marketIntro);
                    audioSource.Stop();
                }

            }
        }
    }

    void OnCollectionsClosed()
    {
        if (UserDataManager.CurrUser.Coins == 0) SaveForLater();
        myCollectionManager.OnClosedCollections -= OnCollectionsClosed;
    }

    void ActiveConfirmationButton(MonsterChestType monsterChestType, Button chestBtn) 
    {
        currSelectedButton = chestBtn;
        if(monsterChestType == MonsterChestType.Regular) tutoHand.gameObject.SetActive(false);
        chestBtn.TryGetComponent<MonsterMarketButtonBehaviour>(out currButton);

        var btnToActive = userMonsterButtonBehaviours.Find(x => x.monsterMarketButton.monsterChestType == monsterChestType);
        if (btnToActive == null) return;
        if(btnToActive.monsterMarketButton.costChest > marketConfig.AvailableCoins)
        {
			for (int i = 0; i < userMonsterButtonBehaviours.Count; i++)
			{
				userMonsterButtonBehaviours[i].SetActiveState();
			}
			confirmButton.gameObject.SetActive(false);
            ShowNoResourcesCorroutine();
            return;
        }

		for (int i = 0; i < userMonsterButtonBehaviours.Count; i++)
		{
			userMonsterButtonBehaviours[i].SetInactiveState();
		}

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

        timeUntilFirstChestOpen = GeneralGameAnalyticsManager.Instance.analytics.timePlayed;

        chestOpenButtonParent.gameObject.SetActive(true);
        chestOpenImg.sprite = currButton.monsterMarketButton.chestCloseSprite;
        chestclosedImg.sprite = currButton.monsterMarketButton.chestCloseSprite;
        confirmButton.gameObject.SetActive(false);
    }

    void BuyChest()
    {
        opennedAtLeastOneChest = true;
        chestOpenButtonParent.gameObject.SetActive(false);
        switch (currButton.monsterMarketButton.monsterChestType)
        {
            case MonsterChestType.Regular:

                if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.Market_Instruction))
                {
                    continueBtn.interactable = true;
                }
				marketConfig.ConsumeCoins(marketConfig.RegularChestPrice);
                starsSpent += marketConfig.RegularChestPrice;
                chestTypeOpenned = 1;
                OpenChest(1, 0, 0);
                GeneralGameAnalyticsManager.RegisterWin();
                break;
            case MonsterChestType.Rare:
                marketConfig.ConsumeCoins(marketConfig.RareChestPrice);
				starsSpent += marketConfig.RareChestPrice;
				chestTypeOpenned = 2;
				OpenChest(1, 1, 0);
                break;
            case MonsterChestType.Legendary:
                marketConfig.ConsumeCoins(marketConfig.LegendaryChestPrice);
				starsSpent += marketConfig.LegendaryChestPrice;
				chestTypeOpenned = 3;
				OpenChest(1, 1, 1);
				GeneralGameAnalyticsManager.RegisterLose();
				break;
        }
        coinsAmtTxt.text = marketConfig.AvailableCoins.ToString();
        SetLockedImageInButtons();
    }

    void ShowNoResourcesCorroutine()
    {
        if (noResources != null) StopCoroutine(noResources);
        noResources = NoResources();
        StartCoroutine(noResources);
    }

    IEnumerator NoResources()
    {
        chestNoEnoughCoins.SetActive(true);
        audioSource.clip = noResourcesAudio;
        audioSource.Play();
        yield return new WaitForSeconds(noResourcesAudio.length);
        audioSource.clip = noStarsAudio;
        audioSource.Play();
        yield return new WaitForSeconds(noStarsAudio.length);
        noResources = null;
    }

    void CloseNoResources()
    {
        StopCoroutine(noResources);
        audioSource.Stop();
        chestNoEnoughCoins.SetActive(false);
    }

    private void SetLockedImageInButtons()
    {
        for (int i = 0; i < userMonsterButtonBehaviours.Count; i++)
        {
            userMonsterButtonBehaviours[i].SetLockedImage();
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
            audioSource.clip = clicContinue;
            audioSource.Play();
            yield return new WaitForSeconds(clicContinue.length);
            blockButtons.gameObject.SetActive(false);
        }
        StopCoroutine(openChest);
    }
    void OpenChest(int regularMonstersAmount, int rareMonstersAmount, int legendaryMonstersAmount)
    {
        chestOpenButtonParent.gameObject.SetActive(false);
        chestOpenedContainer.gameObject.SetActive(true);
        saveForLaterButton.gameObject.SetActive(true);
        chestOpenedContainerImg.sprite = currButton.monsterMarketButton.chestOpenSprite;
        currentMonstersFound.Clear();

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

#if UNITY_EDITOR

    [MenuItem("Hi Hat Games/Give100Coins")]
    public static void Give100Coins()
    {
        UserDataManager.CurrUser.Coins += 100;
    }
#endif

    public void SaveForLater()
    {
        finalStars = UserDataManager.CurrUser.Coins;
        totalTime = GeneralGameAnalyticsManager.Instance.analytics.timePlayed;
        if (!opennedAtLeastOneChest) timeUntilFirstChestOpen = 0;

        var dictionary = new Dictionary<string, object>();
        dictionary.Add(DataIds.timePlayed, totalTime);
        dictionary.Add(DataIds.chestChosen, chestTypeOpenned);
        dictionary.Add(DataIds.starsSpent, starsSpent);
        dictionary.Add(DataIds.starsBeforeSpend, initialStars);
        dictionary.Add(DataIds.unspentStars, finalStars);
        dictionary.Add(DataIds.selectionTime, timeUntilFirstChestOpen);

        UserDataManager.SaveUserAnayticsPerGame(DataIds.monsterMarket, dictionary);

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