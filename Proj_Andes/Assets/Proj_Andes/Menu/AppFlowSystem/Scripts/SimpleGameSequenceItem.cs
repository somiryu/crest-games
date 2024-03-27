using System;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "SimpleGameSequenceItem", menuName = "GameSequencesList/SimpleGameSequenceItem")]
public class SimpleGameSequenceItem : ScriptableObject
{
    public SceneReference scene;

    [NonSerialized] public Dictionary<string, object> itemAnalytics = new Dictionary<string, object>();

    public virtual string GetSceneID() => string.Empty;
    public virtual string GameID { get; set; }
    public virtual Dictionary<string, object> GetAnalytics() => itemAnalytics;
    public virtual void ResetCurrentAnalytics()
    {
        if(itemAnalytics != null) itemAnalytics.Clear();
        else itemAnalytics = new Dictionary<string, object>();

	}
    public virtual int GetCurrItemIdx() => 0;

	public virtual SimpleGameSequenceItem GetItemByIdx(int i) => this;

    public virtual SimpleGameSequenceItem GetNextItem()
    {
        if (GameSequencesList.Instance.prevGame != this) return this;
        else return null;
    }
    public virtual void OnReset() { }

	public virtual void OnSequenceOver() => GameSequencesList.Instance.GoToNextItemInList();
    public void SaveCoins(int coinsAmt)
    {
        Debug.Log("Adding " + coinsAmt + " Coins");
        UserDataManager.CurrUser.Coins += coinsAmt;
    }
    public virtual void SaveAnalytics() { }

    public virtual void SaveGeneralGameAnalytics()
    {
        if (GeneralGameAnalyticsManager.Instance == null) return;
        var analytics = GeneralGameAnalyticsManager.Instance.GetAnalytics();
        if(analytics == null) return;
        var idToUse = string.IsNullOrEmpty(GameID) ? Guid.NewGuid().ToString() : GameID;
        Debug.Log("cuur generic data gamekey " + idToUse);
        UserDataManager.SaveUserAnayticsPerGame(DataIds.generalGamesCollID, analytics, idToUse, shouldUseGameId: false);
    }

}