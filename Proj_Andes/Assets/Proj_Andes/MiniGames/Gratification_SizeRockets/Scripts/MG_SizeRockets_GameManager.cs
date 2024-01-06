using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_SizeRockets_GameManager : MonoBehaviour, IEndOfGameManager
{
	private static MG_SizeRockets_GameManager instance;
	public static MG_SizeRockets_GameManager Instance => instance;

	public MG_SizeRockets_GameConfigs gameConfigs;
	[Header("UI")]
	public Button smallRocketBtn;
	public Button mediumRocketBtn;
	public Button largeRocketBtn;
	public TMP_Text currCoinsLabel;
	public TMP_Text shipsLeftTxt;
	[Header("After Action")]
	public GameObject afterActionPanel;
	public GameObject ingameObj;
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

	private SizeRocketsRocketTypes selectedRocketType;
	private MG_SizeRockets_Planet currTargetPlanet;

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;

    public int minCoinsToGive => gameConfigs.minCoinsToGive;
	public int maxCoinsToGive => gameConfigs.maxCoinsToGive;
	public int planetsAmountToGenerate => gameConfigs.planetsAmountToGenerate;
	public int shipsPerGame => gameConfigs.shipsPerGame;

	private int totalCoinsWon = 0;
	private int shipsLeft;


	private void Awake()
	{
		ingameObj.SetActive(true);
		if (instance != null && instance != this) Destroy(instance);
		instance = this;
		GeneratePlanets();
		selectedRocketType = SizeRocketsRocketTypes.NONE;

		smallRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.small));
		mediumRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.medium));
		largeRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.large));

		smallRocketsPool.Init(5);
		mediumRocketsPool.Init(5);
		largeRocketsPool.Init(5);
		currCoinsLabel.SetText(0.ToString());
		shipsLeft = shipsPerGame;
		shipsLeftTxt.SetText(shipsPerGame.ToString());
	}

	public void GeneratePlanets()
	{
		for(int i = 0; i < planetsAmountToGenerate; i++)
		{
			var newPlanet = Instantiate(planetPrefab);
			newPlanet.transform.SetParent(planetsParent);
			planets.Add(newPlanet);
			var halfSize = planetsSpawnArea.size / 2;
			var minRange = transform.position - halfSize;
			var maxRange = transform.position + halfSize;
			newPlanet.transform.position = GetNewRandomPosition(0, minRange, maxRange);
		}

		var minDist = Mathf.Infinity;
		var maxDist = Mathf.NegativeInfinity;
		for (int i = 0; i < planets.Count; i++)
		{
			var currDist = planets[i].transform.position.x - basePlanet.position.x;
			maxDist = Mathf.Max(maxDist, currDist);
			minDist = Mathf.Min(minDist, currDist);
		}

		for (int i = 0; i < planets.Count; i++)
		{
			var currDist = planets[i].transform.position.x - basePlanet.position.x;
			var percentage = Mathf.InverseLerp(minDist, maxDist, currDist);

			var coinsToGive = Mathf.Lerp(minCoinsToGive, maxCoinsToGive, percentage);

			planets[i].Init(Mathf.RoundToInt(coinsToGive));
		}
		eogManager.OnGameStart();
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
		if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mouseWorldPos.z = 0;
			currTargetPlanet = GetPlanetUnderMouse(mouseWorldPos);
		}

		if (currTargetPlanet != null && selectedRocketType != SizeRocketsRocketTypes.NONE)
		{
			GenerateNewShip(selectedRocketType);
			currTargetPlanet = null;
		}
		if(shipsLeft <= 0 && activeShips.Count == 0) GameOver();
	}


	MG_SizeRockets_Planet GetPlanetUnderMouse(Vector3 position)
	{
		for(int i = 0;i < planets.Count;i++)
		{
			var curr = planets[i];
			var dist = curr.transform.position - position;
			if(dist.magnitude <= 1.5f)
			{
				return curr;
			}
		}
		return null;
	}

	public void OnPressedRocketBtn(SizeRocketsRocketTypes types) => selectedRocketType = types;

	void GenerateNewShip(SizeRocketsRocketTypes types)
	{
		if (shipsLeft <= 0) return;

		var rocketsPool = GetRocketsPool(types);
		var currRocket = rocketsPool.GetNewItem();
		currRocket.transform.position = basePlanet.transform.position;
		currRocket.Init(rocketsPool, currTargetPlanet, basePlanet);
		activeShips.Add(currRocket);
		shipsLeft--;
		shipsLeftTxt.SetText("Naves: " + shipsLeft.ToString());
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
		activeShips.Remove(rocket);
		totalCoinsWon += coinsAmount;
		currCoinsLabel.SetText(totalCoinsWon.ToString());
	}


	void GameOver()
	{
		afterActionPanel.SetActive(true);
		ingameObj.SetActive(false);
		afterAction_CoinsCountTxt.SetText(totalCoinsWon.ToString());
		gameConfigs.coinsCollected = totalCoinsWon;
        eogManager.OnGameOver();
	}

}


public enum SizeRocketsRocketTypes
{
	small,
	medium,
	large,
	NONE
}