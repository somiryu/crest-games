using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_BoostersAndScape_AlienMovementController : MonoBehaviour
{
    MG_BoostersAndScape_Manager manager => MG_BoostersAndScape_Manager.Instance;
    [SerializeField] float currentSpeed;
    Vector3 roadStart;
    Vector3 initPos;
    Vector3 targetPos;
    float unitOfProgress;
    float timer;
    int progressGuide;
    //Pending to improve target time
    float targetTime;
    private void Start()
    {
        initPos = transform.position;
        roadStart = initPos;
        progressGuide = 1;
        targetTime = manager.gameConfig.boosterTriggerRate;
        var dis = manager.rocket.transform.position - initPos;
        unitOfProgress = dis.magnitude / manager.gameConfig.forcedFails;
        targetPos.x = initPos.x + unitOfProgress;
    }
    void Update()
    {
        if (!manager.onPlay) return;
        timer += Time.deltaTime;
        var currentProgress = Mathf.InverseLerp(0, targetTime, timer);
        var currentPos = Mathf.Lerp(initPos.x, targetPos.x, currentProgress);
        transform.position = Vector3.right * currentPos;
    }
    
    public void OnBoosted()
    {
        progressGuide--;
        targetTime = manager.gameConfig.boosterTriggerRate / 2;
        if (progressGuide >= 0)
        {
            targetPos.x = roadStart.x;
        }
        initPos = transform.position;
        timer = 0;
    }
    public void OnFailedToBoost()
    {
        progressGuide++;
        targetTime = manager.gameConfig.boosterTriggerRate;
        initPos = transform.position;
        targetPos.x = roadStart.x + unitOfProgress * progressGuide;
        timer = 0;
    }

    public void OnGameStart()
    {
        transform.position = roadStart;
        initPos = transform.position;
        progressGuide = 1;
        targetTime = manager.gameConfig.boosterTriggerRate;
        var dis = manager.rocket.transform.position - initPos;
        unitOfProgress = dis.magnitude / manager.gameConfig.forcedFails;
        targetPos.x = initPos.x + unitOfProgress;
    }
}
