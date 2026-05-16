using System.Collections.Generic;
using UnityEngine;
using static CameraPan;


public class SceneHandle : MonoBehaviour
{
    public Color FoundSpecialDogColor = new Color32(255, 0, 206, 255);
    public Color FoundDogColor = new Color32(214, 130, 112, 255);
    public Color HintDogColor = new Color32(0, 143, 255, 255);
    [Header("Canera set")]
    public float ToggleRangeZoom = 14f;
    public float maxZoom = 14f;
    public TypePan panType;
    public SpriteRenderer targetSpriteRenderer;
    public SceneObject sceneObject;

    [HideInInspector]
    public List<Dog> lostDogs = new List<Dog>();
    public List<Dog> foundDogs = new List<Dog>();
    public Gate gate;
    ZoneHandle zoneHandle;

#if UNITY_EDITOR
    [ContextMenu("🛠️ DEBUG: Reveal All Dogs (เฉลยทั้งหมด)")]
    public void DebugRevealAllDogs()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("⚠️ ต้องอยู่ในโหมด Play เท่านั้นถึงจะกดเฉลยได้ครับ");
            return;
        }

        List<Dog> dogsToReveal = new List<Dog>(lostDogs);
        foreach (Dog dog in dogsToReveal)
        {
            if (dog != null)
            {
                dog.ChangeState(DogState.Found);
                AddFoundDog(dog);
            }
        }
        Debug.Log($"✅ เฉลยให้แล้วจ้า! พบหมาทั้งหมด {dogsToReveal.Count} ตัว");
    }

    private int debugDogIndex = -1;

    private void Update()
    {
        if (!Application.isPlaying) return;

        // กดลูกศรขวาเพื่อไปตัวถัดไป
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            InspectNextDog(1);
        }
        // กดลูกศรซ้ายเพื่อย้อนกลับ
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            InspectNextDog(-1);
        }
    }

    private void InspectNextDog(int direction)
    {
        Dog[] allDogs = GetAllDog();
        if (allDogs.Length == 0) return;

        debugDogIndex += direction;

        // วนลูป index
        if (debugDogIndex >= allDogs.Length) debugDogIndex = 0;
        if (debugDogIndex < 0) debugDogIndex = allDogs.Length - 1;

        Dog targetDog = allDogs[debugDogIndex];
        if (targetDog != null)
        {
            Debug.Log($"🔍 [Editor Test] ตรวจสอบสุนัข ({debugDogIndex + 1}/{allDogs.Length}): {targetDog.name}");
            
            if (Gamemanager.Instance.cameraPan != null)
            {
                // เลื่อนกล้องไปที่ตำแหน่งหมา และซูมเข้าไปใกล้ๆ (false = zoom in)
                Gamemanager.Instance.cameraPan.TriggerLeap(targetDog.transform.position, false);
            }
        }
    }
