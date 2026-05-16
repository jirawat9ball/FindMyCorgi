using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image itemImage;
    public Image BG;
    public Button button;
    public bool setUp;
    KeyItem keyItem;
    UIBooks uIInventory;

    void Start() {
        button.onClick.AddListener(OnClickSlot);
    }

    void OnClickSlot() {
        uIInventory.ShowInfomation(keyItem);
    }
    public void SetUpSlot(KeyItem _keyItem,UIBooks _uIInventory) {
        setUp = true;
        button.interactable = true;
        keyItem = _keyItem;
        itemImage.enabled = true;
        itemImage.sprite = keyItem.Image;
        uIInventory = _uIInventory;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (setUp)
        { 
            BG.enabled = true; 
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BG.enabled = false;
    }
}
