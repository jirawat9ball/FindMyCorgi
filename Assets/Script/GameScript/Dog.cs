using UnityEngine;

// 🌟 สร้าง Enum สำหรับระบุสถานะของหมา
public enum DogState
{
    Hidden,   // ซ่อนตัวอยู่ในกล่อง
    Visible,  // แสดงตัว รอผู้เล่นมาคลิกหา
    Hint,     // ส่องด้วยเรดาร์คำใบ้
    Found     // ถูกหาเจอแล้ว
}

public class Dog : Interaction
{
    [Header("Dog setUp")]
    public string id;
    public bool isSpecial;


    [Header("State Settings")]
    [Tooltip("สถานะเริ่มต้นตอนเริ่มเกม")]
    public DogState startState = DogState.Visible;

    // ตัวแปรเก็บสถานะปัจจุบัน (ซ่อนไว้ไม่ให้รก Inspector)
    [HideInInspector]
    public DogState currentState;

    [Header("Sprite Settings (ภาพหมา)")]
    public Sprite spriteNotFound;
    public Sprite spriteFound;

    [Header("Obstacle Settings (สิ่งกีดขวาง)")]
    public Obstacle blockingObstacle;

    private Vector3 originalScale;
    private Collider2D dogCollider;
    private bool isPopping = false; // 🌟 ป้องกันคลิกย้ำๆ ทำให้อนิเมชันบวมค้าง

    SceneHandle sceneHandle
    {
        get { return Gamemanager.Instance.currentZone.currentScene; }
    }

    protected override void Awake()
    {
        base.Awake();
        originalScale = transform.localScale;
        dogCollider = GetComponent<Collider2D>();
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        blockingObstacle?.SetDog(this);
    }

    protected override void Start()
    {
        base.Start();

        // 🌟 ปล่อยให้ SceneHandle.SetupScene() เป็นคนจัดการเช็คสถานะจากไฟล์เซฟตอนที่กล้องแพนไปถึง
        // ตอนนี้เลยไม่ต้องทำอะไร ให้หมาอยู่เฉยๆ ไปก่อน
    }

    // ==========================================
    // 🌟 ระบบจัดการสถานะ (State Machine)
    // ==========================================
    public void ChangeState(DogState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case DogState.Hidden:
                // ปิดการมองเห็น และ ปิดการคลิก
                if (spriteRenderer != null) spriteRenderer.enabled = false;
                if (dogCollider != null) dogCollider.enabled = false;
                break;

            case DogState.Visible:
                // เปิดให้มองเห็นเป็นภาพ A และ เปิดให้คลิกได้
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                    if (spriteNotFound != null) spriteRenderer.sprite = spriteNotFound;
                    spriteRenderer.color = Color.white; // สีปกติ
                }
                if (dogCollider != null) dogCollider.enabled = true;
                break;

