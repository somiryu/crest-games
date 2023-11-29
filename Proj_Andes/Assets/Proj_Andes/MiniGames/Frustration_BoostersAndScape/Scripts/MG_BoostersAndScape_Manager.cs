using System.Collections;
using System.Collections.Generic;
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
    float timer;
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
    }

    void Update()
    {
        if (!onPlay) return;
        timer += Time.deltaTime;
        if(timer >= gameConfig.boosterTriggerRate)
        {
            for (int i = 0; i < activeBoosters.Count; i++)
            {
                if (!activeBoosters[i].boosted) OnMissedBooster();
            }
            timer = 0;
        }
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            for (int i = 0; i < activeBoosters.Count; i++)
            {
                if (activeBoosters[i].Boosteable())
                {
                    OnBoostered(activeBoosters[i]);
                    break;
                }
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
        booster.boosted = true;
        alienMov.OnBoosted();
        successfulAttempts++;
        Debug.Log("boosted");

    }
    public void OnMissedBooster()
    {
        alienMov.OnFailedToBoost();
        Debug.Log("not boosted");
    }
    public void ForcedToFail()
    {
        if(successfulAttempts >= 8) onTrapMode = true;
    }
}
