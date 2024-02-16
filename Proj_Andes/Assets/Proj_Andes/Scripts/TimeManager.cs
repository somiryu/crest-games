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
    public void SetNewStopTimeUser(ITimeManagement user, AudioSource audioToPlay = null)
    {
        users.Add(user);
        Debug.Log("adding user to stop time");
        TestToStopTime(audioToPlay);
    }    
    public void RemoveNewStopTimeUser(ITimeManagement user, AudioSource audioToStop = null)
    {
        users.Remove(user);
        Debug.Log("removing user to play time");
        TestToPlayTime(audioToStop);
    }
    void TestToPlayTime(AudioSource audioToPlay = null)
    {
        if(users.Count <= 0)
        {
            Time.timeScale = 1;
            onTimeScalePaused = false;
            Debug.Log("onplay");
            if (audioToPlay != null) audioToPlay.Play();
        }
    }    
    void TestToStopTime(AudioSource audioToStop = null)
    {
        if(users.Count > 0)
        {
            Time.timeScale = 0;
            onTimeScalePaused = true;
            if (audioToStop != null) audioToStop.Pause();
        }
    }
    public void ResetUsers()
    {
        users.Clear();
        TestToPlayTime();
    }
}