            case DogState.Hint:
                // แสดงภาพหมาเฉลยลางๆ สีตามที่ตั้งไว้เป็นคำใบ้
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                    if (spriteFound != null) spriteRenderer.sprite = spriteFound;
                    spriteRenderer.color = sceneHandle.HintDogColor;
                }
                if (dogCollider != null) dogCollider.enabled = true;
                break;

            case DogState.Found:
                // เปลี่ยนเป็นภาพ B และคง Collider ไว้เผื่อคลิกเด้งดึ๋งเล่น
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                    if (spriteFound != null) spriteRenderer.sprite = spriteFound;
                    spriteRenderer.color = isSpecial ? sceneHandle.FoundSpecialDogColor : sceneHandle.FoundDogColor;
                }
                if (dogCollider != null) dogCollider.enabled = true;
                if (blockingObstacle != null) blockingObstacle.DoneState();
                break;
        }
    }

    // ฟังก์ชันนี้เอาไว้ให้ปุ่ม หรือ ของกีดขวาง (ClickObject) สั่งเรียกใช้เพื่อเปิดเผยตัวหมา
    public void RevealDog()
    {
        if (currentState == DogState.Hidden)
        {
            ChangeState(DogState.Visible);
        }
    }

    protected override void OnMouseDown()
    {
        if (!Gamemanager.Instance.isStateGamePlay()) return;

        // ถ้าซ่อนอยู่ ให้ข้ามการคลิกไปเลย ไม่ให้ทำอะไรทั้งสิ้น
        if (currentState == DogState.Hidden) return;

        base.OnMouseDown();

        if (currentState == DogState.Visible || currentState == DogState.Hint)
        {
            // เพิ่งหาเจอครั้งแรก
            sceneHandle.AddFoundDog(this);
            ChangeState(DogState.Found); // เปลี่ยนสถานะเป็นหาเจอแล้ว
            SoundManager.Instance.PlayOnClickSound();
            onComplete?.Invoke();

            if (!isPopping)
            {
                StopAllCoroutines();
                StartCoroutine(PopRoutine());
            }
        }
        else if (currentState == DogState.Found)
        {
            // กรณีคลิกซ้ำตัวที่เจอแล้ว (เด้งอย่างเดียว)
            if (!isPopping)
            {
                StopAllCoroutines();
                StartCoroutine(PopRoutine());
            }
        }
    }

    public void OnReset()
    {
        ChangeState(startState);
        transform.localScale = originalScale;
    }

    public void OnHint()
    {
        // จะโชว์คำใบ้ได้ ก็ต่อเมื่อหมายังไม่ถูกหาเจอ
        if (currentState != DogState.Found)
        {
            ChangeState(DogState.Hint);

            // ส่งสัญญาณไฟคำใบ้ไปที่สิ่งกีดขวางด้วย (ถ้ามีสิ่งกีดขวางบังอยู่)
            if (blockingObstacle != null)
            {
                blockingObstacle.OnHint();
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        // Draw a small sphere at the pivot to verify Gizmos are enabled
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        string spriteName = (spriteRenderer != null && spriteRenderer.sprite != null) ? spriteRenderer.sprite.name : "No Sprite";
        string labelText = $"Dog: {id}\n({spriteName})";

        // Use a simpler label first to ensure it displays
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, labelText);
    }

    private void OnDrawGizmosSelected()
    {
        OnDrawGizmos();
    }

    private Sprite originalSpriteBeforeAlt;
    private bool isAltTesting = false;

    private void Update()
    {
        if (!Application.isPlaying) return;

        // เช็คว่ากดปุ่ม Alt ซ้ายหรือขวาค้างไว้หรือไม่
        bool isAltHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

        if (isAltHeld && !isAltTesting)
        {
            // จังหวะเริ่มกด Alt: บันทึกภาพปัจจุบันและสลับเป็นภาพ Found เพื่อเทสความตรง
            if (spriteRenderer != null && spriteFound != null)
            {
                originalSpriteBeforeAlt = spriteRenderer.sprite;
                spriteRenderer.sprite = spriteFound;
                spriteRenderer.color = Color.white; // ปรับเป็นสีปกติให้เห็นชัดๆ
                isAltTesting = true;
            }
        }
        else if (!isAltHeld && isAltTesting)
        {
            // จังหวะปล่อย Alt: คืนค่าสถานะปัจจุบันกลับไป
            isAltTesting = false;
            ChangeState(currentState); 
        }
    }

    // --- ส่วนที่เพิ่มใหม่: ระบบดักฟังปุ่ม Alt ในหน้า Scene (ไม่ต้องกด Play) ---
    [UnityEditor.InitializeOnLoadMethod]
    private static void HookAltKeyInEditor()
    {
        UnityEditor.SceneView.duringSceneGui += (sceneView) =>
        {
            Event e = Event.current;
            if (e == null) return;

            // ถ้ามีการกด Alt ค้างไว้
            if (e.alt)
            {
                Dog[] allDogs = GameObject.FindObjectsOfType<Dog>();
                foreach (var dog in allDogs)
                {
                    if (dog.spriteRenderer != null && dog.spriteFound != null)
                    {
                        if (dog.spriteRenderer.sprite != dog.spriteFound)
                        {
                            dog.spriteRenderer.sprite = dog.spriteFound;
                            dog.spriteRenderer.color = Color.white;
                        }
                    }
                }
                sceneView.Repaint();
            }
            // ถ้าปล่อย Alt และไม่ได้รันเกม ให้คืนค่ากลับ
            else if (!Application.isPlaying && (e.type == EventType.MouseMove || e.type == EventType.KeyUp))
            {
                Dog[] allDogs = GameObject.FindObjectsOfType<Dog>();
                foreach (var dog in allDogs)
                {
                    if (dog.spriteRenderer != null)
                    {
                        Sprite target = (dog.startState == DogState.Found) ? dog.spriteFound : dog.spriteNotFound;
                        if (dog.spriteRenderer.sprite != target)
                        {
                            dog.spriteRenderer.sprite = target;
                        }
                    }
                }
                sceneView.Repaint();
            }
        };
    }
#endif


    private System.Collections.IEnumerator PopRoutine()
    {
        isPopping = true; // 🌟 ล็อคไว้กันคลิกรัวๆ
        Vector3 popScale = originalScale * 1.3f;
        float popSpeed = 20f;

        while (Vector3.Distance(transform.localScale, popScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, popScale, Time.deltaTime * popSpeed);
            yield return null;
        }

        while (Vector3.Distance(transform.localScale, originalScale) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * popSpeed);
            yield return null;
        }

        transform.localScale = originalScale;
        isPopping = false; // 🌟 ปลดล็อค
    }
}