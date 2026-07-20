using UnityEngine;

public class ClickParticlePlayer : MonoBehaviour
{
    [Header("Sprite Animation Settings")]
    [Tooltip("ใส่รูปภาพแอนิเมชันตอนกดโดน 'ที่ว่างเปล่า' (ลากเฟรม 1-2-3-4 มาใส่เรียงกันได้เลย)")]
    public Sprite[] clickBlankSprites;
    
    [Tooltip("ใส่รูปภาพแอนิเมชันตอนกดโดน 'น้องหมา/สิ่งของ' (ลากเฟรม 1-2-3-4 มาใส่เรียงกันได้เลย)")]
    public Sprite[] clickFoundSprites;
    
    [Tooltip("ระยะเวลาการแสดงรูปแต่ละเฟรม (ยิ่งน้อยยิ่งเล่นเร็ว)")]
    public float effectFrameDuration = 0.05f;
    
    [Tooltip("ขนาดของเอฟเฟกต์")]
    public Vector3 effectScale = new Vector3(1, 1, 1);

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
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
        // 🌟 ป้องกันไม่ให้เล่นเสียงและ Particle เมื่อกดโดนปุ่ม UI
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            // ตรวจสอบว่า เมาส์ หรือ นิ้วสัมผัส อยู่บน UI หรือไม่
            if (Application.isMobilePlatform && Input.touchCount > 0)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
            }
            else
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
            }
        }

        Vector3 spawnPosition;
        Sprite[] spritesToPlay = null; // 🌟 เก็บค่าว่าจะใช้รูปเซ็ตไหน

        Vector2 mousePos2D = cam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit2D = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit2D.collider != null)
        {
            // 🌟 เช็คว่าวัตถุนี้มีเสียงเฉพาะตัวไหม (สคริปต์ Interaction หรือ ClickObject)
            Interaction interaction = hit2D.collider.GetComponent<Interaction>();
            ClickObject clickObj = hit2D.collider.GetComponent<ClickObject>();
            Dog dogObj = hit2D.collider.GetComponent<Dog>();

            if (dogObj != null && dogObj.realDog)
            {
                // 🌟 เล่นเสียงเฉพาะของน้องหมาตัวจริง
                SoundManager.Instance.PlayOnClickRealDogSound();
            }
            else if (clickObj != null)
            {
                if (clickObj.clipClick != null && clickObj.clipClick.Length > 0)
                {
                    AudioClip randomClip = clickObj.clipClick[Random.Range(0, clickObj.clipClick.Length)];
                    SoundManager.Instance.PlayCustomSound(randomClip);
                }
                else
                {
                    SoundManager.Instance.PlayOnClickSound();
                }
            }
            else if (interaction != null && interaction.clipClick != null && interaction.clipClick.Length > 0)
            {
                AudioClip randomClip = interaction.clipClick[Random.Range(0, interaction.clipClick.Length)];
                SoundManager.Instance.PlayCustomSound(randomClip);
            }
            else
            {
                // โดนวัตถุ แต่ไม่ได้ตั้งเสียงเฉพาะไว้ เล่นเสียงปกติ
                SoundManager.Instance.PlayOnClickSound();
            }
            
            spawnPosition = hit2D.point;
            spawnPosition.z = 10f; 
            spritesToPlay = clickFoundSprites; // 🌟 กำหนดให้ใช้รูปตอนเจอของ
        }
        else
        {
            // 🌟 กดโดนที่ว่างเปล่า เล่นเสียง Empty
            SoundManager.Instance.PlayOnEmptyClickSound();
            
            screenPos.z = 10f;
            spawnPosition = cam.ScreenToWorldPoint(screenPos);
            spritesToPlay = clickBlankSprites; // 🌟 กำหนดให้ใช้รูปตอนกดที่ว่าง
        }

        // 🌟 ถ้ายกเลิกใช้ Particle และหันมาใส่รูปภาพตรงๆ
        if (spritesToPlay != null && spritesToPlay.Length > 0)
        {
            StartCoroutine(PlaySpriteEffect(spritesToPlay, spawnPosition));
        }
    }

    // 🌟 ฟังก์ชันสำหรับสร้าง Object ชั่วคราวมาสลับรูปภาพให้เป็นแอนิเมชัน แล้วทำลายทิ้ง
    private System.Collections.IEnumerator PlaySpriteEffect(Sprite[] sprites, Vector3 position)
    {
        GameObject fxObj = new GameObject("ClickEffect");
        fxObj.transform.position = position;
        fxObj.transform.localScale = effectScale;
        
        SpriteRenderer sr = fxObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 9999; // ให้อยู่ชั้นบนสุด จะได้ไม่โดนฉากบัง

        // สลับรูปทีละเฟรม
        for (int i = 0; i < sprites.Length; i++)
        {
            sr.sprite = sprites[i];
            yield return new WaitForSeconds(effectFrameDuration);
        }

        // เล่นจบแล้วทำลายทิ้งเลย
        Destroy(fxObj);
    }
}
