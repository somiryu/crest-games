using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_BoostersAndScape_Spawner : MonoBehaviour, iSpawnerUsers<MG_BoostersAndScape_Boosters>
{
    MG_BoostersAndScape_Manager manager => MG_BoostersAndScape_Manager.Instance;
    [SerializeField] Spawner<MG_BoostersAndScape_Boosters> spawner;
    public bool InitialActiveItems()
    {
        return false;
    }

    public int InitialSize()
    {
        return manager.gameConfig.boostersPerRun;
    }

    public void OnCustomizeSpawn(MG_BoostersAndScape_Boosters newItem, Pool<MG_BoostersAndScape_Boosters> pool)
    {
        manager.totalAttempts++;
        newItem.Init(pool);
    }

    public void Init()
    {
        spawner.Init(this, manager.gameConfig.boosterTriggerRate);
    }

    // Update is called once per frame
    void Update()
    {
        if (!manager.onPlay) return;
        spawner.Tick();
    }
}
