using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.PackageManager;
using Unity.VisualScripting;

public class MovementController : MonoBehaviour
{
    public LevelConfig levelConfig;
    public bool onPlay;
    public Transform character;
    Vector3 firstPos;
    SphereCollider myCollider;
    [SerializeField] Collider[] colls;
    [SerializeField] Button playBtn;
    [SerializeField] Button turboBtn;
    [SerializeField] TextMeshProUGUI finishText;
    float timer;
    float turboTimer;
    int starsGatheredCount;
    int turboClicks;
    bool onTurbo;
    [SerializeField] float currentSpeed;
    [SerializeField] float forceOnClick;
    [SerializeField] float gravitalForce;
    public Camera cam;
    Ray hit;
    void Start()
    {
        Init();
    }
    public void Init()
    {
        TryGetComponent(out myCollider);
        playBtn.onClick.AddListener(Play);
        turboBtn.onClick.AddListener(OnTurbo);
        colls = new Collider[5];
        RideBegining();
    }
    void Update()
    {
        if (onPlay) ContinuousMovement(currentSpeed);
        if (Input.GetMouseButtonDown(1))
        {
            hit = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(hit, out RaycastHit hitInfo)) OnClickBoard(hitInfo.collider);
        }

        var collsAmt = Physics.OverlapSphereNonAlloc(myCollider.transform.position, myCollider.radius, colls);
        for (int i = 0; i < collsAmt; i++) CollisionManagement(colls[i]);

        if (onTurbo) TurboMovement();
    }
    void OnClickBoard(Collider collider)
    {
        if (collider.TryGetComponent(out ClickSpots spots))
        {

            if (spots.goingUp)
            {
                transform.position += Vector3.up * forceOnClick * Time.deltaTime;
            }
            else
            {
                transform.position -= Vector3.up * forceOnClick * Time.deltaTime;
            }
        }
    }
    void ContinuousMovement(float speed)
    {
        timer += Time.deltaTime;
        if (timer <= levelConfig.regularRideDuration)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
        }
        else
        {
            timer = 0;
            transform.position = firstPos;
            onPlay = false; 
        }
    }
    void CollisionManagement(Collider collider)
    {
        if (collider.CompareTag("Finish")) EndOfRide();
        else if (collider.TryGetComponent(out StarsController star))
        {
            star.OnCaptured();
            starsGatheredCount += 1;
            Debug.Log("caught star");
        }
    }
    void OnTurbo()
    {
        turboClicks += 1;
        onTurbo = true;
    }
    void Play()
    {
        if (!onPlay) 
        {
            onPlay = true;
            RideBegining();
        }
        else onPlay = false;
    }
    void TurboMovement()
    {
        turboTimer += Time.deltaTime;
        if (turboTimer >= levelConfig.turboLenght) { Deacceleration(); }
        else
        {
            currentSpeed += levelConfig.accelerationSpeed * Time.deltaTime;
            if (currentSpeed >= levelConfig.turboSpeed) currentSpeed = levelConfig.turboSpeed;
        }
    }
    void Deacceleration()
    {
        currentSpeed -= levelConfig.accelerationSpeed * Time.deltaTime;
        if (currentSpeed <= levelConfig.regularSpeed)
        {
            currentSpeed = levelConfig.regularSpeed;
            Debug.Log("done with turbo");
            onTurbo = false;
            turboTimer = 0;
        }
    }
    void RideBegining()
    {
        firstPos = transform.position;
        currentSpeed = levelConfig.regularSpeed;
        finishText.gameObject.SetActive(false);
    }
    void EndOfRide()
    {
        var ride = new GameRideData();
        ride.starsCollected = starsGatheredCount;
        ride.turboClicks = turboClicks;
        ride.totalRideDuration = timer;
        ride.totalStars = levelConfig.starsAmount;
        finishText.gameObject.SetActive(true); 
        onPlay = false;
    }
}

public class GameRideData
{
    public int turboClicks;
    public int starsCollected;
    public int totalStars;
    public float totalRideDuration;
}
