using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class MG_BoostersAndScape_Manager : MonoBehaviour
{
    static MG_BoostersAndScape_Manager instance;
    public static MG_BoostersAndScape_Manager Instance => instance;
    public MG_BoostersAndScape_GameConfig gameConfig;

    public Transform rocket;
    public Transform alien;
    MG_BoostersAndScape_AlienMovementController alienMov;
    public bool onPlay;

    public float targetSpeed;
    int successfulAttempts;
    public int totalAttempts;
    public float successRange;
    public bool onTrapMode;
    public MG_BoostersAndScape_Boosters currentBooster;
    public float timer;
    public bool onBoost;
    float targetTime;
    Vector3 startPos;

    public List<MG_BoostersAndScape_Boosters> activeBoosters = new List<MG_BoostersAndScape_Boosters>();
    [SerializeField] MG_BoostersAndScape_Spawner spawner;
    [SerializeField] Button playBtn;
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
        playBtn.onClick.AddListener(OnGameStart);
        alien.TryGetComponent(out alienMov);
        startPos = rocket.transform.position;
        startPos.x = 0;
        rocket.transform.position = startPos;
        targetTime = gameConfig.boosterTriggerRate;
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
        Debug.Log(timer + " " + targetTime);
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (currentBooster.Boosteable())
            {
                OnBoostered(currentBooster);
            }
        }
        ForcedToFail();
        if(successfulAttempts == 10)
        {
            OnGameEnd();
            Debug.Log("You won!");
            rocket.transform.position = Vector3.right * 5 * Time.deltaTime;
        }
        if (alien.transform.position.x >= rocket.transform.position.x) OnGameEnd();
        Debug.Log(spawner.spawner.timer);

    }
    void OnGameStart()
    {
        successfulAttempts = 0;
        alienMov.OnGameStart();
        rocket.transform.position = startPos;
        onPlay = true;
    }
    void OnGameEnd()
    {
        onPlay = false;
        spawner.OnGameEnd();
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
        Debug.LogError("boosted");
        timer = 0;
    }
    public void MoveToNextPos(MG_BoostersAndScape_Boosters booster)
    {
        alienMov.MoveToNextPoint();
        Debug.LogError("not boosted");
    }
    public void ForcedToFail()
    {
        if(successfulAttempts >= 8) onTrapMode = true;
    }
}
