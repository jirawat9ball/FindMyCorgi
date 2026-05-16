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
    public void UpdateLostDog(int normalCount, int specialCount)
    {
        if (NormalDogsTxt != null) NormalDogsTxt.text = normalCount.ToString();
        if (SpecialDogTxt != null) SpecialDogTxt.text = specialCount.ToString();
    }
    public void UpdateSnack(int snackCount)
    {
        if (SnackTxt != null) SnackTxt.text = snackCount.ToString();
    }
    public void ShowannounceText(string t) {
        announceText.text = t;
    }

}