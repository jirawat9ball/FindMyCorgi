using System.Collections;
using UnityEngine;

public class ClickToMoveObject : Obstacle
{
    [Header("Move Settings")]
    [Tooltip("ตำแหน่งเป้าหมายที่ต้องการให้วัตถุเลื่อนไป (สร้าง GameObject เปล่าๆ แล้วลากมาใส่ช่องนี้)")]
    public Transform targetTransform;
    
    [Tooltip("ความเร็วในการเลื่อน")]
    public float moveSpeed = 5f;

    [Tooltip("เมื่อคลิกอีกครั้งต้องการให้เลื่อนกลับที่เดิมหรือไม่?")]
    public bool toggleMove = true; 

    [Tooltip("ต้องการให้วัตถุหมุนตามองศาของเป้าหมายด้วยหรือไม่?")]
    public bool rotateToTarget = true;

    // ตัวแปรเก็บสถานะการล็อค (สามารถตั้งให้มันใช้กุญแจไขก่อน ค่อยเลื่อนได้)
    public bool objectIsLocked = false; 

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isAtTarget = false;
    private bool isMoving = false;

    protected override void Awake()
    {
        // 🌟 บันทึกค่าตำแหน่งดั้งเดิมไว้ "ก่อน" ที่ตัวแม่ (Obstacle) จะเข้ามาแก้ค่า Z หรือตัวอื่นๆ มาขยับ
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnMouseDown()
    {
        // 1. เช็คสถานะเกม (คลิกได้เฉพาะตอนเป็นฉากเกมเพลย์)
        if (!Gamemanager.Instance.isStateGamePlay()) 
        { 
            return;
        }

        // 2. เช็คการล็อค (สืบทอดระบบมาจาก Interaction)
        if (NeedKey != null)
        {
            objectIsLocked = BlockCheck();
        }

        if (objectIsLocked)
        {
            Debug.Log("Object is locked: " + gameObject.name);
            DoOnLock(); // เรียกเอฟเฟกต์กุญแจล็อค (มีอยู่ใน Interaction แม่)
            return;
        }

        DoneState();
    }

    public override void DoneState()
    {
        base.DoneState();
        // 3. เริ่มทำการเลื่อนและหมุนวัตถุ
        if (!isMoving)
        {
            if (isAtTarget && toggleMove)
            {
                // เลื่อนกลับจุดเดิม
                StartCoroutine(MoveRoutine(originalPosition, originalRotation));
                isAtTarget = false;
            }
            else if (!isAtTarget && targetTransform != null)
            {
                // เลื่อนและหมุนไปที่ตำแหน่งเป้าหมาย
                StartCoroutine(MoveRoutine(targetTransform.position, targetTransform.rotation));
                isAtTarget = true;
            }
            else if (targetTransform == null)
            {
                Debug.LogWarning("⚠️ อย่าลืมลากเป้าหมายกรอกลงในช่อง targetTransform ด้วยนะครับ! - " + gameObject.name);
            }
        }
    }

    private IEnumerator MoveRoutine(Vector3 destinationPos, Quaternion destinationRot)
    {
        isMoving = true;

        // เลื่อนและหมุนแบบ Smooth โดยค่อยๆ ขยับเข้าหาเป้าหมาย
        while (
            Vector3.Distance(transform.position, destinationPos) > 0.01f ||
            (rotateToTarget && Quaternion.Angle(transform.rotation, destinationRot) > 0.5f)
        )
        {
            transform.position = Vector3.Lerp(transform.position, destinationPos, Time.deltaTime * moveSpeed);
            
            if (rotateToTarget)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, destinationRot, Time.deltaTime * moveSpeed);
            }
            
            yield return null;
        }

        // จัดตำแหน่งและองศาให้ไปจบที่เป้าหมายแบบเป๊ะๆ 100%
        transform.position = destinationPos; 
        if (rotateToTarget)
        {
            transform.rotation = destinationRot;
        }

        isMoving = false;
    }

#if UNITY_EDITOR
    [ContextMenu("✨ Create New Target Transform")]
    public void CreateTargetTransform()
    {
        // 1. สร้าง GameObject ใหม่
        GameObject newTarget = new GameObject("[Target] " + gameObject.name);
        
        // 2. ตั้งตำแหน่งเริ่มต้นให้เท่ากับวัตถุปัจจุบัน
        newTarget.transform.position = transform.position;
        newTarget.transform.rotation = transform.rotation;
        
        // 3. จัดกลุ่มให้อยู่ที่เดียวกับวัตถุ (เพื่อให้ Hierarchy ไม่รก)
        if (transform.parent != null)
        {
            newTarget.transform.SetParent(transform.parent);
        }

        // 4. บันทึกลงในช่อง targetTransform
        targetTransform = newTarget.transform;

        // 5. บันทึก Undo และ Mark Dirty เพื่อให้ Unity เซฟค่า
        UnityEditor.Undo.RegisterCreatedObjectUndo(newTarget, "Create Target Transform");
        UnityEditor.EditorUtility.SetDirty(this);
        
        // เลือกวัตถุใหม่ให้เลยเพื่อให้ผู้เล่นลากต่อได้ทันที
        UnityEditor.Selection.activeGameObject = newTarget;

        Debug.Log($"✅ สร้างเป้าหมายใหม่ให้ '{gameObject.name}' เรียบร้อย! ลากตัวเครื่องหมายในหน้า Scene เพื่อกำหนดตำแหน่งปลายทางได้เลยครับ");
    }

    [ContextMenu("📍 Snap Target to Current Position")]
    public void SnapTargetToCurrent()
    {
        if (targetTransform == null)
        {
            Debug.LogWarning("⚠️ ไม่พบเป้าหมาย! กรุณาสร้างเป้าหมายก่อน หรือใช้เมนู Create New Target ก่อนครับ");
            return;
        }

        // บันทึก Undo สำหรับตัว Target (เพื่อให้กด Ctrl+Z ย้อนกลับตำแหน่ง Target ได้)
        UnityEditor.Undo.RecordObject(targetTransform, "Snap Target to Current Position");
        
        // ย้ายตำแหน่งและองศามาที่วัตถุปัจจุบัน
        targetTransform.position = transform.position;
        targetTransform.rotation = transform.rotation;

        Debug.Log($"📍 ย้ายเป้าหมาย '{targetTransform.name}' มายังตำแหน่งปัจจุบันของ '{gameObject.name}' เรียบร้อยครับ!");
    }
#endif
}
