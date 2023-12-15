using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using TMPro;


public class MG_BoostersAndScape_Manager : MonoBehaviour, IEndOfGameManager
{
    static MG_BoostersAndScape_Manager instance;
    public static MG_BoostersAndScape_Manager Instance => instance;
    public MG_BoostersAndScape_GameConfig gameConfig;

    public Transform rocket;
    public Transform alien;
    MG_BoostersAndScape_AlienMovementController alienMov;
    [HideInInspector] public MG_BoostersAndScape_Boosters currentBooster;

    [HideInInspector] public bool onPlay;
    int successfulAttempts;
    int totalAttempts;
    [HideInInspector] public float catchBoosterRange;
    [HideInInspector] public bool onTrapMode;
    [HideInInspector] public float timer;
    bool onBoost;
    float targetTime;
    Vector3 startPos;

    public List<int> forcedFails = new List<int>();
    [SerializeField] MG_BoostersAndScape_Spawner spawner;
    [SerializeField] Transform endOfGameContainer;
    [SerializeField] TextMeshProUGUI finalScoreText;

    [SerializeField] EndOfGameManager eogManager;
    public EndOfGameManager EndOfGameManager => eogManager;
    private void Awake()
    {
        if(instance != null)
        {
            if (instance != this) DestroyImmediate(this);
        }
        instance = this;
        spawner.Init();
        Init();
    }
    void Init()
    {
        alien.TryGetComponent(out alienMov);
        alienMov.Init();
        startPos = rocket.transform.position;
        startPos.x = 0;
        rocket.transform.position = startPos;
        targetTime = gameConfig.boosterTriggerRate;
        endOfGameContainer.gameObject.SetActive(false);
        catchBoosterRange = 1.5f;
        OnGameStart();
    }

    void Update()
    {
        if (!onPlay) return;
        timer += Time.deltaTime;
        spawner.spawner.timer = timer;
        if(timer >= targetTime)
        {
            if (onBoost)
            {
                onBoost = false;
                targetTime = gameConfig.boosterTriggerRate;
            }
            timer = 0;
            MoveToNextPos(currentBooster);
        }
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (gameConfig.forceToFail) ForcedToFail();
            if (currentBooster.Boosteable())
            {
                OnBoostered(currentBooster);
            }
        }
        if (successfulAttempts == 10)
        {
            OnGameEnd();
            Debug.Log("You won!");
            rocket.transform.position = Vector3.right * 5 * Time.deltaTime;
        }
        if (alien.transform.position.x >= rocket.transform.position.x) OnGameEnd();
    }
    void OnGameStart()
    {
        endOfGameContainer.gameObject.SetActive(false);
        eogManager.OnGameStart();
        spawner.OnGameStart();
        successfulAttempts = 0;
        alienMov.OnGameStart();
        rocket.transform.position = startPos;
        totalAttempts = 0;
        for (int i = 0; i < gameConfig.forcedFails; i++)
        {
            forcedFails.Add(GenerateRandom());
        }
        onPlay = true;

    }
    void OnGameEnd()
    {
        finalScoreText.text = "Boosters captured " + successfulAttempts + "/10";
        endOfGameContainer.gameObject.SetActive(true);
        onPlay = false;
        spawner.OnGameEnd();
        eogManager.OnGameOver();
        Debug.Log("Game over!");
    }
    public void OnBoostered(MG_BoostersAndScape_Boosters booster)
    {
        onBoost = true;
        targetTime = 0.3f;
        alienMov.OnBoosted();
        booster.Boosted();
        spawner.spawner.nextSpawnTime = targetTime;
        successfulAttempts++;
        Debug.Log("boosted");
        timer = 0;
    }
    public void MoveToNextPos(MG_BoostersAndScape_Boosters booster)
    {
        totalAttempts++;
        alienMov.MoveToNextPoint();
    }
    public void ForcedToFail()
    {
        for (int i = 0; i < forcedFails.Count; i++)
        {
            if (totalAttempts == forcedFails[i])
            {
                forcedFails.Remove(forcedFails[i]);
                onTrapMode = true;
            }
            else onTrapMode = false;
        }
        if (successfulAttempts >= gameConfig.boostersPerRun - gameConfig.forcedFails) onTrapMode = true;
    }
    int GenerateRandom()
    {
        return Random.Range((gameConfig.boostersPerRun-gameConfig.forcedFails), totalAttempts);
    }
}
