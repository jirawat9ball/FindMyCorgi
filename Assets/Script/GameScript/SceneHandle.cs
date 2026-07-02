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

    public Gradient sceneOverlayGradient;
    public Material multiplyMaterial;

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

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            InspectNextDog(1);
        }
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

        if (debugDogIndex >= allDogs.Length) debugDogIndex = 0;
        if (debugDogIndex < 0) debugDogIndex = allDogs.Length - 1;

        Dog targetDog = allDogs[debugDogIndex];
        if (targetDog != null)
        {
            Debug.Log($"🔍 [Editor Test] ตรวจสอบสุนัข ({debugDogIndex + 1}/{allDogs.Length}): {targetDog.name}");

            if (Gamemanager.Instance.cameraPan != null)
            {
                Gamemanager.Instance.cameraPan.TriggerLeap(targetDog.transform.position, false);
            }
        }
    }
#endif

    public void Setup()
    {
        lostDogs.Clear();
        foundDogs.Clear();

        FindSCNSpriteRenderer();

        Dog[] interactions = GetAllDog();
        for (int i = 0; i < interactions.Length; i++)
        {
            if (interactions[i] != null)
            {
                lostDogs.Add(interactions[i]);
            }
        }

        SoundManager.Instance.PlayBGSound(sceneObject.soundBG);

        if (Gamemanager.Instance.snackDropMode == Gamemanager.SnackDropMode.DropOnSceneLoad)
        {
            int randomRoll = Random.Range(0, 100);
            int requiredChance = Gamemanager.Instance.dropChancePercent;

            if (randomRoll < requiredChance)
            {
                if (lostDogs.Count > 0)
                {
                    Dog rDog = lostDogs[Random.Range(0, lostDogs.Count)];
                    if (Gamemanager.Instance.snackDropPrefab != null)
                    {
                        Vector3 spawnPos = rDog.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                        Instantiate(Gamemanager.Instance.snackDropPrefab, spawnPos, Quaternion.identity, this.transform);
                    }
                }
            }
        }

        CreateGradientOverlayOnTop();
    }

    public Dog[] GetAllDog()
    {
        return GetComponentsInChildren<Dog>(true);
    }

    private void FindSCNSpriteRenderer()
    {
        if (targetSpriteRenderer != null && !targetSpriteRenderer.gameObject.activeInHierarchy)
        {
            targetSpriteRenderer = null;
        }

        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = FindOrCreateSCNRenderer();
        }

        if (targetSpriteRenderer != null)
        {
            CalculateAutoZoom();
        }
    }

    private SpriteRenderer FindOrCreateSCNRenderer()
    {
        if (targetSpriteRenderer != null) return targetSpriteRenderer;

        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(false);
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

        Bounds bounds = targetSpriteRenderer.bounds;
        float screenAspect = Camera.main.aspect;
        float maxZoomHeight = bounds.size.y / 2f;
        float maxZoomWidth = bounds.size.x / (2f * screenAspect);
        float autoZoom = Mathf.Min(maxZoomHeight, maxZoomWidth);

        maxZoom = autoZoom;
        ToggleRangeZoom = autoZoom;
    }

    public void setZone(ZoneHandle _zoneHandle)
    {
        zoneHandle = _zoneHandle;
    }

    public void SetToGamemanager()
    {
        Gamemanager.Instance.currentZone.currentScene = this;
        Gamemanager.Instance.sceneObject = sceneObject;

        if (targetSpriteRenderer != null && !targetSpriteRenderer.gameObject.activeInHierarchy)
        {
            targetSpriteRenderer = null;
        }

        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = FindOrCreateSCNRenderer();

            if (targetSpriteRenderer == null)
            {
                SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(false);
                if (srs.Length > 0) targetSpriteRenderer = srs[0];
            }

            if (targetSpriteRenderer == null)
            {
                Debug.LogError($"⚠️ ห้ามลืม! GameObject ชื่อ '{gameObject.name}' ยังไม่ได้ใส่ SpriteRenderer (ภาพพื้นหลังฉาก) ครับ!");
                return;
            }
        }

        CameraPan cameraPan = Gamemanager.Instance.cameraPan;
        if (cameraPan == null)
        {
            cameraPan = FindObjectOfType<CameraPan>();
            Gamemanager.Instance.cameraPan = cameraPan;
        }

        if (cameraPan != null)
        {
            cameraPan.SetUpCamera(this);
            cameraPan.TriggerLeap(targetSpriteRenderer.bounds.center, true);
        }
        else
        {
            Debug.LogError("⚠️ หาสคริปต์ 'CameraPan' ไม่เจอในฉากเลยครับ ตรวจสอบกล้องด่วน!");
            return;
        }

        UpdateDogUI();
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

    void SetupScene()
    {
        Dog[] allDogs = GetAllDog();
        foreach (Dog dog in allDogs)
        {
            if (dog == null) continue;

            if (Gamemanager.Instance.IsDogFoundInSave(sceneObject.name, dog.name))
            {
                if (!foundDogs.Contains(dog))
                {
                    foundDogs.Add(dog);
                    lostDogs.Remove(dog);
                    dog.ChangeState(DogState.Found);
                }
            }
            else
            {
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
        return null;
    }

    public Vector3 lostDogsHint()
    {
        int r = Random.Range(0, lostDogs.Count);
        lostDogs[r].OnHint();
        return lostDogs[r].transform.position;
    }

    public void AddLostItem(Dog itemName)
    {
        lostDogs.Add(itemName);
    }

    public void AddFoundDog(Dog dog)
    {
        foundDogs.Add(dog);
        lostDogs.Remove(dog);
        UpdateDogUI();

        Gamemanager.Instance.RegisterFoundDogToSave(sceneObject.name, dog.name);

        Gamemanager.Instance.totalDogsFoundInSession++;
        int requiredDogs = Gamemanager.Instance.dropEveryXDogs;

        if (requiredDogs > 0 && Gamemanager.Instance.totalDogsFoundInSession % requiredDogs == 0)
        {
            if (Gamemanager.Instance.snackDropMode == Gamemanager.SnackDropMode.DropEvery15Dogs)
            {
                if (Gamemanager.Instance.snackDropPrefab != null)
                {
                    Instantiate(Gamemanager.Instance.snackDropPrefab, dog.transform.position, Quaternion.identity, this.transform);
                }
            }
        }

        if (sceneObject.rewardSets.Length > 0)
        {
            for (int i = 0; i < sceneObject.rewardSets.Length; i++)
            {
                if (foundDogs.Count == sceneObject.rewardSets[i].AmontDogtoUnlockKeyItem)
                {
                    KeyItem keyItem = sceneObject.rewardSets[i].KeyItemInThisScene;

                    if (!Gamemanager.Instance.IsHasKey(keyItem))
                    {
                        Gamemanager.Instance.AddKeyItem(keyItem);
                        Gamemanager.Instance.uiIngame.panelPopUpManager.ShowPopUpGotItem(keyItem);
                    }
                }
            }

        }
        if (lostDogs.Count == 0)
        {
            Gamemanager.Instance.dialogueUIManager.OnShowDialog("dialog_found_all");
            Gamemanager.Instance.ClearScene(sceneObject.name);
        }
    }

    public bool IsDogsLost(Dog itemName)
    {
        return lostDogs.Contains(itemName);
    }

    public bool IsDogFound(Dog itemName)
    {
        return foundDogs.Contains(itemName);
    }

    [ContextMenu("🔍 ค้นหาภาพ SCN อัตโนมัติ")]
    public void FindSCNFromMenu()
    {
        targetSpriteRenderer = null;
        targetSpriteRenderer = FindOrCreateSCNRenderer();

        if (targetSpriteRenderer != null)
        {
            CalculateAutoZoom();

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Find SCN SpriteRenderer");
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"✅ ค้นหาสำเร็จ: ติดตั้งภาพฉากหลัง '{targetSpriteRenderer.gameObject.name}' ลงในสคริปต์แล้ว!");
#endif
        }
    }


    private void CreateGradientOverlayOnTop()
    {
        Debug.Log("👉 เริ่มทำงาน: CreateGradientOverlayOnTop (สร้างฟิล์มให้ทุกฉากย่อย)");

        if (sceneOverlayGradient == null)
        {
            Debug.LogError("❌ สร้างฟิล์มไม่ได้: ยังไม่ได้ตั้งค่าสี Gradient ในหน้า Inspector!");
            return;
        }

        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        List<SpriteRenderer> allScnRenderers = new List<SpriteRenderer>();

        foreach (SpriteRenderer sr in allRenderers)
        {
            if (sr.gameObject.name.ToUpper().Contains("SCN"))
            {
                allScnRenderers.Add(sr);
            }
        }

        if (allScnRenderers.Count == 0)
        {
            Debug.LogError("❌ สร้างฟิล์มไม่ได้: ไม่พบภาพฉากหลังที่มีคำว่า 'SCN' เลยครับ");
            return;
        }

        Texture2D tex = new Texture2D(1, 100);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < 100; y++)
        {
            tex.SetPixel(0, y, sceneOverlayGradient.Evaluate(y / 100f));
        }
        tex.Apply();
        Sprite gradientSprite = Sprite.Create(tex, new Rect(0, 0, 1, 100), new Vector2(0.5f, 0.5f), 100f);

        foreach (SpriteRenderer scn in allScnRenderers)
        {
            // ดัก Error เผื่อมี Object ไหนตั้งชื่อ SCN แต่ยังไม่ได้ใส่รูป
            if (scn.sprite == null) continue;

            Transform oldFilm = scn.transform.Find("Scene_Gradient_Film");
            if (oldFilm != null)
            {
                Destroy(oldFilm.gameObject);
            }

            GameObject overlayObj = new GameObject("Scene_Gradient_Film");

            overlayObj.transform.SetParent(scn.transform);

            // 🌟 แก้ไขที่ 1: ดึงตำแหน่งศูนย์กลางจากเนื้อไฟล์ Sprite โดยตรง (หลีกเลี่ยง bounds.center ที่พังตอนซ่อน)
            overlayObj.transform.localPosition = scn.sprite.bounds.center;

            SpriteRenderer overlayRenderer = overlayObj.AddComponent<SpriteRenderer>();
            overlayRenderer.sprite = gradientSprite;

            if (multiplyMaterial != null)
            {
                overlayRenderer.material = multiplyMaterial;
            }

            overlayRenderer.sortingOrder = 4;

            // 🌟 แก้ไขที่ 2: คราวนี้นำขนาดของ "ไฟล์รูป" มาคำนวณแทน
            // ถึงด่านจะถูก SetActive(false) ปิดตาไว้ มันก็จะสเกลได้ถูกต้อง 100% ครับ
            Vector2 bgSpriteSize = scn.sprite.bounds.size;
            Vector2 overlaySpriteSize = overlayRenderer.sprite.bounds.size;

            overlayObj.transform.localScale = new Vector3(bgSpriteSize.x / overlaySpriteSize.x, bgSpriteSize.y / overlaySpriteSize.y, 1);
        }

        Debug.Log($"🎉 สร้างฟิล์มครอบคลุมด่านย่อยทั้งหมด {allScnRenderers.Count} ด่าน เรียบร้อยแล้ว!");
    }
}