using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSettingsData
{
    [Header("Audio Settings")]
    public float masterVolume = 1.0f;
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;

    [Header("Display Settings")]
    public int screenResolutionIndex = 0; // 0 = Fullscreen, 1 = 3840x2160, 2 = 1920x1080
    public bool isFullscreen = true;

    [Header("Language Settings")]
    public int languageIndex = 0; // 0 = English, 1 = Spanish, 2 = French, 3 = German
}

[System.Serializable]
public class SceneSaveData
{
    public float version = 1.0f; // 🌟 เวอร์ชั่นไฟล์เผื่ออัพเดทโครงสร้างในอนาคต
    public string sceneID;
    public bool isUnlocked;
    public bool isCleared;
    public List<string> foundDogIDs = new List<string>();

    public SceneSaveData(string id, bool unlocked)
    {
        sceneID = id;
        isUnlocked = unlocked;
    }
}

[System.Serializable] // 🌟 บังคับใส่บรรทัดนี้ เพื่อให้ Unity แปลงเป็นไฟล์ JSON ได้
public class SaveData
{
    public float version = 1.0f; // 🌟 เวอร์ชั่นไฟล์เผื่ออัพเดทโครงสร้างในอนาคต

    [Header("ด่านปัจุบัน")]
    public string lastSceneName;

    [Header("ไอเทมช่วยเหลือ")]
    public int snackCount = 0;

    [Header("ไอเทมที่เก็บแล้ว")]
    public List<string> collectedKeyItemIDs = new List<string>();

    [Header("กลไกที่ปลดล็อค (ถ้ามี)")]
    public List<string> interactedEnvIDs = new List<string>();

    [Header("การตั้งค่าเกม")]
    public GameSettingsData gameSettings = new GameSettingsData();
}