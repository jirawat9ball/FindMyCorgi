using UnityEngine;
using UnityEngine.UI;

public class ButtonLoadScene : MonoBehaviour
{
    public SceneObject sceneObject;
    Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            Gamemanager.Instance.sceneObject = sceneObject;
            Gamemanager.Instance.AddSceneGame(sceneObject.Zone);
            Debug.Log($"[ButtonLoadScene] °¥ªÿË¡‚À≈¥©“°: {sceneObject.SceneName} „π‚´π: {sceneObject.Zone}");
        });
    }
}
