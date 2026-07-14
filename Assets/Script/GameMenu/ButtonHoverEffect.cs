using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("ค่าการเคลื่อนที่")]
    public float moveXAmount = -20f; // ขยับซ้าย
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
        // ใช้ localPosition แทน anchoredPosition เพราะทำงานถูกต้องกว่าเมื่อ Anchor เปลี่ยน
        originalPosition = rectTransform.localPosition;
        targetPosition = originalPosition;
    }

    void Update()
    {
        // เคลื่อนที่ไปยัง targetPosition อย่างนุ่มนวล
        rectTransform.localPosition = Vector2.Lerp(rectTransform.localPosition, targetPosition, Time.deltaTime * speed);
    }

    public void OnPointerEnter(PointerEventData eventData) { MoveToTarget(); }
    public void OnPointerExit(PointerEventData eventData) { MoveToOriginal(); }
    public void OnSelect(BaseEventData eventData) { MoveToTarget(); }
    public void OnDeselect(BaseEventData eventData) { MoveToOriginal(); }

    private void MoveToTarget()
    {
        targetPosition = originalPosition + new Vector2(moveXAmount, moveYAmount);
        Debug.Log($"[Hover] เคลื่อนที่ไป hover position: {targetPosition}");
    }

    private void MoveToOriginal()
    {
        targetPosition = originalPosition;
    }

    /// <summary>
    /// เรียกหลังจาก SetParent หรือเปลี่ยน hierarchy เพื่อ update originalPosition
    /// ให้ตรงกับ localPosition จริงหลัง parent เปลี่ยน (ใช้ใน Tutorial)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        originalPosition = rectTransform.localPosition;
        targetPosition = originalPosition;
        Debug.Log($"[Hover] UpdateOriginalPosition -> {originalPosition}");
    }
}