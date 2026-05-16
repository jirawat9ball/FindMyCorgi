using UnityEngine;

public class Parallax : MonoBehaviour
{

   
    [Header("Parallax Settings")]
    public bool enableParallax = true;
    public float parallaxAmount = 25f; // ระยะที่จะขยับ (พิกเซล)
    public float parallaxSmoothness = 5f; // ความนุ่มนวล

    private Vector3 initialPosition;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            initialPosition = rectTransform.anchoredPosition;
        }
        else
        {
            initialPosition = transform.localPosition;
        }
    }
    private void Update()
    {
        if (enableParallax)
        {
            HandleParallax();
        }
    }

    private void HandleParallax()
    {
        // 1. หาตำแหน่งเมาส์เทียบกับกึ่งกลางสัดส่วนหน้าจอ (ค่าระหว่าง -0.5 ถึง 0.5)
        Vector2 mousePos = Input.mousePosition;
        float screenX = (mousePos.x / Screen.width) - 0.5f;
        float screenY = (mousePos.y / Screen.height) - 0.5f;

        // 2. คำนวณเป้าหมาย (ขยับสวนทางเมาส์)
        Vector3 targetOffset = new Vector3(-screenX * parallaxAmount, -screenY * parallaxAmount, 0f);

        // 3. ผลักให้ขยับไปหาเป้าหมายอย่างนุ่มนวล
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector3.Lerp(rectTransform.anchoredPosition, initialPosition + targetOffset, Time.deltaTime * parallaxSmoothness);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + targetOffset, Time.deltaTime * parallaxSmoothness);
        }
    }
}
