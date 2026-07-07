using UnityEngine;

public class ClickObjectDogLimit : ClickObject
{
    [Header("Dog Limit Lock")]
    [Tooltip("จำนวนหมาที่ต้องหาให้เจอในฉากก่อนจึงจะคลิกได้")]
    public int requiredDogsLimit = 40;

    [Header("Dog Limit UI")]
    [Tooltip("ใส่ TextMeshPro สำหรับแสดงจำนวนหมาที่เหลือ (เช่น 40) ณ จุดเกต")]
    public TMPro.TextMeshPro dogLimitTxt;

    [Tooltip("ใส่ GameObject รูปภาพพื้นหลัง (BG) ของตัวเลขเพื่อเปิด/ปิดพร้อมกัน")]
    public GameObject dogLimitBG;

    private void Update()
    {
        UpdateDogLimitUI();
    }

    protected override void Start()
    {
        base.Start();

        // 🌟 ตั้งค่า Sorting Layer ของ 3D TextMeshPro ให้แสดงผลทับเหนือสไปรท์ของตัวที่กั้นเกต
        if (dogLimitTxt != null)
        {
            MeshRenderer meshRenderer = dogLimitTxt.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                if (spriteRenderer != null)
                {
                    meshRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                    meshRenderer.sortingOrder = spriteRenderer.sortingOrder + 2; // ทับด้านหน้า 2 ระดับ
                }
                else
                {
                    meshRenderer.sortingOrder = 999;
                }
            }
        }

        // 🌟 ตั้งค่า Sorting Layer ของ BG ให้อยู่ระหว่างกลาง (หลังตัวเลข แต่อยู่หน้าตัวกั้น)
        if (dogLimitBG != null)
        {
            SpriteRenderer bgRenderer = dogLimitBG.GetComponent<SpriteRenderer>();
            if (bgRenderer != null && spriteRenderer != null)
            {
                bgRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                bgRenderer.sortingOrder = spriteRenderer.sortingOrder + 1; // ทับด้านหน้า 1 ระดับ
            }
        }
    }

    private void UpdateDogLimitUI()
    {
        bool showUI = false;
        int remaining = 0;

        // 🌟 เช็คว่ายังไม่เคลียร์ และซีนปัจจุบันมีข้อมูลหมาที่ต้องการ
        if (currentState != ObstacleState.Done && gameObject.activeInHierarchy && requiredDogsLimit > 0)
        {
            if (Gamemanager.Instance != null && Gamemanager.Instance.currentZone != null)
            {
                SceneHandle currentScene = Gamemanager.Instance.currentZone.currentScene;
                if (currentScene != null)
                {
                    int currentFound = currentScene.foundDogs.Count;
                    remaining = requiredDogsLimit - currentFound;
                    if (remaining > 0)
                    {
                        showUI = true;
                    }
                }
            }
        }

        // 🌟 จัดการเปิด/ปิด Text
        if (dogLimitTxt != null)
        {
            if (showUI)
            {
                dogLimitTxt.text = remaining.ToString();
                dogLimitTxt.gameObject.SetActive(true);
            }
            else
            {
                dogLimitTxt.gameObject.SetActive(false);
            }
        }

        // 🌟 จัดการเปิด/ปิด BG
        if (dogLimitBG != null)
        {
            dogLimitBG.SetActive(showUI);
        }
    }

    protected override void OnMouseDown()
    {
        if (!Gamemanager.Instance.isStateGamePlay())
        {
            return;
        }

        // 🌟 1. เช็คจำนวน Corgi ในฉาก
        if (requiredDogsLimit > 0)
        {
            SceneHandle currentScene = Gamemanager.Instance.currentZone.currentScene;
            if (currentScene != null && currentScene.foundDogs.Count < requiredDogsLimit)
            {
                onFail?.Invoke();
                // 🌟 หากหา Corgi ในฉากยังไม่ครบกำหนด แสดงกล่องข้อความโต้ตอบ
                UIManager.Instance.ShowDialog("dialog_need_more_dogs");
                return;
            }
        }

        // 🌟 2. ถ้าผ่านเงื่อนไข Corgi ครบแล้ว ให้ทำคำสั่งไขกุญแจ/คลิก ของตัวแม่ปกติ
        base.OnMouseDown();
    }
}
