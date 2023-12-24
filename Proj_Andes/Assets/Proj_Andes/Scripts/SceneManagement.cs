using Tymski;
using UnityEngine.SceneManagement;

public static class SceneManagement
{
    public static SceneReference currentScene;
    public static SkinType currSkinType { get; set; }
    public static void GoToScene(SceneReference scene)
    {
        currentScene = scene;
        SceneManager.LoadScene(scene);
    }
}
