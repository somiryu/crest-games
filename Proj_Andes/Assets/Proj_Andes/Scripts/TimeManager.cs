using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public interface ITimeManagement
{
    public TimeManager timeManager => TimeManager.Instance;
}
public class TimeManager : MonoBehaviour
{
    private static string instancePrefabPath = "TimeManager";
    [SerializeField] SimpleGameSequenceItem testGeneralData;
    static TimeManager instance;
    public static TimeManager Instance => instance;
    List<ITimeManagement> users = new List<ITimeManagement>();
    public bool onTimeScalePaused;
    public static string createDate;
    public static string currSessionid;
    public static float timer;
    [HideInInspector] public SessionStateLeft gameState;
    [RuntimeInitializeOnLoadMethod]
    private static void RunOnStart()
    {
        if (instance == null)
        {
            var instancePrefab = Resources.Load<TimeManager>(instancePrefabPath);
            instance = GameObject.Instantiate(instancePrefab);
        }
    }

    public string RegisterTestDate()
    {
        return DateTime.Now.ToString("s");
	}

    private void Update()
    {
        CountTimeStart();
    }
    private void Awake()
    {
        if (instance != null && instance != this) DestroyImmediate(instance.gameObject);
        instance = this;
        gameState = SessionStateLeft.Abandoned;
        DontDestroyOnLoad(this);
    }
    public void CountTimeStart()
    {
        timer += Time.unscaledDeltaTime;
    }
    public void ResetSessionTimerAndSave()
    {
        GetQuitGameAnalytics();
        timer = 0;
    }

    public void SetNewStopTimeUser(ITimeManagement user)
    {
        users.Add(user);
        TestToStopTime();
    }
    public void RemoveNewStopTimeUser(ITimeManagement user)
    {
        users.Remove(user);
        TestToPlayTime();
    }
    void TestToPlayTime()
    {
        if (users.Count <= 0)
        {
            Time.timeScale = 1;
            onTimeScalePaused = false;
        }
    }
    void TestToStopTime()
    {
        if (users.Count > 0)
        {
            Time.timeScale = 0;
            onTimeScalePaused = true;
        }
    }
    public void ResetUsers()
    {
        users.Clear();
        TestToPlayTime();
    }
    public void GetQuitGameAnalytics()
    {
        if (UserDataManager.CurrTestID == "Default Test ID")
        {
            return;
        }
        var testAnalytics = testGeneralData.itemAnalytics = new Dictionary<string, object>();
        testAnalytics.Add(DataIds.institutionCode, UserDataManager.CurrInstitutionCode);
        testAnalytics.Add(DataIds.created_At, createDate);
        testAnalytics.Add(DataIds.ended_At, RegisterTestDate());
        testAnalytics.Add(DataIds.grade, UserDataManager.CurrUser.grade);
        testAnalytics.Add(DataIds.age, UserDataManager.CurrUser.age);
        testAnalytics.Add(DataIds.state, gameState.ToString());
        testAnalytics.Add(DataIds.time_Spent, timer);
        UserDataManager.SaveUserAnayticsPerGame(
            DataIds.test,
            testAnalytics,
            UserDataManager.CurrTestID,
            shouldUseTestID: false
            );
        //Resetting the test id since this only gets called when the user exits
        UserDataManager.CurrTestID = "Default Test ID";
        Debug.Log("saved test data " + DataIds.TestID + " " + UserDataManager.CurrTestID + " " + createDate + " " + gameState + " " + " realtime " + timer);
    }
}
