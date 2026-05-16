using TMPro;
using UnityEngine;

public enum Direction
{
    UpDown, LeftRigt
}

// 🌟 1. สร้าง Enum ใหม่เพื่อไว้ให้เลือกฝั่งตอนเปิดสำเร็จ
public enum EndBound
{
    Min, Max
}

[System.Serializable]
public class ClampDirection
{
    [Range(-10, 0)]
    public float Min;
    [Range(0, 10)]
    public float Max;

    public ClampDirection() { 
        Min = -1f;
        Max = 1f;
    }
}

public class SlideObject : Obstacle
{
    public float slideSpeed = 5f;
    public Direction direction = Direction.LeftRigt;
    [Header("Completion Settings")]
    [Tooltip("เมื่อเปิดสำเร็จ ให้ของไปค้างอยู่ฝั่งไหน? (Min = ซ้าย/ล่าง, Max = ขวา/บน)")]
    public EndBound doneAtBound = EndBound.Max; // 🌟 2. เพิ่มตัวแปรไว้ตั้งค่าใน Inspector
    public ClampDirection ClampDirection = new ClampDirection();



    private bool isSliding = false;
    private Vector3 offset;
    private Vector3 StartPos;
    Vector3 targetPosition = new Vector3();

    void Awake()
    {
        StartPos = transform.position;

    }
    protected override void OnMouseDown()
    {
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isSliding = true;
        Gamemanager.Instance.cameraPan.enabled = false;
    }

    void OnMouseUp()
    {
        isSliding = false;
        Gamemanager.Instance.cameraPan.enabled = true;
    }

    void OnMouseDrag()
    {
        if (isSliding)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (direction == Direction.LeftRigt)
            {
                float x = Mathf.Clamp(mousePosition.x, StartPos.x + ClampDirection.Min, StartPos.x + ClampDirection.Max);
                targetPosition = new Vector3(x, transform.position.y, transform.position.z);
            }
            else if (direction == Direction.UpDown)
            {
                float y = Mathf.Clamp(mousePosition.y, StartPos.y + ClampDirection.Min, StartPos.y + ClampDirection.Max);
                targetPosition = new Vector3(transform.position.x, y, transform.position.z);
            }

            transform.position = targetPosition;
        }
    }

    public override void DoneState()
    {
        base.DoneState();
        onComplete?.Invoke();

        // 🌟 3. เช็คค่าจากที่ตั้งไว้ใน Inspector ว่าจะให้ค้างที่ Min หรือ Max
        float targetOffset = (doneAtBound == EndBound.Max) ? ClampDirection.Max : ClampDirection.Min;

        if (direction == Direction.LeftRigt)
        {
            // 🌟 4. แก้บั๊ก! ต้องเอา StartPos.x มาบวกเสมอก่อนจัดตำแหน่ง
            float x = StartPos.x + targetOffset;
            targetPosition = new Vector3(x, transform.position.y, transform.position.z);
        }
        else if (direction == Direction.UpDown)
        {
            // 🌟 4. แก้บั๊ก! ต้องเอา StartPos.y มาบวกเสมอก่อนจัดตำแหน่ง
            float y = StartPos.y + targetOffset;
            targetPosition = new Vector3(transform.position.x, y, transform.position.z);
        }

        transform.position = targetPosition;
    }
}