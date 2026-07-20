using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GotItem : MonoBehaviour
{
    public TextMeshProUGUI text; // 🌟 ใช้สำหรับคำว่า "You got" (ตัวเดิม)
    public TextMeshProUGUI textItemName; // 🌟 ใช้สำหรับ "ชื่อไอเทม" (ต้องลากมาใส่ใหม่)
    public Image Image;
    public KeyItem currentItem;

    public void SetUpItem(KeyItem key) {
        currentItem = key;
        if (key.name == "ItemDemo")
        {
            // ถ้าเป็นหน้าจอจบเดโม ปิดคำว่า "You got" ไปเลย แล้วโชว์แค่ชื่อ
            if (text != null) text.gameObject.SetActive(false);
            if (textItemName != null) textItemName.text = key.KeyName; 
        }
        else
        {
            // ถ้าเป็นไอเทมปกติ แสดงคำว่า "You got" แล้วแยกชื่อไอเทมไว้ข้างล่าง
            if (text != null) 
            {
                text.gameObject.SetActive(true);
                text.text = "You got";
            }
            if (textItemName != null) textItemName.text = key.KeyName;
        }
        
        Image.sprite = key.imageShowGotItem;

        // เล่นเสียงตอนได้รับไอเทมจาก SoundManager โดยตรง
        SoundManager.Instance.PlayGotItemSound();
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Gamemanager.Instance.uiIngame.panelPopUpManager.ClosePopUp();
        }
    }
}
