using UnityEngine;

public enum ObstacleState
{
    Normal,
    Hint,
    Done
}

public abstract class Obstacle : Interaction
{
    [Header("State Settings")]
    public ObstacleState startState = ObstacleState.Normal;
    [HideInInspector] public ObstacleState currentState;

    Dog dog;
    
    // ดึงค่าการตั้งค่าจากฉากปัจจุบัน (เผื่อดึงสี Hint มาใช้)
    SceneHandle sceneHandle
    {
        get { return Gamemanager.Instance.currentZone.currentScene; }
    }

    public void SetDog(Dog dog)
    {
        this.dog = dog;
    }

    protected override void Awake()
    {
        base.Awake();
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
    }

    protected override void Start()
    {
        base.Start();
        ChangeState(startState);
    }

    // ==========================================
    // 🌟 ระบบจัดการสถานะแบบเดียวกับ Dog.cs
    // ==========================================
    public void ChangeState(ObstacleState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case ObstacleState.Normal:
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
                break;

            case ObstacleState.Hint:
                // ย้อมสีสิ่งกีดขวางเป็นคำใบ้ (ให้รู้ว่าต้องคุ้ยตรงนี้)
                if (spriteRenderer != null) spriteRenderer.color = sceneHandle.HintDogColor;
                break;

            case ObstacleState.Done:
                if (spriteRenderer != null) spriteRenderer.color = Color.white; // เอาสีย้อมคืน
                
                // ตรวจสอบว่ามีหมาซ่อนอยู่หรือไม่ ถ้ามีให้โผล่ออกมา!
                if (dog != null && dog.currentState == DogState.Hidden)
                {
                    dog.ChangeState(DogState.Visible);
                }
                break;
        }
    }

    public override void DoneState()
    {
        base.DoneState();
        if (currentState != ObstacleState.Done)
        {
            ChangeState(ObstacleState.Done);
        }
    }

    public void OnHint()
    {
        if (currentState != ObstacleState.Done)
        {
            ChangeState(ObstacleState.Hint);
        }
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        // ถ้าอยู่ในโหมดคำใบ้ แล้วโดนคลิก ให้หายเรืองแสง (กลับสู่สภาพเดิมแบบเนียนๆ)
        if (currentState == ObstacleState.Hint)
        {
            ChangeState(ObstacleState.Normal);
        }
    }
}
