using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_BoostersAndScape_MovementController : MonoBehaviour
{
    MG_BoostersAndScape_Manager manager => MG_BoostersAndScape_Manager.Instance;
    public bool toRight;
    public float dir;
    float currentSpeed;
    private void Start()
    {
        currentSpeed = manager.gameConfig.regularSpeed;
        dir = toRight ? 1 : -1;
    }
    void Update()   
    {
        if (!manager.onPlay) return;
        ContinuousMovement();
    }
    public void ContinuousMovement()
    {
        var movement = Mathf.MoveTowards(currentSpeed, manager.targetSpeed, manager.gameConfig.accelerationSpeed);
        transform.position += Vector3.right * (movement*dir) * Time.deltaTime;
    }
}
