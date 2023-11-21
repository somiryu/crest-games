using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsSpawner : MonoBehaviour, iSpawnerUsers<StarsController> 
{
    public Spawner<StarsController> spawner;
    public bool initialActiveItems;
    public int initialSize;
    public bool InitialActiveItems()
    {
        return initialActiveItems;
    }

    public int InitialSize()
    {
        return initialSize;
    }

    public void OnCustomizeSpawn(StarsController newItem, Pool<StarsController> pool)
    {
        newItem.Init(pool);
    }

    // Start is called before the first frame update
    void Start()
    {
        spawner.Init(this);
    }

    // Update is called once per frame
    void Update()
    {
        spawner.Tick();
    }
}
