using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("การตั้งค่าแอนิเมชัน")]
    public float moveXAmount = -20f; // เลื่อนซ้าย
    public float moveYAmount = 0f;
    public float speed = 15f;

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector2 targetPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        // 🌟 เปลี่ยนมาใช้ localPosition แทน anchoredPosition เพื่อเลี่ยงการถูก Anchor ล็อก
        originalPosition = rectTransform.localPosition;
        targetPosition = originalPosition;
    }

    void Update()
    {
        // เลื่อนไปยังตำแหน่งเป้าหมายอย่างนุ่มนวล
        rectTransform.localPosition = Vector2.Lerp(rectTransform.localPosition, targetPosition, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData) { MoveToTarget(); }
    public void OnPointerExit(PointerEventData eventData) { MoveToOriginal(); }
    public void OnSelect(BaseEventData eventData) { MoveToTarget(); }
    public void OnDeselect(BaseEventData eventData) { MoveToOriginal(); }

    private void MoveToTarget()
    {
        targetPosition = originalPosition + new Vector2(moveXAmount, moveYAmount);
        Debug.Log($"[Hover] สั่งปุ่มเลื่อนไปที่พิกัด: {targetPosition}"); // ดูใน Console ว่าค่าเปลี่ยนไหม
    }

    private void MoveToOriginal()
    {
        targetPosition = originalPosition;
    }
}