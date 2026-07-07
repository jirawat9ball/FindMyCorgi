using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UIingame : MonoBehaviour
{
    public GameObject panalgame;
    public Image HideButton;
    public Sprite HideSprite;
    public Sprite Showprite;
    public SceneHandle sceneHandle;
    public TextMeshProUGUI NormalDogsTxt;
    public TextMeshProUGUI SpecialDogTxt;
    public TextMeshProUGUI announceText;
    public TextMeshProUGUI SnackTxt;
    

    [UnityEngine.Serialization.FormerlySerializedAs("panalPopUpManager")]
    public PanalPopUpManager panelPopUpManager;

    public void ToggleGameUI()
    {
        panalgame.SetActive(!panalgame.activeSelf);
        HideButton.sprite = panalgame.activeSelf ? HideSprite : Showprite;

    }
    private GameObject cachedFindPanel;
    private GameObject cachedNormalPanel;
    private GameObject cachedSpecialPanel;

    public void UpdateLostDog(int normalCount, int specialCount)
    {
        if (NormalDogsTxt != null) NormalDogsTxt.text = normalCount.ToString();
        if (SpecialDogTxt != null) SpecialDogTxt.text = specialCount.ToString();

        // 🌟 ค้นหา Panel ย่อย (Normal/Special) และ Panel แม่ (Find Panal) อัตโนมัติ
        if (cachedFindPanel == null && NormalDogsTxt != null)
        {
            // ให้ถือว่า Parent ของ Text คือกรอบย่อยของแต่ละชนิด
            cachedNormalPanel = NormalDogsTxt.transform.parent.gameObject;
            
            if (SpecialDogTxt != null)
                cachedSpecialPanel = SpecialDogTxt.transform.parent.gameObject;

            Transform parent = NormalDogsTxt.transform.parent;
            while (parent != null)
            {
                if (parent.name == "Find Panal" || parent.name == "Find Panel")
                {
                    cachedFindPanel = parent.gameObject;
                    break;
                }
                parent = parent.parent;
            }
        }

        // 🌟 ซ่อน-แสดง แยกระหว่าง Normal กับ Special
        if (cachedNormalPanel != null)
        {
            cachedNormalPanel.SetActive(normalCount > 0);
        }
        
        if (cachedSpecialPanel != null)
        {
            cachedSpecialPanel.SetActive(specialCount > 0);
        }

        // 🌟 ถ้าหาครบทั้งคู่ ไม่มีเหลือเลยทั้ง Normal และ Special ค่อยซ่อนกรอบใหญ่ทิ้ง
        if (cachedFindPanel != null)
        {
            cachedFindPanel.SetActive(normalCount > 0 || specialCount > 0);
        }
    }
    public void UpdateSnack(int snackCount)
    {
        if (SnackTxt != null) SnackTxt.text = snackCount.ToString();
    }
    public void ShowannounceText(string t) {
        announceText.text = t;
    }

}