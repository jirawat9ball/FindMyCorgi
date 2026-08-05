using UnityEngine;
using UnityEngine.UI;

public class HowToPlayUI : MonoBehaviour
{
    [Header("How To Play Pages")]
    [Tooltip("ใส่หน้า How To Play ทั้ง 5 หน้าลงในนี้")]
    public GameObject[] pages;

    [Header("Navigation Buttons")]
    public Button nextButton;
    public Button prevButton;
    public Button closeButton;

    private int currentPageIndex = 0;

    void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        
        if (prevButton != null)
            prevButton.onClick.AddListener(PrevPage);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    public void ClosePanel()
    {
        // เรียกใช้ฟังก์ชันปิดหน้าต่างของระบบเกม
        if (Gamemanager.Instance != null && Gamemanager.Instance.uiIngame != null)
        {
            Gamemanager.Instance.uiIngame.panelPopUpManager.ClosePopUp();
        }
    }

    void OnEnable()
    {
        // เริ่มที่หน้าแรกเสมอเมื่อเปิดหน้าต่างนี้ขึ้นมา
        currentPageIndex = 0;
        UpdateUI();
    }

    public void NextPage()
    {
        if (currentPageIndex < pages.Length - 1)
        {
            currentPageIndex++;
            UpdateUI();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (pages == null || pages.Length == 0) return;

        // เปิด/ปิด หน้าตาม index ปัจจุบัน
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPageIndex);
            }
        }

        // ซ่อนปุ่มถัดไปถ้าอยู่หน้าสุดท้าย
        if (nextButton != null)
            nextButton.gameObject.SetActive(currentPageIndex < pages.Length - 1);
        
        // ซ่อนปุ่มย้อนกลับถ้าอยู่หน้าแรก
        if (prevButton != null)
            prevButton.gameObject.SetActive(currentPageIndex > 0);

        // ถ้าอยากให้ปุ่มปิดแสดงเฉพาะหน้าสุดท้าย ให้เอาคอมเมนต์บรรทัดล่างออกครับ
        // if (closeButton != null) closeButton.gameObject.SetActive(currentPageIndex == pages.Length - 1);
    }
}
