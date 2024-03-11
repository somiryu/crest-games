using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return null;
		GameSequencesList.Instance.ResetSequence();
		SceneManagement.GoToScene(GameSequencesList.Instance.GetGameSequence().GetNextItem().scene);
	}
}
