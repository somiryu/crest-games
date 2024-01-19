using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHand_TurboRocket : MonoBehaviour
{
    Collider objToFollow;
    [SerializeField] float xOffset;
    [SerializeField] float yOffset;
    public void Init(Collider _objToFollow)
    {
        objToFollow = _objToFollow;
        Vector3 newPosOffset = objToFollow.transform.position;
        newPosOffset.x += xOffset;
        newPosOffset.y -= yOffset;
        transform.position = objToFollow.transform.position + newPosOffset * Time.deltaTime;
    }

}
