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

    private SizeRocketsRocketTypes selectedRocketType;
    private MG_SizeRockets_Planet currTargetPlanet;

    [SerializeField] MG_SizeRockets_Planet tutoPlanet;
    [SerializeField] Transform handSignPlanet;
    [SerializeField] Transform handSignShip;

    int tutoStepIdx;
    public SizeRocketTutoSteps currTutoStep => tutorialSteps[tutoStepIdx];
    public List<SizeRocketTutoSteps> tutorialSteps = new List<SizeRocketTutoSteps>();

    public int shipsPerGame => gameConfigs.shipsPerGame;

    private int totalCoinsWon = 0;
    private int shipsLeft;

    [SerializeField] Transform actionBlocker;

    AudioSource audioSource;
    [SerializeField] AudioClip introAudio;
    [SerializeField] AudioClip selectShipAudio;
    [SerializeField] AudioClip selectWorldAudio;
    [SerializeField] AudioClip tryStartsAudio;
    [SerializeField] AudioClip letsPlayAudio;
    [SerializeField] AudioClip onRightAction;
    [SerializeField] AudioClip onPassedTuto;
    [SerializeField] AudioClip onDepart;
    IEnumerator currAudio;
    private void Awake()
    {
        ISizeRocketsManager.Instance = this;

        TryGetComponent(out audioSource);

        currAudio = ActivateStepActions(introAudio, selectShipAudio);
        StartCoroutine(currAudio);
        planetPrefab.Init(8);
        actionBlocker.gameObject.SetActive(true);

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
    void InitTuto()
    {
        currTargetPlanet = tutoPlanet;
        tutoStepIdx = 0;
        handSignPlanet.gameObject.SetActive(false);
        ActivateTutoUI();
    }

    IEnumerator ActivateStepActions(AudioClip clip, AudioClip clip2 = null)
    {
        audioSource.clip = clip;
        audioSource.Play();
        handSignShip.gameObject.SetActive(false);
        handSignPlanet.gameObject.SetActive(false);

        yield return new WaitForSeconds(clip.length);
        if (clip2 != null)
        {
            handSignShip.gameObject.SetActive(true);
            audioSource.clip = clip2;
            audioSource.Play();
            yield return new WaitForSeconds(clip2.length);
        }
        actionBlocker.gameObject.SetActive(false);
    }
    IEnumerator GetPlanet()
    {
        actionBlocker.gameObject.SetActive(true);
        handSignPlanet.gameObject.SetActive(true);
        audioSource.clip = selectWorldAudio;
        audioSource.Play();
        yield return new WaitForSeconds(selectWorldAudio.length);
        actionBlocker.gameObject.SetActive(false);
        InitTuto();
    }
    IEnumerator ShipDescription()
    {
        audioSource.clip = currTutoStep.onShipTypeAudio;
        audioSource.Play();
        yield return new WaitForSeconds(currTutoStep.onShipTypeAudio.length);
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
        actionBlocker.gameObject.SetActive(true);
        StartCoroutine(ShipDescription());
        if (currTutoStep.Type != SizeRocketsTutoSteps.SmallRocketStep) handSignShip.gameObject.SetActive(false);

        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (currTutoStep == tutorialSteps[i]) currTutoStep.activeRocketType.interactable = true;
            else tutorialSteps[i].activeRocketType.interactable = false;
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
        var otherPos = tutoPlanet.transform.position;
        var delta = otherPos - position;
        //1,5 if just the planets size, Offset of 1 so that planets appear far from each other
        if (delta.magnitude <= 1f + 1)
        {
            return false;
        }
        return true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (currTutoStep.Type == SizeRocketsTutoSteps.SmallRocketStep) handSignPlanet.gameObject.SetActive(false);
        }
        if (selectedRocketType != SizeRocketsRocketTypes.NONE)
        {
            currTargetPlanet = tutoPlanet;
            if (activeShips.Count < 1) GenerateNewShip(selectedRocketType);
        }
        if (shipsLeft <= 0 && activeShips.Count == 0) GameOver();
    }


    MG_SizeRockets_Planet GetPlanetUnderMouse(Vector3 position)
    {
        var curr = tutoPlanet;
        var dist = curr.transform.position - position;
        if (dist.magnitude <= 1.5f)
        {
            return curr;
        }
        return null;
    }

    public void OnPressedRocketBtn(SizeRocketsRocketTypes types)
    {
        if (selectedRocketType != SizeRocketsRocketTypes.NONE) return;
        selectedRocketType = types;
        if (currTutoStep.Type == SizeRocketsTutoSteps.SmallRocketStep)
        {
            currTargetPlanet = null;
            handSignShip.gameObject.SetActive(false);
        }
    }

    void GenerateNewShip(SizeRocketsRocketTypes types)
    {
        if (shipsLeft <= 0) return;
        audioSource.clip = onDepart;
        audioSource.Play();
        if (currTutoStep.Type == SizeRocketsTutoSteps.SmallRocketStep) StartCoroutine(GetPlanet());
        else if (currTutoStep.Type == SizeRocketsTutoSteps.BigRocketStep) handSignShip.gameObject.SetActive(false);
        var rocketsPool = GetRocketsPool(types);
        var currRocket = rocketsPool.GetNewItem();
        currRocket.transform.position = basePlanet.transform.position;
        currRocket.Init(rocketsPool, currTargetPlanet, basePlanet);
        activeShips.Add(currRocket);
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
        totalCoinsWon += coinsAmount;
        currCoinsLabel.SetText(totalCoinsWon.ToString());
    }
    void GameOver()
    {
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
    public Button activeRocketType;
    public AudioClip onShipTypeAudio;
    public AudioClip onResultAudio;
}
