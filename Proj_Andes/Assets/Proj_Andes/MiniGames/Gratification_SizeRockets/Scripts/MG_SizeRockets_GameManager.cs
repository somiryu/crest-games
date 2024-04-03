using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_SizeRockets_GameManager : MonoBehaviour, IEndOfGameManager, ISizeRocketsManager
{
	private static MG_SizeRockets_GameManager instance;
	public static MG_SizeRockets_GameManager Instance => instance;
	public MG_SizeRockets_GameConfigs gameConfig;
    public MG_SizeRockets_GameConfigs gameConfigs { get => gameConfig; set { } }
	[Header("UI")]
	public Button smallRocketBtn;
	public Button mediumRocketBtn;
	public Button largeRocketBtn;
	public TMP_Text currCoinsLabel;
	public TMP_Text shipsLeftTxt;
	public Color selectedColor;
	[SerializeField] AudioClip succeededRound;
	AudioSource audioSource;

	[Header("After Action")]
	public GameObject afterActionPanel;
	public GameObject ingameObj;
	public GameObject ingameObjUI;
	public TMP_Text afterAction_CoinsCountTxt;

	[Header("Rockets")]
	public Pool<MG_SizeRockets_Rocket> smallRocketsPool;
	public Pool<MG_SizeRockets_Rocket> mediumRocketsPool;
	public Pool<MG_SizeRockets_Rocket> largeRocketsPool;

	[Header("Planets")]
	public Transform basePlanet;
	public Transform planetsParent;

	private List<MG_SizeRockets_Planet> planets = new List<MG_SizeRockets_Planet>();
	private List<MG_SizeRockets_Rocket> activeShips = new List<MG_SizeRockets_Rocket>();
	[SerializeField] MG_SizeRockets_Planet level1Planet;

	[SerializeField] public List<AudioClip> coinsLeftAudios = new List<AudioClip>();
	IEnumerator currAudio;
	[SerializeField] Transform actionBlocker;
	[SerializeField] CatchCoinsAudioInstruction catchCoinsAudio;

	private SizeRocketsRocketTypes selectedRocketType;
	private MG_SizeRockets_Planet currTargetPlanet;

	[SerializeField] EndOfGameManager eogManager;
	public EndOfGameManager EndOfGameManager => eogManager;
	public int shipsPerGame => gameConfigs.shipsPerGame;

    private int totalCoinsWon = 0;
	private int shipsLeft;

	private bool gameOverFlag;

	public SizeRocketAnalytics currAnalytics = new SizeRocketAnalytics();
	int roundCount;
	private void Awake()
	{
        ISizeRocketsManager.Instance = this;
		TryGetComponent(out audioSource);
		if (instance != null && instance != this) Destroy(instance);
		instance = this;
		
	}

	private void Start()
	{
		GeneralGameAnalyticsManager.Instance.Init(DataIds.sizeRocketsGame);
		Debug.LogWarning("Initializing size rockets");
		level1Planet.Init(20);
		planets.Add(level1Planet);
		ingameObj.SetActive(true);
		ingameObjUI.SetActive(true);

		eogManager.OnGameStart();
		selectedRocketType = SizeRocketsRocketTypes.NONE;
		smallRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.small));
		mediumRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.medium));
		largeRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.large));

		smallRocketsPool.Init(5);
		mediumRocketsPool.Init(5);
		largeRocketsPool.Init(5);
		currCoinsLabel.SetText(0.ToString());
		shipsLeft = gameConfig.shipsPerGame;
		shipsLeftTxt.SetText(shipsPerGame.ToString());
		roundCount = 0;
		currAudio = GetRoundCoinsAmount();
		StartCoroutine(currAudio);
	}

	IEnumerator GetRoundCoinsAmount()	{
		actionBlocker.gameObject.SetActive(true);
		audioSource.clip = coinsLeftAudios[roundCount];
		audioSource.Play();
		yield return new WaitForSeconds(coinsLeftAudios[roundCount].length);
        actionBlocker.gameObject.SetActive(false);
    }

	private void Update()
	{
		if(gameOverFlag) return;

		if (activeShips.Count > 0)
		{
			currTargetPlanet = level1Planet;
			smallRocketBtn.interactable = false;
			mediumRocketBtn.interactable = false;
			largeRocketBtn.interactable = false;
		}
		else
		{
			smallRocketBtn.interactable = true;
			mediumRocketBtn.interactable = true;
			largeRocketBtn.interactable = true;
		}

		if (activeShips.Count > 0) return;
        currTargetPlanet = level1Planet;

		if (selectedRocketType != SizeRocketsRocketTypes.NONE)
		{
			if (selectedRocketType == SizeRocketsRocketTypes.small) currAnalytics.smallShipsCount++;
			else if (selectedRocketType == SizeRocketsRocketTypes.medium) currAnalytics.mediumShipsCount++;
			else if (selectedRocketType == SizeRocketsRocketTypes.large) currAnalytics.bigShipsCount++;


			GenerateNewShip(selectedRocketType);
			currTargetPlanet.OnMatchHappend();
			currTargetPlanet = null;
			OnPressedRocketBtn(SizeRocketsRocketTypes.NONE);
		}
	}

	public void OnPressedRocketBtn(SizeRocketsRocketTypes types)
	{
		smallRocketBtn.image.color = Color.white;
		mediumRocketBtn.image.color = Color.white;
		largeRocketBtn.image.color = Color.white;
		switch (types) 
		{
			case SizeRocketsRocketTypes.small:
				smallRocketBtn.image.color = selectedColor;
				break;
			case SizeRocketsRocketTypes.medium:
				mediumRocketBtn.image.color = selectedColor;
				break;
			case SizeRocketsRocketTypes.large:
				largeRocketBtn.image.color = selectedColor;
				break;
		}
		selectedRocketType = types;
	}


	void GenerateNewShip(SizeRocketsRocketTypes types)
	{
		if (shipsLeft <= 0) return;
        roundCount++;

        var rocketsPool = GetRocketsPool(types);
		var currRocket = rocketsPool.GetNewItem();
		currRocket.transform.position = basePlanet.transform.position;
		currRocket.Init(rocketsPool, currTargetPlanet, basePlanet);
		activeShips.Add(currRocket);
		shipsLeft--;
		shipsLeftTxt.SetText(shipsLeft.ToString());
		shipsLeftTxt.GetComponent<Animator>().SetTrigger("Score");
		smallRocketBtn.interactable = false;
		mediumRocketBtn.interactable = false;
		largeRocketBtn.interactable = false;
	}



	public Pool<MG_SizeRockets_Rocket> GetRocketsPool(SizeRocketsRocketTypes type)
	{
		return type switch
		{
			SizeRocketsRocketTypes.small => smallRocketsPool,
			SizeRocketsRocketTypes.medium => mediumRocketsPool,
			SizeRocketsRocketTypes.large => largeRocketsPool,
			_ => null
		};
	}

	public void OnShipDeliveredCoins(MG_SizeRockets_Rocket rocket, int coinsAmount)
	{
		currAudio = GetRoundCoinsAmount();
        if(roundCount < coinsLeftAudios.Count) StartCoroutine(currAudio);

        if (rocket.rocketType == SizeRocketsRocketTypes.small) GeneralGameAnalyticsManager.RegisterLose();
		else GeneralGameAnalyticsManager.RegisterWin();


		activeShips.Remove(rocket);
		totalCoinsWon += coinsAmount;
		currCoinsLabel.SetText(totalCoinsWon.ToString());

		if (activeShips.Count == 0 && shipsLeft <= 0)
		{
			GameOver();
			return;
		}

		smallRocketBtn.interactable = true;
		mediumRocketBtn.interactable = true;
		largeRocketBtn.interactable = true;
	}


	void GameOver()
	{
        gameOverFlag = true;
		currAnalytics.stars = totalCoinsWon;
		currAnalytics.timePlayed = GeneralGameAnalyticsManager.Instance.analytics.timePlayed;
		currAnalytics.averageClick = GeneralGameAnalyticsManager.Instance.analytics.GetAverageClickTime();
		afterActionPanel.SetActive(true);
		ingameObj.SetActive(false);
		ingameObjUI.SetActive(false);
		afterAction_CoinsCountTxt.SetText(totalCoinsWon.ToString());
		gameConfigs.SaveCoins(totalCoinsWon);
        eogManager.OnGameOver();
	}

}

public class SizeRocketAnalytics
{
	public int bigShipsCount;
	public int mediumShipsCount;
	public int smallShipsCount;
	public int stars;
	public float averageClick;
	public float timePlayed;
}


public enum SizeRocketsRocketTypes
{
	small,
	medium,
	large,
	NONE
}

public interface ISizeRocketsManager
{
	public static ISizeRocketsManager Instance { get; set; }
    public MG_SizeRockets_GameConfigs gameConfigs { get; set; }
    public void OnShipDeliveredCoins(MG_SizeRockets_Rocket rocket, int coinsAmount);
}