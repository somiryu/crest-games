using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gratification_TurboRocket_CameraController : MonoBehaviour
{
    Gratification_TurboRocket_PlayerController player => Gratification_TurboRocket_PlayerController.Instance;
    [SerializeField] float visualOffset = -4.7f;
    [SerializeField] Vector2 endSequenceCamPos = new Vector2(-4, 2);
    [SerializeField] float movPercent = 1f;
    [SerializeField] float moveSpeed = 1f;


    Vector2 targetPos;

    Vector3 lastPlayerPos;

    private void Start()
    {
        targetPos = Vector3.right * -visualOffset;
        lastPlayerPos = player.transform.position;
    }
    void Update()
    {        
        var currPlayerPos = player.transform.position;
        var dis = (currPlayerPos.y - lastPlayerPos.y) * movPercent;
        var targetY = transform.position.y + dis;
        var currPos = transform.position;
        currPos.x = Mathf.MoveTowards(currPos.x,targetPos.x + player.transform.position.x,player.levelConfig.accelerationSpeed*4* Time.deltaTime);   
        transform.position = new Vector3(currPos.x, targetY, currPos.z);
        lastPlayerPos = currPlayerPos;        
 
    }
    public void OnEnterTurbo()
    {        
        targetPos.x = 0;
        
    }
    public void OnExitTurbo()
    {
        targetPos.x = -visualOffset;
    }

    public void OnGameFinishedSequence()
    {
        targetPos.x = 0;
    }
}
