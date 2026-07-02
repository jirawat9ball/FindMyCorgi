using UnityEngine;
using TMPro;

public class FlashlightManager : MonoBehaviour
{
    public Sprite darkOverlayWithHole;
    public float overlaySize = 200f;

    [Header("ระบบเวลา (ลาก CountdownTimer มาใส่)")]
    public CountdownTimer matchTimer;

    private GameObject flashlightOverlayObj;
    private bool isGameActive = false;
    private SceneHandle currentScene;

    private void Start()
    {
        currentScene = FindObjectOfType<SceneHandle>();
        CreateFlashlightOverlay();
        isGameActive = true;

        // 🌟 สั่งเริ่มเวลา และผูก Event เวลาหมดให้เรียก GameOver
        if (matchTimer != null)
        {
            matchTimer.onTimeOut.AddListener(GameOver);
            matchTimer.StartTimer();
        }
    }

    private void CreateFlashlightOverlay()
    {
        flashlightOverlayObj = new GameObject("Auto_FlashlightDarkness");
        SpriteRenderer renderer = flashlightOverlayObj.AddComponent<SpriteRenderer>();

        renderer.sprite = darkOverlayWithHole;
        renderer.sortingOrder = 900;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(overlaySize, overlaySize);
        flashlightOverlayObj.transform.localScale = Vector3.one;

        Cursor.visible = false;
    }

    private void Update()
    {
        if (!isGameActive || !Gamemanager.Instance.isStateGamePlay()) return;

        if (Camera.main != null && flashlightOverlayObj != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            flashlightOverlayObj.transform.position = mousePos;
        }

        if (currentScene != null && currentScene.lostDogs.Count == 0)
        {
            GameWin();
        }
    }

    public void GameWin()
    {
        if (!isGameActive) return;
        isGameActive = false;
        Cursor.visible = true;
        if (matchTimer != null) matchTimer.StopTimer();

        Debug.Log("🎉 หาเจอครบก่อนหมดเวลา! ชนะด่านไฟฉาย!");

        // 🌟 เรียก UI ชนะ
        if (currentScene != null && Gamemanager.Instance != null)
        {
            Gamemanager.Instance.ClearScene(currentScene.sceneObject.name);
            if (Gamemanager.Instance.dialogueUIManager != null)
            {
                Gamemanager.Instance.dialogueUIManager.OnShowDialog("dialog_found_all");
            }
        }
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;
        Cursor.visible = true;
        if (matchTimer != null) matchTimer.StopTimer();

        Debug.Log("💀 เวลาหมด! Game Over!");

        // 🌟 เรียก UI แพ้ (เปลี่ยนชื่อ Dialog ตามที่ตั้งไว้ในโปรเจกต์ได้เลย)
        if (Gamemanager.Instance != null && Gamemanager.Instance.dialogueUIManager != null)
        {
            Gamemanager.Instance.dialogueUIManager.OnShowDialog("dialog_lose");
        }
    }

    private void OnDestroy()
    {
        if (flashlightOverlayObj != null) Destroy(flashlightOverlayObj);
        Cursor.visible = true;
    }
}