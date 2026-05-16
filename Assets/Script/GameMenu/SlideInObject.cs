using System.Collections;
using UnityEngine;

public class SlideInObject : MonoBehaviour
{
    [Header("Position Settings")]
    [Tooltip("ระยะห่างจากจุดปัจจุบันที่จะให้ไปซ่อน (ค่าเริ่มต้นคือลอยขึ้นไป +1000 แกน Y)")]
    public Vector3 offScreenOffset = new Vector3(0, 1000, 0);

    [Header("Target Position Settings (ล็อคตำแหน่ง)")]
    [Tooltip("ตำแหน่งเป้าหมายที่ต้องการให้ UI เลื่อนมาหยุด")]
    public Vector3 targetPosition;

    [Tooltip("ติ๊กถูกเพื่อบังคับให้ระบบใช้ค่า Target Position ที่เซฟไว้ (แก้บั๊กตำแหน่งเพี้ยนตอน Scale)")]
    public bool useSavedPosition = false;

    private Vector3 offScreenPosition;

    [Header("Animation Settings")]
    public float moveDuration = 0.5f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool autoMoveInOnStart = true;

    [Header("Shake Settings (เมื่อถึงจุดหมาย)")]
    public bool enableShake = true;
    public float shakeDuration = 0.3f;
    public float shakeAmount = 5f;
    public int shakeOscillations = 3;

    private RectTransform rectTransform;
    private Transform targetTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetTransform = transform;

        // 1. กำหนดจุดเป้าหมาย (ถ้าไม่ได้เปิดโหมดล็อคไว้ ถึงจะดึงค่าปัจจุบันมาใช้)
        if (!useSavedPosition)
        {
            if (rectTransform != null)
            {
                targetPosition = rectTransform.anchoredPosition;
            }
            else
            {
                targetPosition = transform.localPosition;
            }
        }

        // 2. คำนวณ "จุดซ่อน" อัตโนมัติ (เอาตำแหน่งเป้าหมาย + ระยะ Offset)
        offScreenPosition = targetPosition + offScreenOffset;
    }

    void OnEnable()
    {
        StopAllCoroutines();

        SetPosition(offScreenPosition);
        targetTransform.localRotation = Quaternion.identity;

        if (autoMoveInOnStart)
        {
            MoveIn();
        }
    }

    public void MoveIn()
    {
        StopAllCoroutines();
        StartCoroutine(MoveInSequenceRoutine());
    }

    public void MoveOut()
    {
        StopAllCoroutines();
        StartCoroutine(SlideRoutine(targetPosition, offScreenPosition));
        targetTransform.localRotation = Quaternion.identity;
    }

    private IEnumerator MoveInSequenceRoutine()
    {
        yield return StartCoroutine(SlideRoutine(offScreenPosition, targetPosition));

        if (enableShake && shakeDuration > 0f)
        {
            yield return StartCoroutine(ShakeRotationRoutine());
        }
    }

    private IEnumerator SlideRoutine(Vector3 startPos, Vector3 endPos)
    {
        float timeElapsed = 0f;

        while (timeElapsed < moveDuration)
        {
            timeElapsed += Time.deltaTime;

            float t = timeElapsed / moveDuration;
            float curveValue = moveCurve.Evaluate(t);

            Vector3 currentPos = Vector3.LerpUnclamped(startPos, endPos, curveValue);
            SetPosition(currentPos);

            yield return null;
        }

        SetPosition(endPos);
    }

    private IEnumerator ShakeRotationRoutine()
    {
        float timeElapsed = 0f;

        while (timeElapsed < shakeDuration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / shakeDuration;

            float currentMaxAngle = shakeAmount * (1.0f - t);
            float angle = currentMaxAngle * Mathf.Sin(t * shakeOscillations * 2f * Mathf.PI);

            targetTransform.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }

        targetTransform.localRotation = Quaternion.identity;
    }

    private void SetPosition(Vector3 pos)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = pos;
        }
        else
        {
            transform.localPosition = pos;
        }
    }

    // ==========================================
    // 🛠️ ระบบบันทึกตำแหน่งในหน้า Editor
    // ==========================================
#if UNITY_EDITOR
    [ContextMenu("📍 บันทึกตำแหน่งปัจจุบันเป็น 'จุดหมาย' (Target Position)")]
    private void SaveCurrentPosition()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            targetPosition = rt.anchoredPosition;
        }
        else
        {
            targetPosition = transform.localPosition;
        }

        useSavedPosition = true; // เปิดโหมดล็อคตำแหน่งให้อัตโนมัติ
        UnityEditor.EditorUtility.SetDirty(this); // แจ้ง Unity ว่ามีการอัปเดต เพื่อให้เรากด Save Scene ได้
        Debug.Log($"[SlideInObject] บันทึกตำแหน่งเป้าหมายสำเร็จ! ({targetPosition})");
    }
#endif
}