using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.PackageManager;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    static PlayerController instance;
    public static PlayerController Instance => instance;
    public LevelConfig levelConfig;
    public bool onPlay;
    public Transform character;
    Vector3 firstPos;
    float currentTargetSpeed;
    SphereCollider myCollider;
    Collider[] colls;
    float timer;
    float turboTimer;
    [HideInInspector] public int starsGatheredCount;
    public bool onTurbo = false;
    float currentSpeed;
    public Camera cam;
    [SerializeField] CameraController camCC;
    [SerializeField] Transform finalSpin;
    [SerializeField] BackgroundController bk;
    public GameStages gameStages;
    public UIController ui;
    public GameRideData data;
    Ray hit;
    private void Awake()
    {
        if (instance != null)
        {
            if(instance != this)
            {
                DestroyImmediate(this);
            }
        }
        instance = this;
    }
    void Start()
    {
        Init();
    }
    public void Init()
    {
        TryGetComponent(out myCollider);
        TryGetComponent(out ui);
        camCC = GetComponentInChildren<CameraController>();
        ui.StartUi();
    }
    void SetSpeedway()
    {
        Vector3 playerStartPos = bk.bkSize.localScale;
        playerStartPos.y = transform.position.y;
        playerStartPos.x -= (bk.bkSize.localScale.x * 1.5f);
        transform.position = playerStartPos;

        Vector3 finalSpinPos = bk.bkSize.localScale;
        finalSpinPos.y = finalSpin.transform.position.y;
        finalSpinPos.x -= bk.bkSize.localScale.x - bk.bkSize.localScale.x/2;
        finalSpin.transform.position = finalSpinPos;
    }
    void Update()
    {
        if (onPlay) ContinuousMovement();
        if (Input.GetMouseButton(1))
        {
            hit = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(hit, out RaycastHit hitInfo))
            {
                if(Input.GetMouseButtonDown(1)) OnClickBoard(hitInfo.collider, hitInfo);
            }
        }

        var collsAmt = Physics.OverlapSphereNonAlloc(myCollider.transform.position, myCollider.radius, colls);
        for (int i = 0; i < collsAmt; i++) CollisionManagement(colls[i]);

    }
    void OnClickBoard(Collider collider, RaycastHit hit)
    {
        if (collider.TryGetComponent(out ClickSpots spots))
        {
            Vector3 newHeight = transform.position;
            newHeight.y = hit.point.y;
            transform.position = newHeight;
        }
    }
    void ContinuousMovement()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, currentTargetSpeed, levelConfig.accelerationSpeed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer <= levelConfig.regularRideDuration)
        {
            transform.position += Vector3.right * currentSpeed * Time.deltaTime;
        }
        else
        {
            timer = 0;
            transform.position = firstPos;
            onPlay = false; 
        }
    }
    public void OnEnterTurboMode()
    {
        currentTargetSpeed = levelConfig.turboSpeed;
        camCC.OnEnterTurbo();
        onTurbo = true;
    }
    public void OnExitTurboMode()
    {
        currentTargetSpeed = levelConfig.regularSpeed;
        camCC.OnExitTurbo();
        onTurbo = false;
    }
    void CollisionManagement(Collider collider)
    {
        if (collider.CompareTag("Finish")) EndOfRide();
        else if (collider.TryGetComponent(out StarsController star))
        {
            if (onTurbo) return;
            star.OnCaptured();
        }
    }
    public void Play()
    {
        if (!onPlay) 
        {
            onPlay = true;
            RideBegining();
        }
        else onPlay = false;
    }
    public void RideBegining()
    {
        bk.Init();
        ui.StartUi();
        colls = new Collider[5];
        SetSpeedway();
        firstPos = transform.position;
        currentSpeed = levelConfig.regularSpeed;
        currentTargetSpeed = levelConfig.regularSpeed;
        gameStages = GameStages.Start;
    }
    void EndOfRide()
    {
        var ride = new GameRideData();
        ride.starsCollected = starsGatheredCount;
        ride.turboSelectedTime = turboTimer;
        ride.totalRideDuration = timer;
        ride.totalStars = levelConfig.starsAmount;
        data = ride;
        ui.EndOfGame();
        bk.EndOfGame();
        onPlay = false;
        gameStages = GameStages.End;
    }
}

public class GameRideData
{
    public float turboSelectedTime;
    public int starsCollected;
    public int totalStars;
    public float totalRideDuration;
}

public enum GameStages
{
    Start,
    End
}