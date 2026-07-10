using UnityEngine;

public class ScalePingPong : MonoBehaviour
{
    [Header("ตั้งค่าการย่อขยาย")]
    [Tooltip("ความเร็วในการเต้นของมือชี้")]
    public float speed = 5f;
    
    [Tooltip("ขนาดที่จะขยายเพิ่มขึ้นไปจากเดิม")]
    public float scaleAmount = 0.2f;

    private Vector3 baseScale;

    void Start()
    {
        // จำขนาดเริ่มต้นเอาไว้
        baseScale = transform.localScale;
    }

    void Update()
    {
        // ใช้สูตรคณิตศาสตร์ (Sin Wave) ทำให้มันย่อขยายขึ้นลงแบบนุ่มนวล
        float offset = Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = baseScale + new Vector3(offset, offset, offset);
    }
}
