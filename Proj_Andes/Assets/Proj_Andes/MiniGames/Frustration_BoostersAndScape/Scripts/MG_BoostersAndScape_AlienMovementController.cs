using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_BoostersAndScape_AlienMovementController : MonoBehaviour
{
    MG_BoostersAndScape_Manager manager => MG_BoostersAndScape_Manager.Instance;
    [SerializeField] float currentSpeed;
    Vector3 initPos;
    Vector3 targetPos;
    float unitOfProgress;
    float timer;
    int progressGuide;
    private void Start()
    {
        initPos = transform.position;
        unitOfProgress = manager.gameConfig.boostersPerRun / manager.gameConfig.forcedFails;
        progressGuide = manager.gameConfig.boostersPerRun;
        targetPos.x = initPos.x + unitOfProgress;
        Debug.Log(unitOfProgress + " " + progressGuide + " " + manager);
    }
    void Update()
    {
        if (!manager.onPlay) return;
        timer += Time.deltaTime;
        var currentProgress = Mathf.InverseLerp(0, manager.gameConfig.boosterTriggerRate, timer);
        var currentPos = Mathf.Lerp(initPos.x, targetPos.x, currentProgress);
        transform.position = Vector3.right * currentPos;
    }
    public void OnBoosted()
    {
        progressGuide--;
        targetPos.x = unitOfProgress * progressGuide;
        initPos = transform.position;
        timer = 0;
    }
    public void OnFailedToBoost()
    {
        progressGuide++;
        targetPos.x = unitOfProgress * progressGuide;
        initPos = transform.position;
        timer = 0;
    }
}
