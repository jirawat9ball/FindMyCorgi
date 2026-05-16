using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveManager>();
                if (_instance != null)
                {
                    _instance.Init();
                }
            }
            return _instance;
        }
    }

    // ที่อยู่โฟลเดอร์สำหรับเซฟ และชื่อไฟล์หลัก
    private string saveDirectoryPath;
    private string mainSaveFilePath;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        if (string.IsNullOrEmpty(saveDirectoryPath))
        {
            // กำหนด Path ที่ปลอดภัยที่สุดในแต่ละแพลตฟอร์ม
            saveDirectoryPath = Application.persistentDataPath;
            mainSaveFilePath = saveDirectoryPath + "/gamesave.json";
        }
    }

    // 🌟 ฟังก์ชัน: บันทึกเกม (ข้อมูลหลัก)
    public void SaveGame(SaveData dataToSave)
    {
        string json = JsonUtility.ToJson(dataToSave, true);
        File.WriteAllText(mainSaveFilePath, json);
        Debug.Log("💾 บันทึกเกมหลักสำเร็จ! ที่อยู่ไฟล์: " + mainSaveFilePath);
    }

    // 🌟 ฟังก์ชัน: บันทึกข้อมูลของแต่ละด่านแยกไฟล์กัน (เพื่อให้รองรับการเพิ่มด่านในอนาคต)
    public void SaveSceneGame(SceneSaveData sceneData)
    {
        string path = saveDirectoryPath + "/save_" + sceneData.sceneID + ".json";
        string json = JsonUtility.ToJson(sceneData, true);
        File.WriteAllText(path, json);
    }

    // 🌟 ฟังก์ชัน: โหลดเกม (ข้อมูลหลัก)
    public SaveData LoadGame()
    {
        if (File.Exists(mainSaveFilePath))
        {
            string json = File.ReadAllText(mainSaveFilePath);
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
            return loadedData;
        }
        return new SaveData(); 
    }

    // 🌟 ฟังก์ชัน: โหลดข้อมูลแต่ละด่าน
    public SceneSaveData LoadSceneGame(string sceneID)
    {
        string path = saveDirectoryPath + "/save_" + sceneID + ".json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SceneSaveData>(json);
        }
        
        // ถ้าไม่เคยประวัติการเซฟด่านนี้ ให้สร้างใหม่ พร้อมเซ็ตค่าเริ่มต้น (ด่าน 1 ปลดล็อคไว้ให้)
        bool isDefaultUnlocked = (sceneID == "scene-tibet-1" || sceneID == "scene_jordan-1");
        return new SceneSaveData(sceneID, isDefaultUnlocked);
    }

    // 🌟 ฟังก์ชัน: โหลดเกมทับของเดิม (เพื่อรักษาเป้าในหน้าต่าง Inspector ไม่ให้กระพริบหาย)
    public void LoadGameOverwrite(SaveData targetData)
    {
        if (File.Exists(mainSaveFilePath))
        {
            string json = File.ReadAllText(mainSaveFilePath);
            JsonUtility.FromJsonOverwrite(json, targetData);

            // 🌟 ปริ้นต์ข้อมูลเซฟทั้งหมดออกทาง Console ตามที่คุณขอครับ
            Debug.Log($"📂 โหลดเกมสำเร็จ! ข้อมูลในเซฟไฟล์:\n{JsonUtility.ToJson(targetData, true)}");
        }
        else
        {
            Debug.LogWarning("⚠️ ไม่พบไฟล์เซฟ ใช้ข้อมูลตั้งต้น");
        }
    }

    // ฟังก์ชันลบเซฟ (เอาไว้ทำปุ่ม New Game)
    public void DeleteSave()
    {
        // ลบไฟล์หลัก
        if (File.Exists(mainSaveFilePath))
        {
            File.Delete(mainSaveFilePath);
        }
        
        // ลบไฟล์เซฟของแต่ละด่านทั้งหมด
        if (!string.IsNullOrEmpty(saveDirectoryPath) && Directory.Exists(saveDirectoryPath))
        {
            string[] saveFiles = Directory.GetFiles(saveDirectoryPath, "save_*.json");
            foreach (string file in saveFiles)
            {
                File.Delete(file);
            }
        }

        Debug.Log("🗑️ ลบไฟล์เซฟเกมทั้งหมดเรียบร้อยแล้ว");
    }

#if UNITY_EDITOR
    // ==========================================
    // 🛠️ ส่วนสำหรับเรียกใช้งานบนหน้าต่าง Unity Editor
    // ==========================================
    
    [MenuItem("CorgiTool/ลบเซฟเกมทั้งหมด (Clear Save)")]
    public static void EditorClearSave()
    {
        string saveDir = Application.persistentDataPath;
        string mainSave = saveDir + "/gamesave.json";
        
        if (File.Exists(mainSave))
        {
            File.Delete(mainSave);
        }

        if (Directory.Exists(saveDir))
        {
            string[] saveFiles = Directory.GetFiles(saveDir, "save_*.json");
            foreach (string file in saveFiles)
            {
                File.Delete(file);
            }
        }

        // ถ้าระหว่างที่เราลบ เรากำลังกด Play ทดสอบเกมอยู่ด้วย ก็สั่งล้างค่าปัจจุบันด้วย
        if (Application.isPlaying && Gamemanager.Instance != null)
        {
            Gamemanager.Instance.currentSaveData = new SaveData();
            Gamemanager.Instance.loadedScenesData.Clear();
            Gamemanager.Instance.Collectable.Clear();
        }

        Debug.Log("🗑️ [CorgiTool] ล้างไฟล์เซฟของเกมในเครื่องทิ้งทั้งหมดแล้ว!");
    }

    [MenuItem("CorgiTool/เปิดโฟลเดอร์เซฟ (Open Save Folder)")]
    public static void EditorOpenSaveFolder()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
#endif
}