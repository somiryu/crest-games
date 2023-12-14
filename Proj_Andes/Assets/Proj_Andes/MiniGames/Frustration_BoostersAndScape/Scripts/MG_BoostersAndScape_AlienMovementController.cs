using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MG_BoostersAndScape_AlienMovementController : MonoBehaviour
{
    MG_BoostersAndScape_Manager manager => MG_BoostersAndScape_Manager.Instance;
    [SerializeField] float currentSpeed;
    float roadStart;
    float initPos;
    float targetPos;
    float unitOfProgress;
    public float timer => manager.timer;
    public int progressGuide;
    float targetTime;
    public void Init()
    {
        initPos = transform.position.x;
        roadStart = initPos;
        progressGuide = 0;
        targetTime = manager.gameConfig.boosterTriggerRate;
        var dis = manager.rocket.transform.position.x - initPos;
        unitOfProgress = Mathf.Abs(dis) / manager.gameConfig.forcedFails;
    }
    void Update()
    {
        if (!manager.onPlay) return;
        var currentProgress = Mathf.InverseLerp(0, targetTime, timer);
        var currentPos = Mathf.Lerp(initPos, roadStart + unitOfProgress * progressGuide, currentProgress);
        transform.position = Vector3.right * currentPos;
    }

    public void OnBoosted()
    {
        progressGuide--;
        targetTime = 0.3f;
        if (progressGuide <= 0)
        {
            progressGuide = 0;
        }
        initPos = transform.position.x;

    }
    public void MoveToNextPoint()
    {
        progressGuide++;
        targetTime = manager.gameConfig.boosterTriggerRate;
        initPos = transform.position.x;
    }

    public void OnGameStart()
    {
        Vector3 startPos;
        startPos = transform.position;
        startPos.x = roadStart;
        transform.position = startPos;
        initPos = transform.position.x;
        progressGuide = 1;
        targetTime = manager.gameConfig.boosterTriggerRate;
        var dis = manager.rocket.transform.position.x - initPos;
        unitOfProgress = Mathf.Abs(dis) / manager.gameConfig.forcedFails;
        targetPos = initPos + unitOfProgress;
    }
}
