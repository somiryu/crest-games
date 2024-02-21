using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private static string instancePrefabPath = "AudioManager";
    static AudioManager instance;
    public static AudioManager Instance => instance;
    public AudioSource currentBkMusic;
    public AudioSource currentSoundEffect;
    [SerializeField] AudioClip clickSound;
    [SerializeField] AudioClip appBackgroundMusic;
    [SerializeField] AudioClip gameBackgroundMusic;
    [SerializeField] AudioClip narrativeBackgroundMusic;
    public BackgroundSoundType backgroundSoundType;
    Button currBtn;
    AudioClip currBkAudio;
    [RuntimeInitializeOnLoadMethod]
    private static void RunOnStart()
    {
        if (instance == null)
        {
            var instancePrefab = Resources.Load<AudioManager>(instancePrefabPath);
            instance = GameObject.Instantiate(instancePrefab);
        }
    }
    private void Awake()
    {
        if (instance != null && instance != this) DestroyImmediate(instance.gameObject);
        instance = this;
        DontDestroyOnLoad(this);
        PlayMusic();
    }
    public void PlayMusic()
    {
        if (GameSequencesList.Instance.currItem is GameConfig || GameSequencesList.Instance.currItem is SimpleGameSequenceItemTutorial)
        {
            backgroundSoundType = BackgroundSoundType.Game;
            currBkAudio = gameBackgroundMusic;
        }
        else if (backgroundSoundType == BackgroundSoundType.Narrative) currBkAudio = narrativeBackgroundMusic;
        else
        {
            currBkAudio = appBackgroundMusic;
            backgroundSoundType = BackgroundSoundType.App;
        }
        if (currentBkMusic.clip != currBkAudio)
        {
			currentBkMusic.clip = currBkAudio;
			var audioIsActive = PlayerPrefs.GetInt(UserDataManager.CurrUser.id + " isTheSoundActive");
            if (audioIsActive == 1)
            {
                currentBkMusic.Play();
            }
        }
        if(backgroundSoundType == BackgroundSoundType.Narrative) backgroundSoundType = BackgroundSoundType.App;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && backgroundSoundType != BackgroundSoundType.Game)
        {
            var selectedElement = EventSystem.current.currentSelectedGameObject;
            if (selectedElement == null) return;
            if(selectedElement.TryGetComponent(out currBtn))
            {
                currentSoundEffect.clip = clickSound;
                currentSoundEffect.Play();

            }
        }
    }
}
public enum BackgroundSoundType
{
    App,
    Game,
    Narrative

}
