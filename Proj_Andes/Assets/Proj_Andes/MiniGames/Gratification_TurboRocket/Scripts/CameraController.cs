using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    PlayerController player => PlayerController.Instance;
    [SerializeField] float visualOffset = -4.7f;
    float targetPos;

    void Update()
    {
        Vector3 newPos = transform.localPosition;
        newPos.x = Mathf.MoveTowards(newPos.x, targetPos, player.levelConfig.accelerationSpeed * 4 * Time.deltaTime);
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
