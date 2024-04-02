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
	public MG_SizeRockets_Planet planetPrefab;
	public BoxCollider planetsSpawnArea;
	public Transform planetsParent;

	private List<MG_SizeRockets_Planet> planets = new List<MG_SizeRockets_Planet>();
	private List<MG_SizeRockets_Rocket> activeShips = new List<MG_SizeRockets_Rocket>();
	[SerializeField] MG_SizeRockets_Planet level1Planet;
	[SerializeField] private List<MG_SizeRockets_Planet> closePlanets;
	[SerializeField] private List<MG_SizeRockets_Planet> middleDistancePlanets;
	[SerializeField] private List<MG_SizeRockets_Planet> farPlanets;

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

		level1Planet.Init(20);
        planets.Add(level1Planet);
		TryGetComponent(out audioSource);
        ingameObj.SetActive(true);
		ingameObjUI.SetActive(true);
		if (instance != null && instance != this) Destroy(instance);
		instance = this;
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
	}

	private void Start()
	{
		GeneralGameAnalyticsManager.Instance.Init(DataIds.sizeRocketsGame);
	}
	void InitLevel2()
	{
		audioSource.clip = succeededRound;
		audioSource.Play();
		level1Planet.gameObject.SetActive(false);
        shipsLeft = gameConfig.shipsPerGame;
        shipsLeftTxt.SetText(shipsLeft.ToString());
        GeneratePlanets();
    }
    public void GeneratePlanets()
	{
		planets.AddRange(closePlanets);
		planets.AddRange(middleDistancePlanets);
		planets.AddRange(farPlanets);

		for (int i = 0; i < closePlanets.Count; i++)
		{
			closePlanets[i].gameObject.SetActive(true);
            closePlanets[i].Init(gameConfigs.closePlanetCoins);
        }
		for (int i = 0; i < middleDistancePlanets.Count; i++)
		{
            middleDistancePlanets[i].gameObject.SetActive(true);
            middleDistancePlanets[i].Init(gameConfigs.middlePlanetCoins);
        }
		for (int i = 0; i < farPlanets.Count; i++)
		{
            farPlanets[i].gameObject.SetActive(true);
            farPlanets[i].Init(gameConfigs.FarPlanetCoins);
        }

	}

	Vector3 GetNewRandomPosition(int trialIdx, Vector3 minPos, Vector3 maxPos)
	{
		trialIdx++;
		if (trialIdx > 20)
		{
			Debug.LogError("No available random position found");
			return Vector3.zero;
		}
		var newTestPosition = Vector3.zero;
		newTestPosition.x = Random.Range(minPos.x, maxPos.x);
		newTestPosition.y = Random.Range(minPos.y, maxPos.y);
		if (IsPositionAvailable(newTestPosition)) return newTestPosition;
		return GetNewRandomPosition(trialIdx, minPos, maxPos);
	}

	bool IsPositionAvailable(Vector3 position)
	{
		for (int i = 0; i < planets.Count; i++)
		{
			var otherPos = planets[i].transform.position;
			var delta = otherPos - position;
			//1,5 if just the planets size, Offset of 1 so that planets appear far from each other
			if (delta.magnitude <= 1f + 1)
			{
				return false;
			}
		}
		return true;
	}

	private void Update()
	{
		if(gameOverFlag) return;

		if (activeShips.Count > 0)
		{
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
		if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mouseWorldPos.z = 0;
			var newPlanet = GetPlanetUnderMouse(mouseWorldPos);
			if(newPlanet != currTargetPlanet)
			{
				currTargetPlanet = newPlanet;
				for (int i = 0; i < planets.Count; i++)
				{
					var curr = planets[i];
					curr.SetSelected(curr == currTargetPlanet);
				}
			}
			currTargetPlanet = GetPlanetUnderMouse(mouseWorldPos);
		}

		if (currTargetPlanet != null && selectedRocketType != SizeRocketsRocketTypes.NONE)
		{

			if(currTargetPlanet.coinsAmount == gameConfigs.FarPlanetCoins) currAnalytics.farPlanets++;
			else if(currTargetPlanet.coinsAmount == gameConfigs.middlePlanetCoins) currAnalytics.midPlanets++;
			else if(currTargetPlanet.coinsAmount == gameConfigs.closePlanetCoins) currAnalytics.closePlanets++;

			if (selectedRocketType == SizeRocketsRocketTypes.small) currAnalytics.smallShipsCount++;
			else if (selectedRocketType == SizeRocketsRocketTypes.medium) currAnalytics.mediumShipsCount++;
			else if (selectedRocketType == SizeRocketsRocketTypes.large) currAnalytics.bigShipsCount++;


			GenerateNewShip(selectedRocketType);
			currTargetPlanet.OnMatchHappend();
			currTargetPlanet = null;
			OnPressedRocketBtn(SizeRocketsRocketTypes.NONE);
		}

		
		
		if (shipsLeft <= 0 && activeShips.Count == 0)
		{
			roundCount++;
			//if (roundCount > 0 && roundCount <= 1) InitLevel2();
			GameOver();
        }
	}


	MG_SizeRockets_Planet GetPlanetUnderMouse(Vector3 position)
	{
		for (int i = 0; i < planets.Count; i++)
		{
			var curr = planets[i];
			var dist = curr.transform.position - position;
			if (dist.magnitude > 1.5f) continue;
			if (curr.coinsAmount == 0) continue;

			var actualCurrentCoins = curr.coinsAmount;
			for (int j = 0; j < activeShips.Count; j++)
			{
				var currShip = activeShips[j];
				if(currShip.targetPlanet == planets[i])
				{
					actualCurrentCoins -= currShip.coinsCapacity;
				}
			}
			if (actualCurrentCoins <= 0) continue;
			return curr;

		}
		return null;
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
		if (rocket.rocketType == SizeRocketsRocketTypes.small) GeneralGameAnalyticsManager.RegisterLose();
		else GeneralGameAnalyticsManager.RegisterWin();

		activeShips.Remove(rocket);
		totalCoinsWon += coinsAmount;
		currCoinsLabel.SetText(totalCoinsWon.ToString());
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
	public int closePlanets;
	public int midPlanets;
	public int farPlanets;
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