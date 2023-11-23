using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankgroundSpawner : MonoBehaviour, iSpawnerUsers<BackgroundController>
{
    public Spawner<BackgroundController> spawner;
    public Collider trigger;
    public bool initalItemsActive;
    public int initialSize;
    public bool InitialActiveItems()
    {
        return initalItemsActive;
    }
    public int InitialSize()
    {
        return initialSize;
    }

    public void OnCustomizeSpawn(BackgroundController newItem, Pool<BackgroundController> pool)
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
