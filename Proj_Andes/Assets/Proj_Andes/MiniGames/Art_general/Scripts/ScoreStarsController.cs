using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ScoreStarsController : MonoBehaviour
{
    [SerializeField] float starMovingTiming;
    Pool<ScoreStarsController> pool;
    bool hasGottenToFinalPos = false;
    float timer;
    Vector3 initialPosition;
    Vector3 finalPosition;
    Vector3 newPos;
    public void Init(Vector3 initPos, Vector3 finalPos, Pool<ScoreStarsController> _pool)
    {
        pool = _pool;
        initialPosition = initPos;
        finalPosition = finalPos;
        timer = 0;
        Debug.Log("starInit " + initialPosition + " " + finalPos);
    }

    void Move(Vector3 initPos, Vector3 finalPos)
    {
        var progress = timer / (float)starMovingTiming;
        var movx = Mathf.Lerp(initPos.x, finalPos.x, progress);
        var movy = Mathf.Lerp(initPos.y, finalPos.y, progress);
        newPos.x = movx;
        newPos.y = movy;
        transform.position = newPos;
        if (transform.position == finalPos || timer >= starMovingTiming)
        {
            hasGottenToFinalPos = true;
            timer = 0;
            pool.RecycleItem(this);
        }

    }

    void Update()
    {
        timer += Time.deltaTime;
        if(!hasGottenToFinalPos)
        {
            Move(initialPosition, finalPosition);
        }
    }
}
