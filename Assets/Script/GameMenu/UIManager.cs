using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private Queue<System.Action> notificationQueue = new Queue<System.Action>();
    private bool isShowingNotification = false;

    // ==========================================
    // ⚙️ SINGLETON
    // ==========================================
    public static UIManager Instance { get; private set; }

    [Header("หน้าจอ UI หลัก")]
    [UnityEngine.Serialization.FormerlySerializedAs("PanalCamp")]
    public GameObject PanelCamp;
    public GameObject MapMenu;
    public GameObject GameMenu;

    [Header("ระบบย่อย UI")]
    public UIingame uiIngame;
    public UIBooks uiInventory;
    public DialogueUIManager dialogueUIManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (currentSceneName == "Stage Select") 
        {
            if (PanelCamp != null) PanelCamp.SetActive(true);
            if (GameMenu != null) GameMenu.SetActive(false);
        }
        else
        {
            // ถ้าเป็นซีนอื่นๆ (เช่น ด่านต่างๆ) ให้สั่งปิดแคมป์ทิ้งไว้เลย
            if (PanelCamp != null) PanelCamp.SetActive(false);
        } 
    }

    // ==========================================
    // 🖥️ การเปิด/ปิดหน้าจอ
    // ==========================================

    /// <summary>
    /// เปิดหน้าจอ Gameplay (เข้าด่าน) ปิดอย่างอื่นทั้งหมด
    /// </summary>
    public void ShowGamePlay()
    {
        MapMenu.SetActive(false);
        PanelCamp.SetActive(false);
        GameMenu.SetActive(true);
        if (uiIngame != null) uiIngame.panelPopUpManager.CloseAllPopUp();
    }

    /// <summary>
    /// กลับไปหน้าแคมป์
    /// </summary>
    public void ShowHome()
    {
        MapMenu.SetActive(false);
        GameMenu.SetActive(false);
        PanelCamp.SetActive(true);
        if (uiIngame != null) uiIngame.panelPopUpManager.CloseAllPopUp();
    }

    /// <summary>
    /// กลับไปหน้าเลือกด่าน (Map Menu)
    /// </summary>
    public void ShowMapMenu()
    {
        PanelCamp.SetActive(true);
        GameMenu.SetActive(false);
        MapMenu.SetActive(true);
        if (uiIngame != null) uiIngame.panelPopUpManager.CloseAllPopUp();
    }

    /// <summary>
    /// อัปเดตจำนวนหมาที่ยังไม่เจอ
    /// </summary>
    public void UpdateLostDog(int normalCount, int specialCount)
    {
        if (uiIngame != null)
        {
            uiIngame.UpdateLostDog(normalCount, specialCount);
        }
    }

    /// <summary>
    /// แสดง Popup ไอเทม
    /// </summary>
    public void ShowPopUpGotItem(KeyItem keyItem)
    {
        notificationQueue.Enqueue(() => {
            isShowingNotification = true;
            if (uiIngame != null)
            {
                uiIngame.panelPopUpManager.ShowPopUpGotItem(keyItem);
            }
        });
        ProcessQueue();
    }

    /// <summary>
    /// แสดงบทพูด
    /// </summary>
    public void ShowDialog(string dialogKey)
    {
        notificationQueue.Enqueue(() => {
            isShowingNotification = true;
            if (dialogueUIManager != null)
            {
                dialogueUIManager.OnShowDialog(dialogKey);
            }
        });
        ProcessQueue();
    }

    private void ProcessQueue()
    {
        if (!isShowingNotification && notificationQueue.Count > 0)
        {
            var action = notificationQueue.Dequeue();
            action?.Invoke();
        }
    }

    public void OnNotificationClosed()
    {
        isShowingNotification = false;
        ProcessQueue();
    }
}