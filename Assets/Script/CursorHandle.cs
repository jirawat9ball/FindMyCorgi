using UnityEngine;

public class CursorHandle : MonoBehaviour
{
    public static CursorHandle Instance { get; private set; }

    public enum CursorSize { Small, Medium, Large }

    [Header("Default Cursor Textures")]
    [Tooltip("ใส่รูป Cursor หลักของเกมลงใน Array นี้")]
    public Texture2D[] defaultCursorTextures;
    public Texture2D[] investigateCursorTextures;
    // 🌟 ตัวแปรซ่อนไว้: เก็บชุด Array ที่กำลังใช้งานอยู่ (สลับไปมาได้)
    private Texture2D[] activeCursorTextures;

    [Header("Index Setup")]
    public int smallIndex = 0;
    public int mediumIndex = 1;
    public int largeIndex = 2;

    [Header("Settings")]
    public CursorSize currentSize = CursorSize.Medium; // เปลี่ยนชื่อเป็น currentSize ให้ตรงความหมาย
    public Vector2 hotspot = Vector2.zero;

    [Header("Current State")]
    public int currentIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // เริ่มเกมให้ใช้ชุดภาพดั้งเดิม
        ResetToBaseCursor();
    }

    // ==========================================
    // 🌟 1. ฟังก์ชันเปลี่ยนไซส์ (จาก UI Slider / Option)
    // ==========================================
    public void ChangeSize(CursorSize newSize)
    {
        currentSize = newSize;
        UpdateCursorDisplay(); // สั่งอัปเดตภาพทันที
    }

    public void OnSliderValueChanged(float value)
    {
        int sizeIndex = Mathf.RoundToInt(value);
        if (sizeIndex >= 0 && sizeIndex <= 2)
        {
            ChangeSize((CursorSize)sizeIndex);
        }
    }

    // ==========================================
    // 🌟 2. ฟังก์ชันรับชุดภาพจากไอเทม (เมื่อเอาเมาส์ชี้)
    // ==========================================

    // รับชุด Array รูปภาพจากไอเทม
    public void SetItemCursor(Texture2D[] itemCursorArray)
    {
        if (itemCursorArray != null && itemCursorArray.Length > 0)
        {
            activeCursorTextures = itemCursorArray; // สลับไปใช้ชุดภาพของไอเทม
            UpdateCursorDisplay(); // อัปเดตภาพทันที
        }
    }

    // ฟังก์ชันสำหรับเรียกกลับไปใช้ชุดภาพหลักของเกม (เมื่อเมาส์ออกจากไอเทม)
    public void ResetToBaseCursor()
    {
        activeCursorTextures = defaultCursorTextures;
        Debug.Log("Reset to base cursor");
        UpdateCursorDisplay();
    }

    // ==========================================
    // 🛠️ ระบบกลางสำหรับจัดการภาพขึ้นหน้าจอ
    // ==========================================
    private void UpdateCursorDisplay()
    {
        if (activeCursorTextures == null || activeCursorTextures.Length == 0) return;

        // คำนวณหา Index จากไซส์ปัจจุบันของผู้เล่น
        int targetIndex = 0;
        switch (currentSize)
        {
            case CursorSize.Small: targetIndex = smallIndex; break;
            case CursorSize.Medium: targetIndex = mediumIndex; break;
            case CursorSize.Large: targetIndex = largeIndex; break;
        }

        // ดึงรูปมาแสดง (มีระบบดัก Error เผื่อชุด Array ที่ส่งมามีรูปไม่ครบ 3 ไซส์)
        if (targetIndex >= 0 && targetIndex < activeCursorTextures.Length)
        {
            currentIndex = targetIndex;
            Texture2D selectedTexture = activeCursorTextures[currentIndex];

            if (selectedTexture != null)
            {
                Cursor.SetCursor(selectedTexture, hotspot, CursorMode.ForceSoftware);
            }
        }
        else
        {
            // 🌟 เซฟตี้: ถ้าไอเทมดันส่ง Array มาแค่รูปเดียว ให้บังคับใช้รูปแรกเลย เกมจะได้ไม่แครช
            Cursor.SetCursor(activeCursorTextures[0], hotspot, CursorMode.ForceSoftware);
        }
    }
}