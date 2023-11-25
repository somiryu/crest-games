using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_VoiceStarOrFlowerItemsSpawner : MonoBehaviour
{
    Pool<Transform> itemsPool;

    public void Init(int poolSize)
    {
        itemsPool.Init(poolSize);
    }

    public void spawnNewItem()
    {
        itemsPool.GetNewItem();
    }

}
