using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_MechanicHand_Hook : MonoBehaviour
{

	MG_Frustration_MechanicHand_MechanicHandController controller;

	string asteroidsTag = MG_MechanicHand_GameManger.ASTEROIDS_TAG;

   public void Init(MG_Frustration_MechanicHand_MechanicHandController _controller)
	{
		controller = _controller;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (controller.hookedObj) return;
		if (other.tag != asteroidsTag) return;
		controller.hookedObj = other.transform;
		GameUIController.Instance.StarEarned(Camera.main.WorldToScreenPoint(transform.position));
		other.transform.SetParent(transform);
	}
}
