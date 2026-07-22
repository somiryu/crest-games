using System;
using UnityEngine;

[Serializable]
public class DialogueResponse
{
	[TextArea(1, 20)]
	public string response;

	[TextArea(1, 20)]
	public string responseAlternative;

	public Sprite responseImage;

	public AudioClip responseAudio;

	public AudioClip responseAudioAlternative;

	public DialogueSequenceData dataAfterResponse;

	public NarrativeAnalyicsInfo analyticInfo;
}
