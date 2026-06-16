using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    public GameObject SplashUI;

    private void Update()
    {
        // ถ้ากดปุ่มใดๆ และยังไม่ได้ถูกกด
        if (Input.anyKeyDown)
        {
            LoadSceneManager.Instance.PlayLocalTransition(() => { });

            Invoke("HideSplashUI", 1f);
        }
    }

    private void HideSplashUI()
    {
        if (SplashUI != null)
        {
            SplashUI.SetActive(false);
        }
    }
}