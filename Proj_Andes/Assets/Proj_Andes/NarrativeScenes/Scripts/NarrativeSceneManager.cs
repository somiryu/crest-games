using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeSceneManager : MonoBehaviour
{
    static NarrativeSceneManager instance;
    public static NarrativeSceneManager Instance => instance;

	[SerializeField] DialoguesDisplayerUI dialogueDisplayerUI;

	public DialoguesDisplayerUI DialogueDisplayerUI => dialogueDisplayerUI;

	RaycastHit[] collisions = new RaycastHit[30];


	private void Awake()
	{
		if(instance == null && instance != this) DestroyImmediate(instance);
		instance = this;
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			var hitAmount = Physics.RaycastNonAlloc(ray, collisions);
			if(hitAmount > 0)
			{
				for(int i = 0; i < hitAmount; i++)
				{
					var curr = collisions[i];
					if(curr.transform.TryGetComponent<NextDialogOnClicked>(out var nextDialogOnClicked) )
					{
						nextDialogOnClicked.OnClicked();
					}
					else if(curr.transform.TryGetComponent<ChangeDialogSequenceOnClicked>(out var changeDialogSequenceOnClicked))
					{
						changeDialogSequenceOnClicked.OnClicked();
					}
				}
			}
		}
	}
}
