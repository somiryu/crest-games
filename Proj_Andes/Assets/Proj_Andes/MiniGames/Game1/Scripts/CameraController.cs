using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController player;
    float visualOffset = -9;
    float targetPos;
    void Start()
    {
        /*
         * method that creates stars bt some space v = d/t // d= v*t 
         * for bk and stars
         * replay  and only show stars collected

         * */
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = transform.localPosition;
        newPos.x = Mathf.MoveTowards(newPos.x, targetPos, PlayerController.Instance.levelConfig.accelerationSpeed * Time.deltaTime);
        transform.localPosition = newPos;
    }
    public void OnEnterTurbo()
    {
        targetPos = visualOffset;
    }
    public void OnExitTurbo()
    {
        targetPos = 0;
    }
}
