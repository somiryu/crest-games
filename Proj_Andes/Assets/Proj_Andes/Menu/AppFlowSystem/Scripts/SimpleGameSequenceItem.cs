using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleGameSequenceItem", menuName = "GameSequencesList/SimpleGameSequenceItem")]
public class SimpleGameSequenceItem : ScriptableObject
{
    public SceneReference scene;

    public Dictionary<string, object> itemAnalytics;

    public virtual Dictionary<string, object> GetAnalytics() => itemAnalytics;
    public virtual void ResetCurrentAnalytics()
    {
        itemAnalytics.Clear();
    }
    public virtual int GetCurrItemIdx() => 0;

	public virtual SimpleGameSequenceItem GetItemByIdx(int i) => this;

    public virtual SimpleGameSequenceItem GetNextItem()
    {
        if (GameSequencesList.Instance.prevGame != this) return this;
        else return null;
    }
    public virtual void OnReset() { }

	public virtual void OnSequenceOver() => GameSequencesList.Instance.GoToNextSequence();
    public virtual void SaveAnalytics()
    {

    }
}