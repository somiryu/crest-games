using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class MG_BoostersAndScape_Boosters : MonoBehaviour
{
    MG_BoostersAndScape_Manager manager => MG_BoostersAndScape_Manager.Instance;
    Pool<MG_BoostersAndScape_Boosters> pool;
    float timer;
    float lifetime;
    Vector3 initPos;
    Vector3 targetPos;
    public bool boosted;

    [Header("GameParticles")]
    [SerializeField] ParticleSystem boostedParticles;
    public void Init(Pool<MG_BoostersAndScape_Boosters> _pool)
    {
        pool = _pool;
        timer = 0;
        boosted = false;
        initPos.x = transform.position.x;
        lifetime = manager.gameConfig.boosterTriggerRate * 2;
        targetPos.x = manager.rocket.transform.position.x - initPos.x*2;
        manager.currentBooster = this;
        manager.currAnalytic = new BoostersAndScapeAnalytics();
    }

    void Update()
    {
        if (!manager.onPlay) return;
        timer += Time.deltaTime;
        if(timer > lifetime) Recycle(); 
        var currentProgress = Mathf.InverseLerp(0, manager.gameConfig.boosterTriggerRate*2, timer);
        var currentPos = Mathf.Lerp(initPos.x, targetPos.x, currentProgress);
        transform.position = Vector3.right * currentPos;
    }

    void Recycle()
    {
        pool.RecycleItem(this);
    }
    public void Boosted()
    {
        manager.RegisterDistanceForAnalytic(transform.position);
        boosted = true;
        boostedParticles.Play();
        Recycle();
    }
    public void NotBoosted()
    {
        boosted = false;
    }
    public bool Boosteable()
    {
        if(manager.onTrapMode) return false;
        if (boosted) return false;
        if (transform.position.x > manager.rocket.transform.position.x - manager.catchBoosterRange 
            && transform.position.x < manager.rocket.transform.position.x + manager.catchBoosterRange)
            return true;
        else
        {
            return false;
        }
    }
}
