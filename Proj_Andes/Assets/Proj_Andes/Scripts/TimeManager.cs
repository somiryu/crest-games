using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface ITimeManagement
{
    public TimeManager timeManager => TimeManager.Instance;
}
public class TimeManager : MonoBehaviour
{
    private static string instancePrefabPath = "TimeManager";
    static TimeManager instance;
    public static TimeManager Instance => instance;
    List<ITimeManagement> users = new List<ITimeManagement>();
    public bool onTimeScalePaused;
    [RuntimeInitializeOnLoadMethod]
    private static void RunOnStart()
    {
        if (instance == null)
        {
            var instancePrefab = Resources.Load<TimeManager>(instancePrefabPath);
            instance = GameObject.Instantiate(instancePrefab);
        }
    }
    private void Awake()
    {
        if(instance != null && instance != this) DestroyImmediate(instance.gameObject);
        instance = this;
        DontDestroyOnLoad(this);
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
        if(users.Count <= 0)
        {
            Time.timeScale = 1;
            onTimeScalePaused = false;
        }
    }    
    void TestToStopTime()
    {
        if(users.Count > 0)
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
}
