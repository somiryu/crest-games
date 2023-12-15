using UnityEngine;

[CreateAssetMenu(fileName = "SimpleGameSequenceItem", menuName = "GameSequencesList/SimpleGameSequenceItem")]
public class SimpleGameSequenceItem : GameSequence
{
    public override GameSequenceItem GetNextItem() => this;
	public override void OnReset() { }

	public override void OnSequenceOver() => GameSequencesList.Instance.GoToNextSequence();
}