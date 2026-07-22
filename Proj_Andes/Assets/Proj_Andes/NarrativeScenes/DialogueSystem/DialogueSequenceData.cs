using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Sequence Data", menuName = "DialogueSystem/Sequence")]
public class DialogueSequenceData : ScriptableObject
{
	public DialogueData[] dialogues;

	public override string ToString()
	{
		return null;
	}
}
