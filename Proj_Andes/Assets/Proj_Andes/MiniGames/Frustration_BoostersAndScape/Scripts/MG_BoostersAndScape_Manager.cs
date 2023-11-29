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
    }

    // Update is called once per frame
    void Update()
    {
        if (!onPlay) return;
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            for (int i = 0; i < activeBoosters.Count; i++)
            {
                if (activeBoosters[i].Boosteable())
                {
                    OnBoostered();
                    break;
                }
                else OnMissedBooster();
            }
        }
        if (alien.transform.position.x >= rocket.transform.position.x) OnGameEnd();
    }
    void OnGameStart()
    {
        successfulAttempts = 0;
        onPlay = true;
    }
    void OnGameEnd()
    {
        onPlay = false;
        Debug.Log("Game over!");
    }
    public void OnBoostered()
    {
        targetSpeed = gameConfig.turboSpeed;
        alienMov.OnBoosted();
        successfulAttempts++;
        Debug.Log("boosted");

    }
    public void OnMissedBooster()
    {
        alienMov.OnFailedToBoost();

        Debug.Log("not boosted");
    }
    public void OnBoosterEnded()
    {
        targetSpeed = gameConfig.regularSpeed;
    }
}
