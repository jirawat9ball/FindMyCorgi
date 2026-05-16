using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : Graphic
{
    [Header("Line Settings")]
    public RectTransform[] points;
    public float thickness = 10f;

    [Header("Animation")]
    [Range(0f, 1f)]
    [Tooltip("ความคืบหน้าของการวาดเส้น (0 = มองไม่เห็น, 1 = วาดเต็มเส้น)")]
    public float drawProgress = 1f;

    [Tooltip("เส้นกราฟความเร็วในการวาดเส้น (Ease In Out จะสวยที่สุด)")]
    public AnimationCurve drawCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ฟังก์ชัน Coroutine สำหรับให้ CountryManager เรียกใช้
    public IEnumerator AnimateLine(float duration)
    {
        float timeElapsed = 0f;
        drawProgress = 0f;
        SetAllDirty(); // สั่งให้อัปเดตเป็นเส้นว่างเปล่าก่อน

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            // 1. แปลงเวลาให้เป็นเปอร์เซ็นต์ (0.0 ถึง 1.0)
            float t = timeElapsed / duration;

            // 2. เอาเปอร์เซ็นต์ไปเทียบกับเส้นกราฟ AnimationCurve
            float curveValue = drawCurve.Evaluate(t);

            // 3. บังคับค่าให้อยู่ระหว่าง 0 ถึง 1 เท่านั้น ป้องกันเส้นวาดยาวเกินจุดปลายทาง
            drawProgress = Mathf.Clamp01(curveValue);

            SetAllDirty(); // อัปเดตความยาวเส้นทุกเฟรม
            yield return null;
        }

        drawProgress = 1f;
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Length < 2) return;

        // 1. ดึงตำแหน่งพิกัดทั้งหมดมาเก็บไว้ก่อน
        List<Vector2> localPoints = new List<Vector2>();
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
                localPoints.Add(rectTransform.InverseTransformPoint(points[i].position));
        }

        if (localPoints.Count < 2) return;

        // 2. คำนวณระยะทางทั้งหมดของเส้นทาง
        float[] distances = new float[localPoints.Count - 1];
        float totalDistance = 0f;
        for (int i = 0; i < localPoints.Count - 1; i++)
        {
            distances[i] = Vector2.Distance(localPoints[i], localPoints[i + 1]);
            totalDistance += distances[i];
        }

        // 3. คำนวณว่าตอนนี้ต้องวาดเส้นยาวแค่ไหนตาม drawProgress
        float currentTargetDist = totalDistance * drawProgress;
        float currentDist = 0f;
        int prevVIndex = 0;

        // 4. เริ่มวาดเส้นทีละท่อน
        for (int i = 0; i < localPoints.Count - 1; i++)
        {
            if (currentDist >= currentTargetDist) break; // หยุดวาดถ้าความยาวเกิน Progress แล้ว

            Vector2 point1 = localPoints[i];
            Vector2 point2 = localPoints[i + 1];
            float segmentDist = distances[i];

            // ถ้าระยะทางในท่อนนี้ วาดไปได้แค่ครึ่งทาง ให้หาจุดกึ่งกลาง (Lerp)
            if (currentDist + segmentDist > currentTargetDist)
            {
                float t = (currentTargetDist - currentDist) / segmentDist;
                point2 = Vector2.Lerp(point1, point2, t);
            }

            currentDist += segmentDist;

            // --- ส่วนกระบวนการสร้างรูปทรงสี่เหลี่ยมของเส้น ---
            Vector2 dir = (point2 - point1).normalized;
            if (dir == Vector2.zero) continue;
            Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness / 2f);

            int vIndex = vh.currentVertCount;
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;

            vertex.position = point1 + normal; vh.AddVert(vertex);
            vertex.position = point1 - normal; vh.AddVert(vertex);
            vertex.position = point2 + normal; vh.AddVert(vertex);
            vertex.position = point2 - normal; vh.AddVert(vertex);

            vh.AddTriangle(vIndex + 0, vIndex + 1, vIndex + 2);
            vh.AddTriangle(vIndex + 2, vIndex + 1, vIndex + 3);

            // ปิดรอยต่อ
            if (i > 0)
            {
                vertex.position = point1;
                vh.AddVert(vertex);
                int jointIndex = vh.currentVertCount - 1;
                vh.AddTriangle(prevVIndex + 2, vIndex + 0, jointIndex);
                vh.AddTriangle(prevVIndex + 3, vIndex + 1, jointIndex);
            }
            prevVIndex = vIndex;
        }
    }

#if UNITY_EDITOR
    // อัปเดตทันทีเมื่อแก้ค่าในหน้าต่าง Inspector ตอนที่ยังไม่ได้กด Play
    protected override void OnValidate()
    {
        // 1. ดักจับบั๊ก: ถ้าตัวมันถูกลบไปแล้ว ให้หยุดการทำงานทันที
        if (this == null) return;

        base.OnValidate();

        // 2. สั่งอัปเดตเส้น เฉพาะตอนที่ Object นี้ยังเปิดใช้งานอยู่เท่านั้น
        if (IsActive())
        {
            SetAllDirty();
        }
    }
#endif
}