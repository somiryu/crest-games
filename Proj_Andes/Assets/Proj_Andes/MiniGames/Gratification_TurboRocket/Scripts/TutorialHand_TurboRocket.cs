using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHand_TurboRocket : MonoBehaviour
{
    Collider objToFollow;
    [SerializeField] float xOffset;
    [SerializeField] float yOffset;
    public void Init(Collider _objToFollow, Vector2 handOffset)
    {
        objToFollow = _objToFollow;
        transform.SetParent(objToFollow.transform);
        transform.localPosition = handOffset;
	}

}
