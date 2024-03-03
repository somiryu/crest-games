using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ScoreStarsController : MonoBehaviour
{
    [SerializeField] float starMovingTiming;
    Pool<ScoreStarsController> pool;
    float timer;
    Vector3 initialPosition;
    Vector3 finalPosition;

    public void Init(Vector3 initPos, Vector3 finalPos, Pool<ScoreStarsController> _pool)
    {
        pool = _pool;
        initialPosition = initPos;
        finalPosition = finalPos;
        timer = 0;
        Debug.Log("starInit " + initialPosition + " " + finalPos);
    }

    void Update()
    {
        timer += Time.deltaTime;
		var progress = timer / (float)starMovingTiming;
		transform.position = Vector3.Lerp(initialPosition, finalPosition, progress);
		if (timer >= starMovingTiming)
		{
			timer = 0;
			pool.RecycleItem(this);
		}
	}
}
