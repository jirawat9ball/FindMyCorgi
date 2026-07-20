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
    public GameObject NormalClearedImg; // 🌟 รูปภาพติ๊กถูกของ Normal
    public TextMeshProUGUI SpecialDogTxt;
    public GameObject SpecialClearedImg; // 🌟 รูปภาพติ๊กถูกของ Special
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

    public void UpdateLostDog(int normalCount, int specialCount, int normalTotal = 0, int specialTotal = 0)
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
            // โชว์กรอบก็ต่อเมื่อด่านนี้มี Normal ให้หา (ถ้าไม่มีเลย จะปิดไปเลย)
            cachedNormalPanel.SetActive(normalTotal > 0);
            
            if (normalTotal > 0 && normalCount == 0)
            {
                // ถ้าหาเจอหมดแล้ว ซ่อนเลข โชว์รูปติ๊กถูก
                if (NormalDogsTxt != null) NormalDogsTxt.gameObject.SetActive(false);
                if (NormalClearedImg != null) NormalClearedImg.SetActive(true);
            }
            else
            {
                // ถ้ายังมีให้หา โชว์เลข ซ่อนรูปติ๊กถูก
                if (NormalDogsTxt != null) NormalDogsTxt.gameObject.SetActive(true);
                if (NormalClearedImg != null) NormalClearedImg.SetActive(false);
            }
        }
        
        if (cachedSpecialPanel != null)
        {
            cachedSpecialPanel.SetActive(specialTotal > 0);
            
            if (specialTotal > 0 && specialCount == 0)
            {
                if (SpecialDogTxt != null) SpecialDogTxt.gameObject.SetActive(false);
                if (SpecialClearedImg != null) SpecialClearedImg.SetActive(true);
            }
            else
            {
                if (SpecialDogTxt != null) SpecialDogTxt.gameObject.SetActive(true);
                if (SpecialClearedImg != null) SpecialClearedImg.SetActive(false);
            }
        }

        // 🌟 ซ่อนกรอบใหญ่ทิ้ง ก็ต่อเมื่อไม่มีหมาให้หาเลยทั้ง 2 แบบในด่านนี้
        if (cachedFindPanel != null)
        {
            cachedFindPanel.SetActive(normalTotal > 0 || specialTotal > 0);
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