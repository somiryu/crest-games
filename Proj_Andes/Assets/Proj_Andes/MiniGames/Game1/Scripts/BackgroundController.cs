using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    Pool<BackgroundController> pool;
    public float backgroundSpeed;
    public float lifeTime;
    float timer;
    public void Init(Pool<BackgroundController> _pool)
    {
        pool = _pool;
    }

    void ContinuosMovement()
    {
        transform.position = Vector3.right * backgroundSpeed * Time.deltaTime;
    }
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(timer >= lifeTime)
        {
            pool.RecycleItem(this);
        }
        ContinuosMovement();
    }
}
