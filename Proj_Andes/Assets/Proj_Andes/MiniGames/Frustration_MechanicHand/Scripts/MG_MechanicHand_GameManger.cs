using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MG_MechanicHand_GameManger : MonoBehaviour, IEndOfGameManager
{
	private static MG_MechanicHand_GameManger instance;
	public static MG_MechanicHand_GameManger Instance => instance;

	public MG_MechanicHandGameConfigs gameConfigs;
	public MG_Frustration_MechanicHand_MechanicHandController player;

    public Pool<Transform> asteroidsPool = new Pool<Transform>();
	public List<BoxCollider> asteroidsAreaPerRound = new List<BoxCollider>();
	public TMP_Text playerLifesAmountTxt;
	public GameObject afterActionPanel;
	public TMP_Text afterAction_ResultsTxt;
	public GameObject afterAction_WinLabel;
	public GameObject afterAction_LoseLabel;

	private List<Transform> currRoundAsteroids = new List<Transform>();
    BoxCollider CurrAsteroidsSpawnArea => asteroidsAreaPerRound[currRound];

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;

    public int asteroidsPerRound => gameConfigs.asteroidsPerRound;
	public float sizeLoseOnRoundChange => gameConfigs.asteroidsSizeLoseAmountPerRound;
	int initialPlayerLifes => gameConfigs.playerLifes;

	int currRound = 0;
	float currAsteroidsSize = 1;
	int currPlayerLifes;
	public int totalCapturedAsteroids = 0;

	public static string ASTEROIDS_TAG = "Asteroid";

	public bool IsOnEndScreen => afterActionPanel.activeInHierarchy;

	public int NeededAsteroidsToWin => Mathf.FloorToInt((asteroidsPerRound * 3) * gameConfigs.percentageNeededToWin);

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		Init();
	}

	void Init()
    {
		totalCapturedAsteroids = 0;
		currPlayerLifes = initialPlayerLifes;
		playerLifesAmountTxt.SetText(currPlayerLifes.ToString());

		afterActionPanel.SetActive(false);
        currAsteroidsSize = 1;

        asteroidsPool.Init(10);
		player.Init();
		OnRoundStart();
    }

	void OnRoundStart()
	{
		currAsteroidsSize = 1 - (sizeLoseOnRoundChange * currRound);
		var center = CurrAsteroidsSpawnArea.transform.position;
		var maxPosition = center + CurrAsteroidsSpawnArea.size/2;
		var minPosition = center - CurrAsteroidsSpawnArea.size/2;

		currRoundAsteroids.Clear();

		for (int i = 0;i < asteroidsPerRound; i++)
		{
			var newPos = GetNewRandomPosition(0, minPosition, maxPosition);
			var curr = asteroidsPool.GetNewItem();
			curr.position = newPos;
			curr.localScale = Vector3.one * currAsteroidsSize;
			currRoundAsteroids.Add(curr);
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
		for (int i = 0; i < currRoundAsteroids.Count; i++)
		{
			var otherPos = currRoundAsteroids[i].position;
			var delta = otherPos - position;
			//Offset of 1 so that asteroids appear far from each other
			if(delta.magnitude <= currAsteroidsSize + 1)
			{
				return false;
			}
		}
		return true;
	}

	public void OnCapturedAsteroid(Transform asteroid)
	{
		totalCapturedAsteroids++;
		asteroidsPool.RecycleItem(asteroid);
		asteroid.SetParent(asteroidsPool.hierarchyParent);
		currRoundAsteroids.Remove(asteroid);
		if(currRoundAsteroids.Count == 0)
		{
			currRound++;
			if(currRound == 3) GameOver();
			else OnRoundStart();
		}
	}

	public void OnPlayerFailedHook()
	{
		currPlayerLifes--;
		playerLifesAmountTxt.SetText(currPlayerLifes.ToString());
		if(currPlayerLifes <= 0)
		{
			GameOver();
		}
	}

	void GameOver()
	{
		afterActionPanel.SetActive(true);
		//afterAction_ResultsTxt.SetText("Capturaste: " + totalCapturedAsteroids + " de " + asteroidsPerRound * 3);
		afterAction_ResultsTxt.SetText( totalCapturedAsteroids.ToString());
		var ratio = totalCapturedAsteroids / (asteroidsPerRound*3f);
		afterAction_WinLabel.SetActive(ratio >= gameConfigs.percentageNeededToWin);
		afterAction_LoseLabel.SetActive(ratio < gameConfigs.percentageNeededToWin);
		eogManager.OnGameOver();
	}


}
