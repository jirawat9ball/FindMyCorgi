using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateGame { 
    menu, gameplay
}
public class Gamemanager : MonoBehaviour
{
    // ==========================================
    // ⚙️ SINGLETON & CORE
    // ==========================================
    #region Core
    public static Gamemanager Instance { get; private set; } // Instance ของ Singleton
    #endregion

    // ==========================================
    // 🖥️ UI & STATE REFERENCES
    // ==========================================
    #region UI & State
    public StateGame stateGame;
    public CameraPan cameraPan;
    public CursorHandle cursorHandle;

    // 🌟 Backward-compatible shortcuts ที่ชี้ไปที่ UIManager ตัวจริง
    // เพื่อให้สคริปอื่นๆ ที่เคยเรียกใช้ Gamemanager.Instance.dialogueUIManager ยังใช้ได้ปกติโดยไม่พัง
    public DialogueUIManager dialogueUIManager => UIManager.Instance != null ? UIManager.Instance.dialogueUIManager : null;
    public UIingame uiIngame => UIManager.Instance != null ? UIManager.Instance.uiIngame : null;
    #endregion

    // ==========================================
    // 🗺️ SCENE & DATA REFERENCES
    // ==========================================
    #region Scene & Data
    public ZoneHandle currentZone;
    public SceneObject sceneObject;
    public SceneObject[] sceneObjects;
    
    [SerializeField]
    public SaveData currentSaveData = new SaveData(); // 🌟 กำหนดค่าเริ่มต้นไว้เลย จะได้กางดูใน Inspector ได้ตลอด

    [Header("ข้อมูลเซฟของแต่ละด่านที่โหลดมา")]
    public List<SceneSaveData> loadedScenesData = new List<SceneSaveData>();
    #endregion

    // ==========================================
    // 🎒 INVENTORY
    // ==========================================
    #region Inventory
    public int Snack;
    public List<KeyItem> Collectable = new List<KeyItem>();

    [Header("Item Database")]
    public KeyItem[] allKeyItemsDatabase; // 🌟 ใส่ไอเทมทุกชิ้นในเกมไว้ในนี้
    #endregion

    // ==========================================
    // 🧪 A/B TESTING ENGINE
    // ==========================================
    #region A/B Testing Data
    public enum SnackDropMode { None, DropOnSceneLoad, DropEvery15Dogs }
    
    [Header("A/B Testing: Snack Drop System")]
    public SnackDropMode snackDropMode = SnackDropMode.DropOnSceneLoad;
    [Range(0, 100)]
    [Tooltip("โอกาสดรอปขนม (เปอร์เซ็นต์) สำหรับ Mode 1")]
    public int dropChancePercent = 10;
    
    [Tooltip("จำนวนหมาที่ต้องหาเจอเพื่อดรอปขนม สำหรับ Mode 2")]
    public int dropEveryXDogs = 15;

    [Tooltip("ใส่ Prefab ขนมหมาที่มีสคริปต์ SnackDropObject ให้พร้อม")]
    public GameObject snackDropPrefab;
    
    [HideInInspector]
    public int totalDogsFoundInSession = 0; // ยอดสะสมหมาถูกหาเจอในรอบเล่น
    #endregion

    // ==========================================
    // 🔄 UNITY LIFECYCLE
    // ==========================================
    #region Lifecycle
    private void Awake()
    {
        // ตรวจสอบว่ามี Instance อื่นอยู่หรือไม่
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ทำให้ Instance ยังคงอยู่เมื่อเปลี่ยน Scene
        }
        else
        {
            Destroy(gameObject); // ทำลาย Instance ใหม่หากมี Instance อื่นอยู่แล้ว
            return; // 🌟 หยุดการทำงานของตัวก็อปปี้ผีดิบทันที! จะได้ไม่ไปโหลดเซฟซ้ำซ้อน
        }
        
        // 🌟 โหลดเซฟมาใส่ตัวแปรตรงๆ เพื่อบังคับให้ Unity Inspector รีเฟรชข้อมูลแบบ 100%
        currentSaveData = SaveManager.Instance.LoadGame();

        SyncSaveData(); // 🌟 รวบรวมข้อมูลเซฟกลับเข้าเกม

