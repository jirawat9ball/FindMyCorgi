using UnityEngine;
using UnityEngine.EventSystems;// Required when using Event data.
using UnityEngine.UI;

public class AddListernerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject Uitxt;
    void Start() {
        OnUiText(false);
    }
    void OnUiText(bool set) {
        Uitxt.gameObject.SetActive(set);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnUiText(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnUiText(false);
    }
}
