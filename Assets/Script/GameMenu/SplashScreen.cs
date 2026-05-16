using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    public string sceneName = "Stage Select";
    private void Update()
    {
        if (Input.anyKeyDown) {
            LoadScene(sceneName);
        }
    }
    public void LoadScene(string sceneName) {
        LoadSceneManager.Instance.LoadeScene(sceneName);
    }
}
