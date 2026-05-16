using UnityEngine;

// ใช้เป็น static class ได้เลย เพราะเราไม่ต้องเอาไปแปะใน GameObject
public static class AutoManagerSpawner
{
    // 🌟 แอตทริบิวต์นี้คือพระเอก! มันจะสั่งให้ Unity รันฟังก์ชันนี้ "ก่อน" ที่ฉากจะโหลดขึ้นมา
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void SpawnManagersIfNeeded()
    {
        // 1. เช็คก่อนว่าใน Scene ปัจจุบันมี Gamemanager อยู่แล้วหรือยัง?
        // (เผื่อในกรณีที่คุณรันจากหน้า Main Menu ที่มีของพวกนี้อยู่แล้ว จะได้ไม่เสกซ้ำซ้อน)
        Gamemanager existingManager = Object.FindObjectOfType<Gamemanager>();

        // ถ้ายังไม่มี (แปลว่าเรากด Play เทสด่านย่อยโดยตรง)
        if (existingManager == null)
        {
            // 2. ไปโหลด Prefab ชื่อ "CoreManagers" จากโฟลเดอร์ Resources
            GameObject managerPrefab = Resources.Load<GameObject>("CoreManagers");

            if (managerPrefab != null)
            {
                // 3. เสกขึ้นมาในฉาก
                GameObject instance = Object.Instantiate(managerPrefab);
                instance.name = "[ Auto-Spawned CoreManagers ]";

                // ไม่ต้องสั่ง DontDestroyOnLoad ตรงนี้ เพราะใน Gamemanager.cs ของคุณ 
                // มีคำสั่ง DontDestroyOnLoad(gameObject); ในฟังก์ชัน Awake() เตรียมไว้ดีอยู่แล้วครับ

                Debug.Log("✅ ระบบเสก Core Managers อัตโนมัติสำหรับการทดสอบฉากนี้เรียบร้อยแล้วครับ!");
            }
            else
            {
                Debug.LogWarning("⚠️ ระบบ Auto-Spawn ทำงาน แต่หา Prefab ชื่อ 'CoreManagers' ในโฟลเดอร์ 'Resources' ไม่เจอครับ!");
            }
        }
    }
}