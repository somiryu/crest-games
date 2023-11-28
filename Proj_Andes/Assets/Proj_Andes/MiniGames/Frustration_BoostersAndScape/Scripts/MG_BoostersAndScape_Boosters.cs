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
    [SerializeField] float lifetime;
    bool boosted;
    public void Init(Pool<MG_BoostersAndScape_Boosters> _pool)
    {
        pool = _pool;
        timer = 0;
        boosted = false;
        manager.activeBoosters.Add(this);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifetime) Recycle();
    }

    void Recycle()
    {
        manager.activeBoosters.Remove(this);
        pool.RecycleItem(this);
    }

    public bool Boosteable()
    {
        if(manager.onTrapMode) return false;
        if (transform.position.x <= manager.rocket.transform.position.x + manager.successRange || transform.position.x >= manager.rocket.transform.position.x - manager.successRange)
        {
            Recycle();
            Debug.Log("boosteable");
            return true;
        }
        else { Debug.Log("not boosteable"); return false; }
    }
}
