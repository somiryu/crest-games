using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TutorialSteps = tutorialSteps;


public class SizeRocketsTutorial_Manager : MonoBehaviour, ISizeRocketsManager
{
    public MG_SizeRockets_GameConfigs gameConfig;
    public MG_SizeRockets_GameConfigs gameConfigs { get => gameConfig; set { } }

    [Header("UI")]
    public Button smallRocketBtn;
    public Button mediumRocketBtn;
    public Button largeRocketBtn;
    public TMP_Text currCoinsLabel;
    public TMP_Text shipsLeftTxt;

    [Header("Rockets")]
    public Pool<MG_SizeRockets_Rocket> smallRocketsPool;
    public Pool<MG_SizeRockets_Rocket> mediumRocketsPool;
    public Pool<MG_SizeRockets_Rocket> largeRocketsPool;

    [Header("Planets")]
    public Transform basePlanet;
    public MG_SizeRockets_Planet planetPrefab;
    public BoxCollider planetsSpawnArea;
    public Transform planetsParent;

    private List<MG_SizeRockets_Rocket> activeShips = new List<MG_SizeRockets_Rocket>();
    MG_SizeRockets_Rocket activeShip;
    private SizeRocketsRocketTypes selectedRocketType;
    private MG_SizeRockets_Planet currTargetPlanet;

    [SerializeField] MG_SizeRockets_Planet tutoPlanet;
    [SerializeField] Transform handSignPlanet;

    int tutoStepIdx;
    public SizeRocketTutoSteps currTutoStep => tutorialSteps[tutoStepIdx];
    public List<SizeRocketTutoSteps> tutorialSteps = new List<SizeRocketTutoSteps>();

    public int shipsPerGame => gameConfigs.shipsPerGame;
    bool moving;
    public MG_SizeRockets_Rocket currShip { get => activeShip; set => activeShip = value; }
    public bool shipIsMoving { get => moving; set { } }


    private int totalCoinsWon = 0;
    private int shipsLeft;

    [SerializeField] Transform actionBlocker;

    AudioSource audioSource;
    [SerializeField] AudioClip introAudio;
    [SerializeField] AudioClip selectShipAudio;
    [SerializeField] AudioClip selectWorldAudio;
    [SerializeField] AudioClip tryStartsAudio;
    [SerializeField] AudioClip letsPlayAudio;
    [SerializeField] AudioClip letsTryAudio;
    [SerializeField] AudioClip onRightAction;
    [SerializeField] AudioClip onPassedTuto;
    [SerializeField] AudioClip onDepart;

    private void Awake()
    {
        ISizeRocketsManager.Instance = this;
        Debug.Log("Asigning instance");
        TryGetComponent(out audioSource);
    }

