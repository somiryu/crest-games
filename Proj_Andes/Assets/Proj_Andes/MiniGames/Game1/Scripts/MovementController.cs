using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.PackageManager;

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
    [SerializeField] float currentSpeed;
    [SerializeField] float forceOnClick;
    [SerializeField] float gravitalForce;
    public Camera cam;
    Ray hit;
    void Start()
    {
        TryGetComponent(out myCollider);
        firstPos = transform.position;
        playBtn.onClick.AddListener(Play);
        turboBtn.onClick.AddListener(OnTurbo);
        colls = new Collider[5];
        currentSpeed = levelConfig.regularSpeed;
        finishText.gameObject.SetActive(false);
    }

    // Update is called once per frame
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

        if (currentSpeed == levelConfig.turboSpeed)
        {
            turboTimer += Time.deltaTime;
            if(turboTimer >= levelConfig.turboLenght)
            {
                currentSpeed = levelConfig.regularSpeed;
                turboTimer = 0;
            }
        }
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
        if (collider.CompareTag("FinishLine")) { finishText.gameObject.SetActive(true); Debug.Log("FNINSHLINE"); }
        else if (collider.TryGetComponent(out StarsController star))
        {
            star.gameObject.SetActive(false);
            starsGatheredCount += 1;
            Debug.Log("caught star");
        }
    }
    void OnTurbo()
    {
        currentSpeed = levelConfig.turboSpeed;
    }
    void Play()
    {
        if (!onPlay) onPlay = true;
        else onPlay = false;
    }
}
