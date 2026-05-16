using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GotItem : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image Image;

    public void SetUpItem(KeyItem key) {
        text.text = "You got " + key.KeyName;
        Image.sprite = key.imageShowGotItem;
    }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            Gamemanager.Instance.uiIngame.panelPopUpManager.ClosePopUp();
        }
    }
}
