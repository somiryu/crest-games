using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using tutorialSteps = tutorialSteps;

public class MG_MechanicHand_GameManger : MonoBehaviour, IEndOfGameManager, ITimeManagement
{
	private static MG_MechanicHand_GameManger instance;
	public static MG_MechanicHand_GameManger Instance => instance;

	public MG_MechanicHandGameConfigs gameConfigs;
	public MG_Frustration_MechanicHand_MechanicHandController player;

    public Pool<Transform> asteroidsPool = new Pool<Transform>();
	public List<BoxCollider> asteroidsAreaPerRound = new List<BoxCollider>();
	public TMP_Text playerLifesAmountTxt;
	public GameObject afterActionPanel;
	public GameObject inGameObjs;
	public TMP_Text afterAction_ResultsTxt;
	public TMP_Text constant_ResultsTxt;
	public GameObject afterAction_WinLabel;
	public GameObject afterAction_LoseLabel;
	public GameObject inGameUiContainer;
	public Button sendHookBtn;

	[SerializeField] AudioSource audioSource;
	[SerializeField] AudioClip notHookedAudio;
	[SerializeField] AudioClip introductionAudio;
	[SerializeField] AudioClip letsTryAudio;
	[SerializeField] AudioClip moveHookAudio;
	[SerializeField] AudioClip lettPlayAudio;
	[SerializeField] AudioClip noStarsAudio;
	[SerializeField] Transform blockingPanel;
	[SerializeField] Transform tutoHand1;
	[SerializeField] CatchCoinsAudioInstruction audioInstruction;

	private List<Transform> currRoundAsteroids = new List<Transform>();
    BoxCollider CurrAsteroidsSpawnArea => asteroidsAreaPerRound[currRound];

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;
	public Image trapImage;
    public int asteroidsPerRound => gameConfigs.asteroidsPerRound;
	public float sizeLoseOnRoundChange => gameConfigs.asteroidsSizeLoseAmountPerRound;
	int initialPlayerLifes => gameConfigs.playerLifes;

	public int currRound = 0;
	float currAsteroidsSize = 1;
	int currPlayerLifes;
	public int totalCapturedAsteroids = 0;

	public static string ASTEROIDS_TAG = "Asteroid";

	public bool IsOnEndScreen => afterActionPanel.activeInHierarchy;

	public int NeededAsteroidsToWin => Mathf.FloorToInt((asteroidsPerRound * 3) * gameConfigs.percentageNeededToWin);
	float totalTime;
	IEnumerator currentInstruction;
	bool tutoDone;

	public List<MechHandRoundAnalytics> allRoundAnalytics = new List<MechHandRoundAnalytics>();
	public MechHandRoundAnalytics currRoundAnalytics;

    //DATA ANALYTICS
    public float timePlayed;
    public int clickRepetitions;
    public int lostByCheat;
    public int clawThrows;

