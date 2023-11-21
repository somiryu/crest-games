using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsController : MonoBehaviour
{
    MeshRenderer rend;
    Pool<StarsController> pool;
    public void Init(Pool<StarsController> _pool)
    {
        pool = _pool;
        rend = GetComponentInChildren<MeshRenderer>();
        rend.material.color = Color.yellow;
    }

    public void OnCaptured()
    {
        pool.RecycleItem(this);
        gameObject.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
