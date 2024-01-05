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
        lastPlayerPos = player.transform.position;
    }
    void Update()
    {
        if (!player.onTurbo)
        {

            var currPlayerPos = player.transform.position;
            var dis = (currPlayerPos.y - lastPlayerPos.y) * movPercent;
            var targetY = transform.position.y + dis;
            var currPos = transform.position;
            transform.position = new Vector3(player.transform.position.x - visualOffset, targetY, currPos.z);

            lastPlayerPos = currPlayerPos;
        }
        else
        {
            var newPos = new Vector3(player.transform.position.x, transform.position.y, transform.position.z);
           
            transform.position = Vector3.MoveTowards(transform.position, newPos, player.playerCurrentSpeed * Time.deltaTime);
            
        }
    }
    public void OnEnterTurbo()
    {        
        targetPos.x = visualOffset;
        
    }
    public void OnExitTurbo()
    {
        targetPos.x = 0;
    }

    public void OnGameFinishedSequence()
    {
        targetPos = endSequenceCamPos;
    }
}