#endif

    public void Setup()
    {
        // 🌟 ป้องกันลิสต์ซ้ำซ้อนเวลาถูกเรียกซ้ำ
        lostDogs.Clear();
        foundDogs.Clear();

        // 🌟 เรียกใช้ฟังก์ชันค้นหา SCN
        FindSCNSpriteRenderer();

        Dog[] interactions = GetAllDog();
        for (int i = 0; i < interactions.Length; i++)
        {
            if (interactions[i] != null)
            {
                lostDogs.Add(interactions[i]);
            }
        } // 🌟 นำวงเล็บปีกกาของลูป for ที่หายไปกลับคืนมา!
        
        SoundManager.Instance.PlayBGSound(sceneObject.soundBG);

        // 🌟 A/B Testing Mode 1: 10% Drop on Scene Load
        if (Gamemanager.Instance.snackDropMode == Gamemanager.SnackDropMode.DropOnSceneLoad)
        {
            int randomRoll = Random.Range(0, 100);
            int requiredChance = Gamemanager.Instance.dropChancePercent;
            Debug.Log($"🎲 [A/B Test Mode 1] ทดสอบโอกาสดรอป: สุ่มได้เลข {randomRoll} (ต้องการ < {requiredChance})");

            if (randomRoll < requiredChance) // อิงจากเปอร์เซ็นต์ที่ตั้งไว้ใน Gamemanager
            {
                if (lostDogs.Count > 0) // ให้เกิดใกล้หมาที่ยังหาไม่เจอ จะได้กวาดสายตามาเห็นง่ายๆ
                {
                    Dog rDog = lostDogs[Random.Range(0, lostDogs.Count)];
                    if (Gamemanager.Instance.snackDropPrefab != null)
                    {
                        Vector3 spawnPos = rDog.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                        Instantiate(Gamemanager.Instance.snackDropPrefab, spawnPos, Quaternion.identity, this.transform);
                        Debug.Log("✅ [A/B Test Mode 1] สุ่มตกขนมหมาในฉาก สำเร็จ!! (ดรอปให้แล้ว)");
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ [A/B Test Mode 1] สุ่มผ่านแล้ว! แต่คุณลืมใส่ Prefab ขนมหมาใน Gamemanager ครับ!");
                    }
                }
                else
                {
                    Debug.Log("❌ [A/B Test Mode 1] สุ่มผ่าน! แต่ในฉากนี้ไม่มีน้องหมาเหลือให้ซ่อนขนมแล้ว");
                }
            }
            else
            {
                Debug.Log("❌ [A/B Test Mode 1] เกลือ! รอบนี้ดรอปไม่สำเร็จจ้า");
            }
        }
    }

    public Dog[] GetAllDog()
    {
        // 🌟 ใส่ (true) เพื่อให้มันค้นหาหมาเจอทุกตัว แม้ว่าลูกตัวนั้นจะถูกปิด (Inactive) เอาไว้อยู่ก็ตาม
        return GetComponentsInChildren<Dog>(true);
    }
    // 🌟 ฟังก์ชันใหม่: สำหรับค้นหา SpriteRenderer จากตัวลูกที่ชื่อ SCN
    private void FindSCNSpriteRenderer()
    {   
        if (targetSpriteRenderer != null) return;
        
        targetSpriteRenderer = FindOrCreateSCNRenderer();

        // 🌟 เพิ่มบรรทัดนี้ลงไป: เมื่อหาภาพเจอแล้ว ให้คำนวณซูมทันที!
        if (targetSpriteRenderer != null)
        {
            CalculateAutoZoom();
        }
    }
    
    private SpriteRenderer FindOrCreateSCNRenderer()
    {
        if (targetSpriteRenderer != null) return targetSpriteRenderer;

        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr.gameObject.name.ToUpper().Contains("SCN"))
            {
                return sr;
            }
        }
        return null;
    }
    private void CalculateAutoZoom()
    {
        if (targetSpriteRenderer == null) return;

        // 1. ดึงกรอบขนาดของภาพฉากหลัง
        Bounds bounds = targetSpriteRenderer.bounds;

        // 2. หาอัตราส่วนหน้าจอของเครื่องผู้เล่น (เช่น 16:9 หรือหน้าจอยาวๆ แบบมือถือรุ่นใหม่)
        // ใช้ Camera.main.aspect จะแม่นยำที่สุดครับ
        float screenAspect = Camera.main.aspect;

        // 3. คำนวณขนาดซูมสูงสุดที่กล้องจะพอดีกับ "ความสูง" ของภาพพอดี
        float maxZoomHeight = bounds.size.y / 2f;

        // 4. คำนวณขนาดซูมสูงสุดที่กล้องจะพอดีกับ "ความกว้าง" ของภาพพอดี
        float maxZoomWidth = bounds.size.x / (2f * screenAspect);

        // 5. 🌟 หัวใจสำคัญ: ใช้ Mathf.Min เพื่อเลือกค่าที่ "น้อยกว่า" 
        // รับประกันว่าต่อให้ภาพจะยาวหรือกว้างแค่ไหน กล้องก็จะไม่หลุดขอบภาพเด็ดขาด!
        float autoZoom = Mathf.Min(maxZoomHeight, maxZoomWidth);

        // 6. นำค่าที่คำนวณได้ไปเซ็ตให้ตัวแปรของคุณ
        maxZoom = autoZoom;

        // สำหรับ ToggleRangeZoom คุณสามารถให้เท่ากับ maxZoom เลย 
        // หรือถ้าอยากให้จังหวะ Toggle มันซูมเข้าไปใกล้หน่อย ก็คูณเลขเข้าไปได้ครับ เช่น autoZoom * 0.8f
        ToggleRangeZoom = autoZoom;

        Debug.Log($"[Auto Zoom] คำนวณระยะซูมของฉาก {gameObject.name} ได้ที่: {autoZoom}");
    }
    public void setZone(ZoneHandle _zoneHandle) {
        zoneHandle = _zoneHandle;
    }
    public void SetToGamemanager()
    {
        Gamemanager.Instance.currentZone.currentScene = this;
        Gamemanager.Instance.sceneObject = sceneObject;

        // 🌟 1. ดักแก้ Error SpriteRenderer แย่งกันเกิด (ต้องทำก่อนส่งให้กล้อง!)
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponent<SpriteRenderer>();

            // ถ้าดึงแล้วยัง null อีก แสดงว่าลืมใส่ภาพฉากจริงๆ 
            if (targetSpriteRenderer == null)
            {
                Debug.LogError($"⚠️ ห้ามลืม! GameObject ชื่อ '{gameObject.name}' ที่มีสคริปต์ SceneHandle ยังไม่ได้ใส่ SpriteRenderer (ภาพพื้นหลังฉาก) ครับ!");
                return; // หยุดการทำงาน
            }
        }

        // 🌟 2. ดึงค่า CameraPan มาเช็คก่อน ถ้าหาไม่เจอ ให้ค้นหาในฉากอัตโนมัติ (โค้ดของคุณ)
        CameraPan cameraPan = Gamemanager.Instance.cameraPan;
        if (cameraPan == null)
        {
            cameraPan = FindObjectOfType<CameraPan>(); // ค้นหาสคริปต์กล้องในฉาก
            Gamemanager.Instance.cameraPan = cameraPan; // บันทึกกลับให้ Gamemanager จำไว้ใช้ต่อ
        }

        // 🌟 3. สั่งตั้งค่ากล้อง 
        if (cameraPan != null)
        {
            // ตอนนี้ส่ง this ไปได้อย่างปลอดภัยแล้ว เพราะเรารับประกันว่า targetSpriteRenderer มีค่าแน่นอน
            cameraPan.SetUpCamera(this);
            cameraPan.TriggerLeap(targetSpriteRenderer.bounds.center, true);
        }
        else
        {
            Debug.LogError("⚠️ เกมพังแน่! หาสคริปต์ 'CameraPan' ไม่เจอในฉากเลยครับ ตรวจสอบกล้องด่วน!");
            return;
        }

        // 🌟 4. อัปเดต UI และตั้งค่าฉากต่อ
        UpdateDogUI(); // เปลี่ยนมาเรียกใช้ตัวนี้แทน
        SetupScene();
    }
    public void UpdateDogUI()
    {
        int normalLostCount = 0;
        int specialLostCount = 0;

        foreach (Dog dog in lostDogs)
        {
            if (dog != null)
            {
                if (dog.isSpecial)
                    specialLostCount++;
                else
                    normalLostCount++;
            }
        }

        if (Gamemanager.Instance.uiIngame != null)
        {
            Gamemanager.Instance.uiIngame.UpdateLostDog(normalLostCount, specialLostCount);
        }
    }
    void SetupScene() {
        Dog[] allDogs = GetAllDog();
        foreach (Dog dog in allDogs)
        {
            if (dog == null) continue;
            
            // 🌟 เช็คเลยว่าหมาตัวนี้อยู่ในเซฟของเราแล้วหรือยัง?
            if (Gamemanager.Instance.IsDogFoundInSave(sceneObject.name, dog.name))
            {
                // ถ้าเคยตั้งว่าหาเจอแล้ว ให้จัดเข้าลิสต์และสลับสถานะ
                if (!foundDogs.Contains(dog))
                {
                    foundDogs.Add(dog);
                    lostDogs.Remove(dog);
                    
                    // 🌟 ให้น้องหมาเปลี่ยนสลับสถานะตัวเอง และปิดสิ่งกีดขวาง
                    dog.ChangeState(DogState.Found);
                }
            }
            else
            {
                // 🌟 สำคัญสุด: ถ้ายังไม่เคยหาเจอ ให้มันกลับไปใช้สถานะเริ่มต้นตามที่ตั้งไว้ในหน้า Inspector
                dog.ChangeState(dog.startState);
            }
        }
        UpdateDogUI();
    }
    
    Dog FindDogInteractionByName(string itemName)
    {
        Dog[] allDogs = GetAllDog();
        foreach (Dog item in allDogs)
        {
            if (item.name == itemName)
            {
                return item;
            }
        }
        return null; // คืนค่า null หากไม่พบไอเท็ม
    }
    
    public Vector3 lostDogsHint() {
        int r = Random.Range(0,lostDogs.Count);
        Debug.Log("Get lostDogs" + lostDogs[r].name);
        lostDogs[r].OnHint();
        return lostDogs[r].transform.position;
    }
    public void AddLostItem(Dog itemName)
    {
        lostDogs.Add(itemName);
        Debug.Log("เพิ่มไอเท็มที่หาย: " + itemName);
    }

    public void AddFoundDog(Dog dog)
    {
        foundDogs.Add(dog);
        lostDogs.Remove(dog);
        UpdateDogUI();

        // 🌟 บันทึกการเจอน้องหมาลงในเซฟไฟล์
        Gamemanager.Instance.RegisterFoundDogToSave(sceneObject.name, dog.name);

        // 🌟 A/B Testing Tracking & Mode 2
        Gamemanager.Instance.totalDogsFoundInSession++; // บวกยอดสะสม
        int requiredDogs = Gamemanager.Instance.dropEveryXDogs;
        // ป้องกันการหารด้วย 0
        if (requiredDogs > 0 && Gamemanager.Instance.totalDogsFoundInSession % requiredDogs == 0) 
        {
            if (Gamemanager.Instance.snackDropMode == Gamemanager.SnackDropMode.DropEvery15Dogs)
            {
                if (Gamemanager.Instance.snackDropPrefab != null)
                {
                    Instantiate(Gamemanager.Instance.snackDropPrefab, dog.transform.position, Quaternion.identity, this.transform);
                    Debug.Log($"[A/B Test Mode 2] 🐶 น้องหมาตัวที่ {Gamemanager.Instance.totalDogsFoundInSession} คายขนมออกมา!");
                }
            }
        }

        if (sceneObject.rewardSets.Length > 0) {
            for (int i = 0; i < sceneObject.rewardSets.Length; i++) {
                // 🌟 1. เปลี่ยนจาก >= เป็น == เพื่อให้มันแจกของแค่จังหวะที่ยอดครบพอดีเท่านั้น
                if (foundDogs.Count == sceneObject.rewardSets[i].AmontDogtoUnlockKeyItem)
                {
                    KeyItem keyItem = sceneObject.rewardSets[i].KeyItemInThisScene;

                    // 🌟 2. ดักเช็คก่อนว่าในกระเป๋ามีไอเทมชิ้นนี้หรือยัง? (กันเด้งซ้ำตอนโหลดเซฟ)
                    if (!Gamemanager.Instance.IsHasKey(keyItem))
                    {
                        // แอดเข้ากระเป๋าก่อน แล้วค่อยสั่งโชว์ PopUp
                        Gamemanager.Instance.AddKeyItem(keyItem);
                        Gamemanager.Instance.uiIngame.panelPopUpManager.ShowPopUpGotItem(keyItem);
                    }
                }
            }
            
        }
        if (lostDogs.Count == 0) {
            Debug.Log("Found All");
            Gamemanager.Instance.dialogueUIManager.OnShowDialog("dialog_found_all");
            
            // 🌟 ปลดล็อคด่านและบันทึกลงไป (ในที่นี้บันทึกชื่อฉากว่าสำเร็จแล้ว)
            Gamemanager.Instance.ClearScene(sceneObject.name);
        }
        Debug.Log("พบไอเท็ม: " + dog);
    }

    public bool IsDogsLost(Dog itemName)
    {
        return lostDogs.Contains(itemName);
    }

    public bool IsDogFound(Dog itemName)
    {
        return foundDogs.Contains(itemName);
    }

    // 🌟 แค่เติม [ContextMenu] ไว้บนหัวฟังก์ชัน Unity ก็จะเอาไปใส่ในเมนู 3 จุดให้ทันที!
    [ContextMenu("🔍 ค้นหาภาพ SCN อัตโนมัติ")]
    public void FindSCNFromMenu()
    {
        targetSpriteRenderer = FindOrCreateSCNRenderer();

        // 3. ถ้าหาเจอ ให้จับยัดใส่ตัวแปรและบังคับ Save ค่า
        if (targetSpriteRenderer != null)
        {
            // 🌟 ปลดล็อกคอมเมนต์บรรทัดล่างนี้ ถ้าคุณใส่ฟังก์ชันคำนวณซูมอัตโนมัติไว้แล้ว
             CalculateAutoZoom(); 

#if UNITY_EDITOR
            // บังคับให้ Unity จำค่าที่เปลี่ยนแปลงไป (จะได้ไม่หายตอนกด Play)
            UnityEditor.Undo.RecordObject(this, "Find SCN SpriteRenderer");
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"✅ ค้นหาสำเร็จ: ติดตั้งภาพฉากหลัง '{targetSpriteRenderer.gameObject.name}' ลงในสคริปต์แล้ว!");
#endif
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบ SCN! ลองเช็คดูว่ามี GameObject ที่ชื่อมีคำว่า 'SCN' ไหมครับ");
        }
    }
}