	private void Start()
	{
		StartCoroutine(RunTutorialStartInstructions());
		planetPrefab.Init(8);
		actionBlocker.gameObject.SetActive(true);

		selectedRocketType = SizeRocketsRocketTypes.NONE;
        moving = false;
		smallRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.small));
		mediumRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.medium));
		largeRocketBtn.onClick.AddListener(() => OnPressedRocketBtn(SizeRocketsRocketTypes.large));

		smallRocketsPool.Init(5);
		mediumRocketsPool.Init(5);
		largeRocketsPool.Init(5);
		currCoinsLabel.SetText(0.ToString());
		shipsLeft = gameConfig.shipsAmtLavel1;
		shipsLeftTxt.SetText(shipsPerGame.ToString());
	}


    IEnumerator RunTutorialStartInstructions()
    {
        actionBlocker.gameObject.SetActive(true);
		audioSource.clip = introAudio;
		audioSource.Play();
		yield return new WaitForSeconds(introAudio.length);

		currTutoStep.handSign.gameObject.SetActive(true);
        tutoStepIdx = 0;
		currTargetPlanet = tutoPlanet;

		audioSource.clip = selectShipAudio;
		audioSource.Play();
		yield return new WaitForSeconds(selectShipAudio.length);
		actionBlocker.gameObject.SetActive(false);
	}

    IEnumerator TouchScreenToMoveInstruction()
    {
        actionBlocker.gameObject.SetActive(true);
        handSignPlanet.gameObject.SetActive(true);
        audioSource.clip = selectWorldAudio;
        audioSource.Play();
        yield return new WaitForSeconds(selectWorldAudio.length);
        yield return ShipDescription();
        actionBlocker.gameObject.SetActive(false);
    }
    IEnumerator ShipDescription()
    {
        audioSource.clip = currTutoStep.onShipTypeAudio;
        audioSource.Play();
        yield return new WaitForSeconds(currTutoStep.onShipTypeAudio.length);
        audioSource.clip = letsTryAudio;
        audioSource.Play();
        yield return new WaitForSeconds(letsTryAudio.length);
        actionBlocker.gameObject.SetActive(false);
    }
    IEnumerator ResultDelivered(AudioClip clip)
    {
		actionBlocker.gameObject.SetActive(true);
		audioSource.clip = clip;
        audioSource.Play();
        yield return new WaitForSeconds(clip.length);
        GetNextStep();
    }

    void GetNextStep()
    {
        if (tutoStepIdx + 1 < tutorialSteps.Count)
        {
            actionBlocker.gameObject.SetActive(true);
            tutoStepIdx++;
            audioSource.clip = onRightAction;
            audioSource.Play();
			selectedRocketType = SizeRocketsRocketTypes.NONE;
			currTargetPlanet = null;
			ActivateTutoUI();
        }
        else GameOver();
    }

    void ActivateTutoUI()
    {
        if(!handSignPlanet.gameObject.activeInHierarchy) actionBlocker.gameObject.SetActive(true);
		StartCoroutine(ShipDescription());
        if(currTutoStep.Type != SizeRocketsTutoSteps.SmallRocketStep) currTutoStep.handSign.gameObject.SetActive(true);
        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (currTutoStep == tutorialSteps[i]) currTutoStep.activeRocketType.interactable = true;
            else tutorialSteps[i].activeRocketType.interactable = false;
        }
    }

    private void Update()
    {
        if (currShip != null && currShip.shouldMove)
        {
            if(currTutoStep.Type == SizeRocketsTutoSteps.SmallRocketStep) handSignPlanet.gameObject.SetActive(false);
        }
        if (selectedRocketType != SizeRocketsRocketTypes.NONE)
        {
            currTargetPlanet = tutoPlanet;
            if (activeShips.Count < 1) GenerateNewShip(selectedRocketType);
        }
    }

    public void OnPressedRocketBtn(SizeRocketsRocketTypes types)
    {
        currTutoStep.handSign.gameObject.SetActive(false);
        if (selectedRocketType != SizeRocketsRocketTypes.NONE) return;
        selectedRocketType = types;
        if (currTutoStep.Type == SizeRocketsTutoSteps.SmallRocketStep)
        {
            currTargetPlanet = null;
        }
    }

    void GenerateNewShip(SizeRocketsRocketTypes types)
    {
        if (shipsLeft <= 0) return;
        audioSource.clip = onDepart;
        audioSource.Play();
        if (currTutoStep.Type == SizeRocketsTutoSteps.SmallRocketStep) StartCoroutine(TouchScreenToMoveInstruction());
        var rocketsPool = GetRocketsPool(types);
        var currRocket = rocketsPool.GetNewItem();
        currRocket.transform.position = basePlanet.transform.position;
        currRocket.Init(rocketsPool, currTargetPlanet, basePlanet);
        activeShips.Add(currRocket);
        activeShip = currRocket;
        Debug.Log("Just asigned curr ship: " + currShip);
        shipsLeft--;
        shipsLeftTxt.SetText(shipsLeft.ToString());
        shipsLeftTxt.GetComponent<Animator>().SetTrigger("Score");

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
        selectedRocketType = SizeRocketsRocketTypes.NONE;
        currTargetPlanet = null;
        StartCoroutine(ResultDelivered(currTutoStep.onResultAudio));
        activeShips.Remove(rocket);
        activeShip = null;
        totalCoinsWon += coinsAmount;
        currCoinsLabel.SetText(totalCoinsWon.ToString());
    }
    void GameOver()
    {
        Debug.Log("Calling game over");
        StartCoroutine(GoToNextScene());
        gameConfigs.coinsCollected = totalCoinsWon;
        tutoPlanet.gameObject.SetActive(false);
    }
    IEnumerator GoToNextScene()
    {
        audioSource.clip = tryStartsAudio;
        audioSource.Play();
        yield return new WaitForSeconds(tryStartsAudio.length);
        audioSource.clip = letsPlayAudio;
        audioSource.Play();
        yield return new WaitForSeconds(letsPlayAudio.length);
        UserDataManager.CurrUser.RegisterTutorialStepDone(TutorialSteps.SizeRocketsDone.ToString());
        yield return new WaitForSeconds(1);
        GameSequencesList.Instance.GoToNextSequence();
    }
}

public enum SizeRocketsTutoSteps
{
    BigRocketStep,
    MediumRocketStep,
    SmallRocketStep
}

[Serializable]
public class SizeRocketTutoSteps
{
    public SizeRocketsTutoSteps Type;
    public Transform handSign;
    public Button activeRocketType;
    public AudioClip onShipTypeAudio;
    public AudioClip onResultAudio;
}
