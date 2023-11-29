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
    public void Init(Pool<MG_BoostersAndScape_Boosters> _pool)
    {
        pool = _pool;
        manager.activeBoosters.Add(this);
        timer = 0;
        lifetime = manager.gameConfig.boosterTriggerRate +2;
        boosted = false;
        initPos.x = transform.position.x;
        targetPos.x = manager.rocket.transform.position.x - initPos.x*2;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifetime) Recycle();
        if (!manager.onPlay) return;
        timer += Time.deltaTime;
        var currentProgress = Mathf.InverseLerp(0, manager.gameConfig.boosterTriggerRate*2, timer);
        var currentPos = Mathf.Lerp(initPos.x, targetPos.x, currentProgress);
        transform.position = Vector3.right * currentPos;
    }

    void Recycle()
    {
        manager.activeBoosters.Remove(this);
        pool.RecycleItem(this);
    }
    public void Boosted()
    {
        boosted = true;
    }
    public bool Boosteable()
    {
        if(manager.onTrapMode) return false;
        if (transform.position.x > manager.rocket.transform.position.x - manager.successRange && transform.position.x < manager.rocket.transform.position.x + manager.successRange)
        {
            Debug.Log("boosteable");
            return true;
        }
        else { Debug.Log("not boosteable"); return false; }
    }
}
