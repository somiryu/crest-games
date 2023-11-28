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
    public bool onPlay;

    public float targetSpeed;
    int successfulAttempts;
    public int totalAttempts;
    public float successRange;
    public bool onTrapMode;

    public List<MG_BoostersAndScape_Boosters> activeBoosters = new List<MG_BoostersAndScape_Boosters>();

    [SerializeField] Button playBtn;
    private void Awake()
    {
        if(instance != null)
        {
            if (instance != this) DestroyImmediate(this);
        }
        instance = this;
        Init();
    }

    void Init()
    {
        playBtn.onClick.AddListener(OnGameStart);
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
        //temporary
        if(totalAttempts > gameConfig.boostersPerRun) onPlay = false;
    }
    
    void OnGameStart()
    {
        successfulAttempts = 0;
        onPlay = true;
    }
    void OnGameEnd()
    {
        onPlay = false;
    }
    public void OnBoostered()
    {
        targetSpeed = gameConfig.turboSpeed;
        alien.TryGetComponent(out MG_BoostersAndScape_MovementController mov);
        mov.dir = -1;
        successfulAttempts++;
        Debug.Log("boosted");

    }
    public void OnMissedBooster()
    {
        targetSpeed = gameConfig.regularSpeed;
        Debug.Log("not boosted");
    }
}
