using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GotItem : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image Image;
    public KeyItem currentItem;

    public void SetUpItem(KeyItem key) {
        currentItem = key;
        if (key.name == "ItemDemo")
        {
            // ถ้าเป็นหน้าจอจบเดโม ไม่ต้องใส่คำว่า "You got"
            text.text = key.KeyName; 
        }
        else
        {
            // ถ้าเป็นไอเทมปกติ ให้ใส่ "You got " นำหน้าเหมือนเดิม
            text.text = "You got " + key.KeyName;
        }
        
        Image.sprite = key.imageShowGotItem;
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Gamemanager.Instance.uiIngame.panelPopUpManager.ClosePopUp();
        }
    }
}
