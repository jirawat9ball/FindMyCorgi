using UnityEngine;

public class UIManager : MonoBehaviour
{
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
        if (uiIngame != null)
        {
            uiIngame.panelPopUpManager.ShowPopUpGotItem(keyItem);
        }
    }

    /// <summary>
    /// แสดงบทพูด
    /// </summary>
    public void ShowDialog(string dialogKey)
    {
        if (dialogueUIManager != null)
        {
            dialogueUIManager.OnShowDialog(dialogKey);
        }
    }
}
