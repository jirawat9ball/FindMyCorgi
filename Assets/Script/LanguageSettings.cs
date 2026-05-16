using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro; // 🌟 เพิ่มสำหรับการใช้ TextMeshPro

public class LanguageSettings : MonoBehaviour
{
    // ==========================================
    // ⚙️ SINGLETON
    // ==========================================
    public static LanguageSettings Instance { get; private set; }
    public enum Language { English, Spanish, French, German } // Add your desired languages

    public Language currentLanguage = Language.English;
    public string languageFilePrefix = "lang_"; // Prefix for language files (e.g., lang_en.json)

    // 🌟 Event ที่จะถูกเรียกเมื่อมีการเปลี่ยนภาษา เพื่อให้ UI ทั้งหมดอัปเดตตัวเองอัตโนมัติ
    public event System.Action OnLanguageChanged;

    private Dictionary<string, string> languageData = new Dictionary<string, string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 🌟 โหลดภาษาจากไฟล์เซฟที่เพิ่งเพิ่มไปใน SaveData ถ้ามี
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            currentLanguage = (Language)Gamemanager.Instance.currentSaveData.gameSettings.languageIndex;
        }

        LoadLanguage(currentLanguage);
    }

    public void SetLanguage(Language language)
    {
        currentLanguage = language;
        LoadLanguage(language);
        
        // 🌟 ถ้าเชื่อมกับเซฟ ก็บันทึกข้อมูลเดี๋ยวนั้นเลย
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            Gamemanager.Instance.currentSaveData.gameSettings.languageIndex = (int)language;
            Gamemanager.Instance.AutoSaveProgress();
        }
    }

    private void LoadLanguage(Language language)
    {
        string languageCode = GetLanguageCode(language);
        string filePath = Path.Combine(Application.streamingAssetsPath, languageFilePrefix + languageCode + ".json");

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            LanguageData loadedData = JsonUtility.FromJson<LanguageData>(jsonData);

            // 🌟 แปลง List เข้าสู่ Dictionary ให้ค้นหาคำได้ไวขึ้น
            languageData.Clear();
            if (loadedData != null && loadedData.entries != null)
            {
                foreach (var item in loadedData.entries)
                {
                    if (!languageData.ContainsKey(item.key))
                    {
                        languageData.Add(item.key, item.value);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Language file not found: " + filePath + "\n🌟 กรุณาสร้างโฟลเดอร์ StreamingAssets และใส่ไฟล์ภาษาเข้าไปครับ");
        }

        // 🌟 สั่งให้ UI ทั้งโปรเจกต์ที่ซิงค์เอาไว้ อัปเดตข้อความเดี๋ยวนี้เลย
        OnLanguageChanged?.Invoke();
    }

    public string GetText(string key)
    {
        if (languageData.ContainsKey(key))
        {
            return languageData[key];
        }
        else
        {
            Debug.LogWarning("Language key not found: " + key);
            return key; // Return the key itself as a fallback.
        }
    }

    private string GetLanguageCode(Language language)
    {
        switch (language)
        {
            case Language.English: return "en";
            case Language.Spanish: return "es";
            case Language.French: return "fr";
            case Language.German: return "de";
            // Add more language codes as needed.
            default: return "en"; // Default to English.
        }
    }
}

// Helper class for JSON deserialization
[System.Serializable]
public class LanguageEntry
{
    public string key;
    public string value;
}

[System.Serializable]
public class LanguageData
{
    public List<LanguageEntry> entries = new List<LanguageEntry>();
}

// ==========================================
// 🌟 สคริปต์สำหรับนำไปใส่ใน Text (UI) เพื่อเปลี่ยนภาษาอัตโนมัติ
// ==========================================
public class UITextLocalizer : MonoBehaviour
{
    private TextMeshProUGUI myText;
    
    [Header("กุญแจสำหรับจำแนกคำ (Key จากไฟล์ Json)")]
    public string textKey = "greeting";

    void Awake()
    {
        myText = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        UpdateText();
    }

    void OnEnable()
    {
        // ลงทะเบียนรอรับคำสั่งเมื่อภาษาถูกเปลี่ยน
        if (LanguageSettings.Instance != null)
        {
            LanguageSettings.Instance.OnLanguageChanged += UpdateText;
        }
        UpdateText();
    }

    void OnDisable()
    {
        // ถอนการลงทะเบียนเมื่อ Object ถูกปิดตาย ป้องกัน Error Memory leak
        if (LanguageSettings.Instance != null)
        {
            LanguageSettings.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    public void UpdateText()
    {
        if (LanguageSettings.Instance != null && myText != null && !string.IsNullOrEmpty(textKey))
        {
            myText.text = LanguageSettings.Instance.GetText(textKey);
        }
    }
}