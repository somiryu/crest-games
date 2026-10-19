using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Sequence Data", menuName = "DialogueSystem/Sequence")]
public class DialogueSequenceData : ScriptableObject
{
	public DialogueData[] dialogues;

	public override string ToString()
	{
		if (dialogues != null && dialogues.Length > 0)
		{
			DialogueData firstLine = dialogues[0];
			return $"{firstLine.characterType}: {firstLine.text}";
		}
		return name;
	}
}
