using UnityEngine;
using TMPro;

public class BossSonarManager : MonoBehaviour
{
    public float timeLimit = 60f;

    public Sprite darkOverlayWithHole;
    public float overlayScale = 20f; // ขนาดของแผ่นดำ 
    public float rotationSpeed = 25f; // ความเร็วในการหมุน

    private GameObject radarOverlayObj;
    private float currentTime;
    private bool isGameActive = false;
    private TextMeshProUGUI timerText;
    private SceneHandle currentScene;

    private void Start()
    {
        currentScene = FindObjectOfType<SceneHandle>();

        GameObject timerObj = GameObject.Find("Text_Timer");
        if (timerObj != null) timerText = timerObj.GetComponent<TextMeshProUGUI>();

        // เสกแผ่นสีดำเจาะรูขึ้นมา
        CreateRadarOverlay();

        currentTime = timeLimit;
        isGameActive = true;
    }

    private void CreateRadarOverlay()
    {
        radarOverlayObj = new GameObject("Auto_RadarDarkness");
        SpriteRenderer renderer = radarOverlayObj.AddComponent<SpriteRenderer>();
        renderer.sprite = darkOverlayWithHole;
        renderer.sortingOrder = 900;

        radarOverlayObj.transform.localScale = new Vector3(overlayScale, overlayScale, 1f);
    }

    private void Update()
    {
        if (!isGameActive || !Gamemanager.Instance.isStateGamePlay()) return;

        // 🌟 ให้แผ่นดำวิ่งตามกล้องและหมุนรอบตัวเอง
        if (Camera.main != null && radarOverlayObj != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            camPos.z = 0f;
            radarOverlayObj.transform.position = camPos;

            // หมุนไปเรื่อยๆ
            radarOverlayObj.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }

        // ระบบเวลานับถอยหลัง
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();

            if (currentTime <= 0)
            {
                GameOver();
            }
        }

        // เช็คเงื่อนไขชนะ 
        if (currentScene != null && currentScene.lostDogs.Count == 0)
        {
            GameWin();
        }
    }

    public void GameWin()
    {
        isGameActive = false;
        Debug.Log("🎉 หาเจอครบก่อนหมดเวลา! ชนะด่านโซน่าร์!");
    }

    public void GameOver()
    {
        currentTime = 0;
        isGameActive = false;
        if (timerText != null) timerText.text = "0";
        Debug.Log("💀 เวลาหมด! Game Over!");
    }

    private void OnDestroy()
    {
        if (radarOverlayObj != null) Destroy(radarOverlayObj);
    }
}