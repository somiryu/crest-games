using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleGameSequenceItem", menuName = "GameSequencesList/SimpleGameSequenceItem")]
public class SimpleGameSequenceItem : GameSequence
{
    public override Dictionary<string, object> GetAnalytics() => itemAnalytics;
    public override int GetCurrItemIdx() => 0;

	public override GameSequenceItem GetItemByIdx(int i) => this;

	public override GameSequenceItem GetNextItem() => this;
	public override void OnReset() { }

	public override void OnSequenceOver() => GameSequencesList.Instance.GoToNextSequence();
}