        cursorHandle = GetComponent<CursorHandle>();
        stateGame = StateGame.menu;
    }
    #endregion

    // ==========================================
    // 💾 SAVE SYSTEM PROCESSES
    // ==========================================
    #region Save System
    public void SyncSaveData()
    {
        Collectable.Clear();
        // 🌟 ดึงข้อมูลจำนวนครั้งคำใบ้ (Snack) ที่โหลดได้จากเซฟ
        Snack = currentSaveData.snackCount;
        if (uiIngame != null) uiIngame.UpdateSnack(Snack);

        // 🌟 ดึงข้อมูล KeyItem ที่เคยเก็บไว้ใส่กลับเข้ากระเป๋า โดยเทียบจาก Database
        if (allKeyItemsDatabase != null)
        {
            foreach (KeyItem item in allKeyItemsDatabase)
            {
                if (item != null && currentSaveData.collectedKeyItemIDs.Contains(item.name))
                {
                    if (!Collectable.Contains(item))
                        Collectable.Add(item);
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ อย่าลืมลากไฟล์ KeyItem ทั้งหมดมาใส่ในช่อง 'allKeyItemsDatabase' ใน Gamemanager ด้วยนะครับ!");
        }
    }

    public void AutoSaveProgress()
    {
        // 1. อัปเดตข้อมูลฉากล่าสุด (ดึงชื่อมาจาก LoadSceneManager ก็ได้)
        // currentSaveData.lastSceneName = LoadSceneManager.Instance.CurrentScene;

        // 2. สั่ง SaveManager เอาไปเขียนไฟล์หลัก
        SaveManager.Instance.SaveGame(currentSaveData);

        // 3. เซฟข้อมูลด่านทั้งหมดที่ถูกโหลดมาแล้วด้วย
        foreach (var sceneData in loadedScenesData)
        {
            SaveManager.Instance.SaveSceneGame(sceneData);
        }
    }

    public SceneSaveData GetSceneSaveData(string sceneID)
    {
        SceneSaveData data = loadedScenesData.Find(s => s.sceneID == sceneID);
        if (data == null)
        {
            // ถ้าไม่เคยโหลดเข้า Memory มาก่อน ให้ลองดึงจากไฟล์เซฟ
            data = SaveManager.Instance.LoadSceneGame(sceneID);
            loadedScenesData.Add(data);
        }
        return data;
    }

    public void RegisterFoundDogToSave(string sceneID, string dogID)
    {
        SceneSaveData data = GetSceneSaveData(sceneID);
        if (!data.foundDogIDs.Contains(dogID))
        {
            data.foundDogIDs.Add(dogID);
            AutoSaveProgress(); // หาเจอ 1 ตัว เซฟทันที (กันผู้เล่นเกมหลุด)
        }
    }

    public void UnlockScene(string sceneID)
    {
        // 🌟 ปลดล็อคด่านใหม่และเซฟ
        SceneSaveData data = GetSceneSaveData(sceneID);
        if (!data.isUnlocked)
        {
            data.isUnlocked = true;
            AutoSaveProgress();
            Debug.Log("🔓 ปลดล็อคด่านแล้ว: " + sceneID);
        }
    }

    public void ClearScene(string sceneID)
    {
        SceneSaveData data = GetSceneSaveData(sceneID);
        if (!data.isCleared)
        {
            data.isCleared = true;
            AutoSaveProgress();
            Debug.Log("🏆 เคลียร์ด่านแล้ว: " + sceneID);
        }
    }

    public bool IsSceneUnlocked(string sceneID)
    {
        SceneSaveData data = GetSceneSaveData(sceneID);
        return data != null && data.isUnlocked;
    }

    public bool IsDogFoundInSave(string sceneID, string dogID)
    {
        SceneSaveData data = GetSceneSaveData(sceneID);
        return data != null && data.foundDogIDs.Contains(dogID);
    }

    public void ResetGame()
    {
        // 🌟 1. ล้างของในกระเป๋า
        Collectable.Clear();
        
        // 🌟 2. ลบทิ้งทั้งไฟล์เซฟหลักและไฟล์เซฟย่อยทุกด่าน
        SaveManager.Instance.DeleteSave();

        // 🌟 3. สร้าง SaveData ใหม่เอี่ยมเพื่อล้างค่า และล้างข้อมูลด่านใน Memory
        currentSaveData = new SaveData(); 
        loadedScenesData.Clear(); 
        totalDogsFoundInSession = 0; // รีเซ็ตยอดหมาในรอบนี้ด้วย
        
        // 🌟 3. บันทึกลงเครื่องทันที
        AutoSaveProgress(); 
        SyncSaveData(); // อัปเดตข้อมูลว่างเปล่ากลับเข้าระบบ
        
        Debug.Log("🗑️ รีเซ็ตเซฟข้อมูลเกมทั้งหมดเรียบร้อยแล้ว!");
    }
    #endregion

    // ==========================================
    // 🎮 GAME STATE & SCENE FLOW
    // ==========================================
    #region Game State & Flow
    public void SetStateMenu() {
        if (sceneObject != null) {
            stateGame = StateGame.menu;
        }
    }
    public void SetStateGamePlay()
    {
        if (sceneObject != null)
        {
            stateGame = StateGame.gameplay;
            if (UIManager.Instance != null) UIManager.Instance.ShowGamePlay();
        }
    }
    public bool isStateGamePlay() { 
        return stateGame == StateGame.gameplay;
    }
    public void GoHome()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowHome();
    }
    public void GoMapMenu()
    {
        if (UIManager.Instance != null) UIManager.Instance.ShowMapMenu();
    }
    public void AddSceneGame(string SceneName) {
        LoadSceneManager.Instance.AddNewScene(SceneName, () => {
            SetStateGamePlay();
        });
    }
    public void BackScene() {
        if (currentZone.currentScene.sceneObject.backScene == null)
        {
            StartCoroutine(EnumGotoScene(() => {
                LoadSceneManager.Instance.UnloadCurrentScene(GoMapMenu);
                SoundManager.Instance.PlayBGSoundMainMenu();
            }));
        }
        else {
            StartCoroutine(EnumGotoScene(() => {
                currentZone.BackScene();
            }));
        }
    }
    IEnumerator EnumGotoScene(System.Action onMidpointComplete)
    {

        yield return StartCoroutine(LoadSceneManager.Instance.LocalTransitionRoutine(() =>
        {
            onMidpointComplete?.Invoke();
        }));

    }
    #endregion

    // ==========================================
    // 🎒 ITEM & INVENTORY ENGINE
    // ==========================================
    #region Item Engine
    public void UseHintItem() {
        if (Snack <= 0) {
            Debug.Log("not enough Hint Item");
            return;
        }
        Snack--;
        
        // 🌟 เซฟจำนวนที่ลดลงทันที
        currentSaveData.snackCount = Snack;
        if (uiIngame != null) uiIngame.UpdateSnack(Snack);
        AutoSaveProgress();

        Debug.Log("UseHintItem");
        Vector3 target = currentZone.currentScene.lostDogsHint();
        cameraPan.TriggerLeap(target);
    }

    // 🌟 ระบบเพิ่มจำนวนคำใบ้หลังจากดูโฆษณาจบ
    public void AddSnackFromAd(int amount)
    {
        Snack += amount;
        currentSaveData.snackCount = Snack;
        if (uiIngame != null) uiIngame.UpdateSnack(Snack);
        AutoSaveProgress();
        Debug.Log($"🎉 เพิ่มคำใบ้ (Snack) ฟรี {amount} ชิ้น รวมจำนวนที่มี: {Snack}");
    }
    public void AddKeyItem(KeyItem keyItem) {
        if (Collectable.Contains(keyItem)) {
            Debug.Log("You has this "+ keyItem.name +" key item");
            return;
        }
        Collectable.Add(keyItem);

        // 🌟 บันทึก KeyItem ลงไฟล์เซฟ
        if (!currentSaveData.collectedKeyItemIDs.Contains(keyItem.name))
        {
            currentSaveData.collectedKeyItemIDs.Add(keyItem.name);
            AutoSaveProgress();
        }

        Debug.Log("Add Key Item " + keyItem.name + " to Collectable");
    }
    public bool IsHasKey(KeyItem keyItem) {
        return Collectable.Contains(keyItem);
    }
    #endregion
}
