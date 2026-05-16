using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems; // ต้อง using ตัวนี้ถึงจะจับ Event เมาส์ได้

public class UIButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Settings")]
    [Tooltip("ขนาดที่จะขยายเพิ่ม เช่น 1.1 คือใหญ่ขึ้น 10%")]
    public float scaleMultiplier = 1.1f;

    [Tooltip("ความเร็วในการขยายตัว")]
    public float scaleSpeed = 15f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Start()
    {
        // บันทึกขนาดดั้งเดิมของปุ่มเอาไว้ตอนเริ่มเกม
        originalScale = Vector3.one;
    }

    // ทำงานเมื่อเอาเมาส์มา "ชี้" (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        // สั่งขยายขนาด
        Vector3 targetScale = originalScale * scaleMultiplier;
        scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale));
    }

    // ทำงานเมื่อเอาเมาส์ "ออก"
    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        // สั่งหดกลับไปขนาดเดิม
        scaleCoroutine = StartCoroutine(ScaleRoutine(originalScale));
    }

    // ตัวจัดการแอนิเมชันให้การย่อ/ขยายดูสมูท (Smooth)
    private IEnumerator ScaleRoutine(Vector3 targetScale)
    {
        // ใช้ Lerp ค่อยๆ ปรับขนาดไปหาเป้าหมาย
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            yield return null; // รอเฟรมถัดไป
        }

        // บังคับให้ขนาดเป๊ะๆ ในตอนจบ
        transform.localScale = targetScale;
    }
}