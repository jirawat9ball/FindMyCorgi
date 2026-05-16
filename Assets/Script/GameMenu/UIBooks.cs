using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBooks : MonoBehaviour
{
    [Header("Items")]
    public GameObject ItemsPanal;
    public Image TabImageItem;
    public Image ImageInfomation;
    public TextMeshProUGUI textInfomation;
    public TextMeshProUGUI textItemName;
    public SlotItem[] KeyItemslots;
    public SlotItem[] Gemslots;
    [Header("Travel")]
    public Image TabImageTravel;
    public GameObject TravelPanal;
    private void Start()
    {
        SetUpSlots();
        ToggleTab(true);

    }
    public void OnClickTap(int i)
    {
        bool isTabItem = (i == 0);
        ToggleTab(isTabItem);
        ItemsPanal.SetActive(isTabItem);
        TravelPanal.SetActive(!isTabItem);
    }

    private void ToggleTab(bool isTabItem)
    {
        TabImageItem.color = isTabItem ? Color.white : Color.gray;
        TabImageTravel.color = !isTabItem ? Color.white : Color.gray;
    }

    private void SetUpSlots()
    {
        List<KeyItem> AllItems = Gamemanager.Instance.Collectable;
        List<KeyItem> ListItems = new List<KeyItem>();
        List<KeyItem> Liststons = new List<KeyItem>();

        foreach (KeyItem item in AllItems) {
            if (item.isStone == true)
            {
                Liststons.Add(item);
            }
            else {
                ListItems.Add(item);
            }
        }


        for (int i = 0; i < ListItems.Count; i++) {
            KeyItemslots[i].SetUpSlot(ListItems[i], this);
        }
        for (int i = 0; i < Liststons.Count; i++)
        {
            Gemslots[i].SetUpSlot(Liststons[i], this);
        }
        if (ListItems.Count > 0) {
            ShowInfomation(ListItems[0]);
        }
        
    }
    public void ShowInfomation(KeyItem keyItem) {
        ImageInfomation.sprite = keyItem.Image;
        textInfomation.text = keyItem.KeyInfomation;
        textItemName.text = keyItem.KeyName;
    }
}
