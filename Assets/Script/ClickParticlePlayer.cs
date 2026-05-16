using UnityEngine;

public class ClickParticlePlayer : MonoBehaviour
{

    [Header("Particle Settings")]
    [Tooltip("ลาก Particle System ที่มีอยู่ในฉาก (Scene) มาใส่ช่องนี้")]
    public ParticleSystem targetParticle;

    [Tooltip("ระยะห่างจากกล้อง (ใช้สำหรับเกม 2D หรือเมื่อคลิกไม่โดนวัตถุ 3D)")]
    public float defaultZDistance = 10f;

    [Header("3D Settings")]
    [Tooltip("เปิดใช้งานถ้าต้องการให้ Particle ย้ายไปบนพื้นผิววัตถุ 3D ที่มี Collider")]
    public bool useRaycastFor3D = false;

    [Header("Emission Counts")]
    public int countOnEmptySpace = 4;
    public int countOnObject = 10;

    private Camera cam;

    void Start()
    {
        // อ้างอิงถึง Main Camera ในฉาก
        cam = Camera.main;
        // ตั้งค่าเริ่มต้น: ให้ Particle หยุดปล่อยแบบอัตโนมัติ (Manual Emit เท่านั้น)
        if (targetParticle != null)
        {
            var emission = targetParticle.emission;
            emission.enabled = true;
            emission.rateOverTime = 0; // ปิดการไหลแบบปกติ
            // ลบ Burst ใน Inspector ออกด้วยเพื่อให้สคริปต์คุม 100%
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleClick(Input.GetTouch(0).position);
        }
    }

    void HandleClick(Vector3 screenPos)
    {
        SoundManager.Instance.PlayOnClickSound();

        if (targetParticle == null) return;

        Vector3 spawnPosition;
        int emitCount;

        // แปลงตำแหน่งเมาส์บนจอ ให้เป็นตำแหน่งในโลกของเกม
        Vector2 mousePos2D = cam.ScreenToWorldPoint(screenPos);

        // ยิง Raycast แบบ 2D ลงไปที่จุดนั้นโดยตรง (ระยะทาง 0)
        RaycastHit2D hit2D = Physics2D.Raycast(mousePos2D, Vector2.zero);

        // ตรวจสอบว่า "ชนวัตถุ 2D" หรือไม่
        if (hit2D.collider != null)
        {
            // กรณี: กดโดนวัตถุ 2D
            spawnPosition = hit2D.point;
            spawnPosition.z = defaultZDistance; // บังคับแกน Z ไม่ให้พาร์ติเคิลจม
            emitCount = countOnObject; // 10
            Debug.Log("Hit 2D Object: " + hit2D.collider.gameObject.name);
        }
        else
        {
            // กรณี: กดพื้นที่ว่าง
            screenPos.z = defaultZDistance;
            spawnPosition = cam.ScreenToWorldPoint(screenPos);
            emitCount = countOnEmptySpace; // 4
            Debug.Log("Empty Space");
        }

        targetParticle.transform.position = spawnPosition;
        targetParticle.Emit(emitCount);
    }
}