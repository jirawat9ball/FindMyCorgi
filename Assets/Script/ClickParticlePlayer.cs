using UnityEngine;

public class ClickParticlePlayer : MonoBehaviour
{
    [Header("Particle Settings")]
    [Tooltip("Particle System in the scene")]
    public ParticleSystem targetParticle;

    [Tooltip("Z distance for 2D raycast")]
    public float defaultZDistance = 10f;

    [Header("3D Settings")]
    [Tooltip("Enable for 3D raycast")]
    public bool useRaycastFor3D = false;

    [Header("Emission Counts")]
    public int countOnEmptySpace = 4;
    public int countOnObject = 10;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (targetParticle != null)
        {
            var emission = targetParticle.emission;
            emission.enabled = true;
            emission.rateOverTime = 0; 
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
        int emitCount;

        Vector2 mousePos2D = cam.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit2D = Physics2D.Raycast(mousePos2D, Vector2.zero);

        if (hit2D.collider != null)
        {
            // 🌟 เช็คว่าวัตถุนี้มีเสียงเฉพาะตัวไหม (สคริปต์ Interaction)
            Interaction interaction = hit2D.collider.GetComponent<Interaction>();
            if (interaction != null && interaction.clipClick != null && interaction.clipClick.Length > 0)
            {
                // สุ่มเสียงจาก Array
                AudioClip randomClip = interaction.clipClick[Random.Range(0, interaction.clipClick.Length)];
                SoundManager.Instance.PlayCustomSound(randomClip);
            }
            else
            {
                // โดนวัตถุ เล่นเสียงปกติ
                SoundManager.Instance.PlayOnClickSound();
            }
            
            spawnPosition = hit2D.point;
            spawnPosition.z = defaultZDistance; 
            emitCount = countOnObject;
        }
        else
        {
            // 🌟 กดโดนที่ว่างเปล่า เล่นเสียง Empty
            SoundManager.Instance.PlayOnEmptyClickSound();
            
            screenPos.z = defaultZDistance;
            spawnPosition = cam.ScreenToWorldPoint(screenPos);
            emitCount = countOnEmptySpace;
        }

        if (targetParticle != null)
        {
            targetParticle.transform.position = spawnPosition;
            targetParticle.Emit(emitCount);
        }
    }
}
