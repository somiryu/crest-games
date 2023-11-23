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
    [SerializeField] TextMeshProUGUI finishText;
    float timer;
    float turboTimer;
    int starsGatheredCount;
    public bool onTurbo;
    bool slowling;
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
        
        colls = new Collider[5];
        RideBegining();
    }
    void Update()
    {
        if (onPlay) ContinuousMovement(currentSpeed);
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

        if (onTurbo)
        {
            TurboMovement();
            if (Input.GetMouseButtonUp(1)) slowling = true;
            if(slowling) Deacceleration();
        }
    }
    void OnClickBoard(Collider collider, RaycastHit hit)
    {
        if (collider.TryGetComponent(out ClickSpots spots))
        {
            Vector3 newHeight = transform.position;
            newHeight.y = hit.point.y;
            transform.position = newHeight;
        }
        else if (collider.TryGetComponent(out TurboButton turbo))
        {
            onTurbo = true;
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
            star.OnCaptured(onTurbo);
            starsGatheredCount += 1;
            Debug.Log("caught star");
        }
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
        if (slowling) return;
        turboTimer += Time.deltaTime;
        currentSpeed += levelConfig.accelerationSpeed * Time.deltaTime;
        Vector3 turboHeight = transform.position;
        turboHeight.y = firstPos.y;
        transform.position = turboHeight;
        if (currentSpeed >= levelConfig.turboSpeed) currentSpeed = levelConfig.turboSpeed;
    }
    void Deacceleration()
    {
        currentSpeed -= levelConfig.accelerationSpeed * Time.deltaTime;
        if (currentSpeed <= levelConfig.regularSpeed)
        {
            currentSpeed = levelConfig.regularSpeed;
            onTurbo = false;
            slowling = false;
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
        ride.turboSelectedTime = turboTimer;
        ride.totalRideDuration = timer;
        ride.totalStars = levelConfig.starsAmount;
        finishText.gameObject.SetActive(true); 
        onPlay = false;
    }
}

public class GameRideData
{
    public float turboSelectedTime;
    public int starsCollected;
    public int totalStars;
    public float totalRideDuration;
}
