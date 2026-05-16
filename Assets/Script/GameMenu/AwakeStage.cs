using UnityEngine;

public class AwakeStage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGSoundMainMenu();

    }
}
