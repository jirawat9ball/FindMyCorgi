using UnityEngine;
using TMPro;

public class FlashlightManager : MonoBehaviour
{

    public float timeLimit = 60f;
    public Sprite darkOverlayWithHole;
    public float overlaySize = 200f;

    private GameObject flashlightOverlayObj;
    private float currentTime;
    private bool isGameActive = false;
    private TextMeshProUGUI timerText;
    private SceneHandle currentScene;

    private void Start()
    {
        currentScene = FindObjectOfType<SceneHandle>();

        GameObject timerObj = GameObject.Find("Text_Timer");
        if (timerObj != null) timerText = timerObj.GetComponent<TextMeshProUGUI>();

        CreateFlashlightOverlay();

        currentTime = timeLimit;
        isGameActive = true;
    }

    private void CreateFlashlightOverlay()
    {
        flashlightOverlayObj = new GameObject("Auto_FlashlightDarkness");
        SpriteRenderer renderer = flashlightOverlayObj.AddComponent<SpriteRenderer>();

        renderer.sprite = darkOverlayWithHole;
        renderer.sortingOrder = 900;  

        
        renderer.drawMode = SpriteDrawMode.Sliced;

        
        renderer.size = new Vector2(overlaySize, overlaySize);

        //  บังคับล็อค Scale ไว้ที่ 1 เท่าเดิม
        flashlightOverlayObj.transform.localScale = Vector3.one;

        Cursor.visible = false; // ซ่อนเมาส์
    }

    private void Update()
    {
        if (!isGameActive || !Gamemanager.Instance.isStateGamePlay()) return;

        // 🌟 ให้แผ่นความมืดวิ่งตามเมาส์
        if (Camera.main != null && flashlightOverlayObj != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            flashlightOverlayObj.transform.position = mousePos;
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
        Cursor.visible = true;
        Debug.Log("🎉 หาเจอครบก่อนหมดเวลา! ชนะด่านไฟฉาย!");
    }

    public void GameOver()
    {
        currentTime = 0;
        isGameActive = false;
        if (timerText != null) timerText.text = "0";
        Cursor.visible = true;

        Debug.Log("💀 เวลาหมด! Game Over!");
    }

    private void OnDestroy()
    {
        if (flashlightOverlayObj != null) Destroy(flashlightOverlayObj);
        Cursor.visible = true;
    }
}