using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    PlayerController player => PlayerController.Instance;
    float visualOffset = -9;
    float targetPos;

    void Update()
    {
        Vector3 newPos = transform.localPosition;
        newPos.x = Mathf.MoveTowards(newPos.x, targetPos, player.levelConfig.accelerationSpeed * Time.deltaTime);
        newPos.y = 0;
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