    private void Awake()
	{
		instance = this;
		if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_MechanicHand_1HoldClickAndMove)) GameUIController.Instance.onTuto = true;
		else tutoDone = true;
    }

	private void Start()
	{
		Init();
		GeneralGameAnalyticsManager.Instance.Init(DataIds.mechanicHandGame);
        var newRound = new MechHandRoundAnalytics();
        currRoundAnalytics = newRound;
    }
	IEnumerator Introduction()
	{
		player.canDrag = false;
		tutoHand1.gameObject.SetActive(false);
		blockingPanel.gameObject.SetActive(true);
		TimeManager.Instance.SetNewStopTimeUser(this);

        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_MechanicHand_1HoldClickAndMove))
		{
            audioSource.clip = introductionAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(introductionAudio.length+1);
            audioSource.clip = letsTryAudio;
            audioSource.Play();
            yield return new WaitForSecondsRealtime(letsTryAudio.length);
            TimeManager.Instance.RemoveNewStopTimeUser(this);
            tutoHand1.gameObject.SetActive(true);
            tutoDone = false;
            StartCoroutine( PlayAudiosGuide(moveHookAudio));
        }
		else
		{
            blockingPanel.gameObject.SetActive(false);
            TimeManager.Instance.RemoveNewStopTimeUser(this);
        }
		player.canDrag = true;
	}
	IEnumerator PlayAudiosGuide(AudioClip clip, AudioClip clip2 = null)
	{
		if(clip2 != null)
		{
            audioSource.clip = clip2;
            audioSource.Play();
            yield return new WaitForSeconds(clip2.length);
        }
		audioSource.clip = clip;
		audioSource.Play();
		yield return new WaitForSecondsRealtime(clip.length);
        blockingPanel.gameObject.SetActive(false);
    }
    void Init()
    {
		totalCapturedAsteroids = 0;
		totalTime = 0;
		currPlayerLifes = initialPlayerLifes;
		playerLifesAmountTxt.SetText(currPlayerLifes.ToString());

		afterActionPanel.SetActive(false);
		inGameObjs.SetActive(true);
        currAsteroidsSize = 1;

        asteroidsPool.Init(10);
		player.Init();
		OnRoundStart();
		sendHookBtn.onClick.AddListener(player.OnClickSendHook);
		currentInstruction = Introduction();
		StartCoroutine(Introduction());
    }
	void Update()
	{
		totalTime += Time.deltaTime;
		if (Input.GetMouseButtonDown(0)) clickRepetitions++;
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
        trapImage.gameObject.SetActive(false);
        eogManager.OnGameStart();
	}

	Vector3 GetNewRandomPosition(int trialIdx, Vector3 minPos, Vector3 maxPos)
	{
		trialIdx++;
		if (trialIdx > 20)
		{
			//Debug.LogError("No available random position found");
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
		GeneralGameAnalyticsManager.RegisterWin();
		totalCapturedAsteroids++;
		constant_ResultsTxt.SetText(totalCapturedAsteroids.ToString());

		asteroidsPool.RecycleItem(asteroid);
		asteroid.SetParent(asteroidsPool.hierarchyParent);
		currRoundAsteroids.Remove(asteroid);
		if(currRoundAsteroids.Count == 0)
		{
			currRound++;
			if(currRound == 3) GameOver();
			else OnRoundStart();
		}
        if (!tutoDone) StartCoroutine(PlayAudiosGuide(lettPlayAudio));
        tutoDone = true;
    }

    public void OnPlayerFailedHook()
	{
		GeneralGameAnalyticsManager.RegisterLose();
		GameUIController.Instance.StarLost();
		currPlayerLifes--;
		totalCapturedAsteroids -= 2;
		totalCapturedAsteroids = Mathf.Max(totalCapturedAsteroids, 0);
		constant_ResultsTxt.SetText(totalCapturedAsteroids.ToString());
		playerLifesAmountTxt.SetText(currPlayerLifes.ToString());
		if(currPlayerLifes <= 0)
		{
			GameOver();
		}
	}

	void GameOver()
	{
		inGameUiContainer.gameObject.SetActive(false);
		afterActionPanel.SetActive(true);
		inGameObjs.SetActive(false);
		StartCoroutine(PlayAudiosGuide(noStarsAudio));
		//afterAction_ResultsTxt.SetText("Capturaste: " + totalCapturedAsteroids + " de " + asteroidsPerRound * 3);
		afterAction_ResultsTxt.SetText( totalCapturedAsteroids.ToString());
		var ratio = totalCapturedAsteroids / (asteroidsPerRound*3f);
		afterAction_WinLabel.SetActive(ratio >= gameConfigs.percentageNeededToWin);
		afterAction_LoseLabel.SetActive(ratio < gameConfigs.percentageNeededToWin);
		timePlayed = totalTime;
        gameConfigs.SaveCoins(totalCapturedAsteroids);
        eogManager.OnGameOver();
	}


}

public class MechHandRoundAnalytics
{
	public int roundCount;
	public int thrown;
	public float presition;
}