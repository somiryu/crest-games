using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public interface iSpawnerUsers<T> where T : Component
{
    public Transform transform { get; }
    public void OnCustomizeSpawn(T newItem, Pool<T> pool);
    public int InitialSize();
    public bool InitialActiveItems();
}

[Serializable]
public class Spawner<T> where T : Component
{
    public Pool<T> pool = new Pool<T>();
    //public float initialSpawnDelay = 1;
    public float spawnRate = 3;
    public float randomFactor = 1;
    public Collider spawnArea;
    public bool randomSpawnArea;
    public float nextSpawnTime = 3;
    //public bool initialDelayDone = false;
    public float timer;
    iSpawnerUsers<T> owner;
    public void Init(iSpawnerUsers<T> user, float _spawnRate)
    {
        owner = user;
        pool.Init(user.InitialSize(), user.InitialActiveItems());
        spawnRate = _spawnRate;
    }
    public void Tick()
    {
        timer += Time.deltaTime;
        if(timer >= nextSpawnTime)
        {
            SpawnNewItem();
            timer = 0;
            nextSpawnTime = GetNextSpawnTime();
        }
    }
    public void SpawnNewItem()
    {
        var newItem = pool.GetNewItem();
        if (randomSpawnArea)
        {
            var center = owner.transform.position;
            var upLimit = center + spawnArea.bounds.extents;
            var downLimit = center - spawnArea.bounds.extents;
            var randomPos = center;
            randomPos.x += Random.Range(downLimit.x, upLimit.x);
            randomPos.y += Random.Range(downLimit.y, upLimit.y);
            newItem.transform.position = randomPos;
        }
        else newItem.transform.position = spawnArea.transform.position;
        newItem.gameObject.SetActive(true);
        owner.OnCustomizeSpawn(newItem, pool);
    }
    public float GetNextSpawnTime()
    {
        return spawnRate + Random.Range(-randomFactor, randomFactor);
    }
}
