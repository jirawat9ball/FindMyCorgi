using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AnimatedLineDrawer : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform[] points;

    [Tooltip("ความเร็วในการลากเส้นลากเส้น")]
    public float drawSpeed = 5f;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // รีเซ็ตเส้นให้ว่างเปล่าตอนเริ่ม
        lineRenderer.positionCount = 0;

        StartCoroutine(DrawLineAnimated());
    }

    IEnumerator DrawLineAnimated()
    {
        yield return new WaitForSeconds(0.5f); // รอครึ่งวินาทีก่อนเริ่มวาด
        if (points == null || points.Length < 2) yield break;

        // วางจุดแรกสุดไว้ก่อน
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, points[0].position);

        // ค่อยๆ วาดไปยังจุดถัดไปทีละจุด
        for (int i = 1; i < points.Length; i++)
        {
            // เพิ่มจุดปลายทางใหม่เข้าไป (แต่ให้จุดเริ่มต้นอยู่ตำแหน่งเดิมก่อน)
            lineRenderer.positionCount = i + 1;

            Vector3 startPosition = points[i - 1].position;
            Vector3 targetPosition = points[i].position;

            float distance = Vector3.Distance(startPosition, targetPosition);
            float currentDistance = 0f;

            // ลูปนี้จะค่อยๆ ขยับปลายเส้นให้ยืดยาวออกไปหา target
            while (currentDistance < distance)
            {
                currentDistance += Time.deltaTime * drawSpeed;
                float t = Mathf.Clamp01(currentDistance / distance);

                Vector3 interpolatedPosition = Vector3.Lerp(startPosition, targetPosition, t);
                lineRenderer.SetPosition(i, interpolatedPosition);

                yield return null; // รอเฟรมถัดไปแล้ววาดต่อ
            }

            // จัดตำแหน่งให้ตรงเป๊ะเมื่อวาดเสร็จ 1 ท่อน
            lineRenderer.SetPosition(i, targetPosition);
        }
    }
}