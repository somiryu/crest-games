using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gratification_TurboRocket_CameraController : MonoBehaviour
{
    Gratification_TurboRocket_PlayerController player => Gratification_TurboRocket_PlayerController.Instance;
    [SerializeField] float visualOffset = -4.7f;
    [SerializeField] Vector2 endSequenceCamPos = new Vector2(-4, 2);
    Vector2 targetPos;

    void Update()
    {
        Vector3 newPos = transform.localPosition;
        newPos.x = Mathf.MoveTowards(newPos.x, targetPos.x, player.levelConfig.accelerationSpeed * 4 * Time.deltaTime);
        newPos.y = Mathf.MoveTowards(newPos.y, targetPos.y, player.levelConfig.accelerationSpeed * 4 * Time.deltaTime);
		transform.localPosition = newPos;
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
