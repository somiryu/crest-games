using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHand_TurboRocket : MonoBehaviour
{
    Collider objToFollow;
    [SerializeField] float offset;
    public void Init(Collider _objToFollow)
    {
        objToFollow = _objToFollow;
        transform.position = objToFollow.transform.position + Vector3.right * offset * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = objToFollow.transform.position + Vector3.right * offset * Time.deltaTime;
    }
}
