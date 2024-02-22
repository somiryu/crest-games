using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeSceneManager : MonoBehaviour
{
    static NarrativeSceneManager instance;
    public static NarrativeSceneManager Instance => instance;

	[SerializeField] DialoguesDisplayerUI dialogueDisplayerUI;
	[SerializeField] DialogueSequenceData _startingSequence;

	public DialoguesDisplayerUI DialogueDisplayerUI => dialogueDisplayerUI;
	public int NarrativeIdx;

	RaycastHit[] collisions = new RaycastHit[30];
	[SerializeField] Transform loadingScreen;
	IEnumerator Delay;
	public bool isTheEntranceNarrative;

	private void Awake()
	{
		if(instance == null && instance != this) DestroyImmediate(instance);
		instance = this;
        Delay = StartDelay();
        if (!isTheEntranceNarrative) StartCoroutine(Delay);
    }
	IEnumerator StartDelay()
	{
		loadingScreen.gameObject.SetActive(true);
		yield return new WaitForSeconds(3);
        loadingScreen.gameObject.SetActive(false);
		StopCoroutine(Delay);
    }
    private void Start()
	{
        AudioManager.Instance.backgroundSoundType = BackgroundSoundType.Narrative;
		AudioManager.Instance.PlayMusic();
        if (_startingSequence != null) dialogueDisplayerUI.ShowDialogueSequence(_startingSequence);
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
