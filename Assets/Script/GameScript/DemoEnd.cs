using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoEnd : MonoBehaviour
{
    public static DemoEnd Instance { get; private set; }

    [Header("Social Links")]
    public string facebookUrl = "https://www.facebook.com/";
    public string instagramUrl = "https://www.instagram.com/";
    public string discordUrl = "https://discord.gg/";
    public string wishlistUrl = "https://store.steampowered.com/app/yourgame";

    [Header("Demo Settings")]
    [Tooltip("รายชื่อด่านที่ต้องเล่นให้ผ่านเพื่อจบ Demo (ถ้าตรงกับชื่อในลิสต์นี้ถึงจะเอามาคิด)")]
    public List<string> demoScenes = new List<string> { "scene_tibet-1", "scene_jordan-1", "scene_jordan-2" };

    [Tooltip("ไอเทมที่จะแจกตอนจบ Demo (ลากไฟล์ ItemDemo มาใส่ช่องนี้ได้เลย)")]
    public KeyItem targetDemoItem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsDemoEnd()
    {
        bool allDemoCleared = true;
        if (Gamemanager.Instance.sceneObjects != null && Gamemanager.Instance.sceneObjects.Length > 0)
        {
            foreach (SceneObject s in Gamemanager.Instance.sceneObjects)
            {
                if (s != null)
                {
                    bool isRequiredScene = false;
                    foreach (string demoName in demoScenes)
                    {
                        if (s.name.Contains(demoName) || demoName.Contains(s.name)) 
                        {
                            isRequiredScene = true;
                            break;
                        }
                    }

                    if (isRequiredScene)
                    {
                        SceneSaveData data = Gamemanager.Instance.GetSceneSaveData(s.name);
                        if (data == null || !data.isCleared)
                        {
                            Debug.Log($"DemoEnd: ❌ Scene {s.name} ยังไม่ผ่าน (จำเป็นสำหรับ Demo)");
                            allDemoCleared = false;
                        }
                        else
                        {
                            Debug.Log($"DemoEnd: ✅ Scene {s.name} ผ่านแล้ว!");
                        }
                    }
                }
            }
        }
        else
        {
            allDemoCleared = false;
        }
        return allDemoCleared;
    }

    public void ShowDemoEndPopup()
    {
        Debug.Log("DemoEnd: 🔵 เริ่มทำงาน ShowDemoEndPopup()");
        StartCoroutine(DemoEndRoutine());
    }

    private IEnumerator DemoEndRoutine()
    {
        // 1. ใช้ KeyItem จากช่องที่ลากใส่ไว้โดยตรง (แก้ปัญหาหาใน Database ไม่เจอ)
        KeyItem demoItem = targetDemoItem;
        
        // ถ้าบังเอิญลืมลากใส่ ค่อยไปควานหาใน Database เป็นแผนสำรอง
        if (demoItem == null && Gamemanager.Instance.allKeyItemsDatabase != null)
        {
            foreach (KeyItem item in Gamemanager.Instance.allKeyItemsDatabase)
            {
                if (item != null && item.name == "ItemDemo")
                {
                    demoItem = item;
                    break;
                }
            }
        }

        // 2. เด้ง Popup ของรางวัล
        if (demoItem != null)
        {
            UIManager.Instance.ShowPopUpGotItem(demoItem);

            // 3. รอคิวของ Dialog อื่นๆ จบ และเด้ง Popup GotItem ขึ้นมา
            if (Gamemanager.Instance.uiIngame != null && Gamemanager.Instance.uiIngame.panelPopUpManager != null)
            {
                var gotItemObj = Gamemanager.Instance.uiIngame.panelPopUpManager.GotItem.gameObject;
                var gotItemScript = Gamemanager.Instance.uiIngame.panelPopUpManager.gotItem;

                Debug.Log("DemoEnd: ⏳ รอให้ Popup ItemDemo เปิดขึ้นมา (อาจจะรอคิว Dialog)");
                // รอจนกว่า Popup จะถูกเปิดขึ้นมาและเป็น ItemDemo
                yield return new WaitUntil(() => gotItemObj.activeSelf == true && gotItemScript != null && gotItemScript.currentItem == demoItem);
                
                Debug.Log("DemoEnd: ⏳ Popup เปิดแล้ว รอให้ผู้เล่นกดปิด");
                // รอจนกว่าผู้เล่นจะกดปิด Popup
                yield return new WaitUntil(() => gotItemObj.activeSelf == false);
            }
        }
        else
        {
            Debug.LogWarning("DemoEnd: 🔴 ไม่พบ KeyItem ชื่อ ItemDemo (ใช้คลิกแทนการรอ Popup)");
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }

        // 5. 🌟 รัน Transition ของเกม (มืดลง -> เปิดหน้า Thank You -> สว่างขึ้น)
        if (LoadSceneManager.Instance != null)
        {
            Debug.Log("DemoEnd: ⏳ กำลังรัน Transition จบเกม และ Unload Scene...");
            LoadSceneManager.Instance.UnloadCurrentScene(() => 
            {
                // 6. โชว์หน้า Thank You ตอนที่จอมืดสนิท
                Debug.Log("DemoEnd: 🎉 เปิดหน้า Thank You");
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowThankYouPanel();
                }
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBGSoundMainMenu();
                }
                
                // คืนค่า Gamemanager กลับโหมดปกติ
                Gamemanager.Instance.SetStateMenu();
            });
            Debug.Log("DemoEnd: ☀️ เรียกคำสั่ง Unload เรียบร้อยแล้ว (กำลังทำ Transition)!");
        }
        else
        {
            // แผนสำรอง เผื่อหา LoadSceneManager ไม่เจอ
            Debug.Log("DemoEnd: 🎉 เปิดหน้า Thank You (แบบไม่มี Transition)");
            if (UIManager.Instance != null) UIManager.Instance.ShowThankYouPanel();
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBGSoundMainMenu();
        }
    }

    public void OpenFacebookLink()
    {
        if (!string.IsNullOrEmpty(facebookUrl))
        {
            Application.OpenURL(facebookUrl);
        }
    }

    public void OpenInstagramLink()
    {
        if (!string.IsNullOrEmpty(instagramUrl))
        {
            Application.OpenURL(instagramUrl);
        }
    }

    public void OpenDiscordLink()
    {
        if (!string.IsNullOrEmpty(discordUrl))
        {
            Application.OpenURL(discordUrl);
        }
    }

    public void OpenWishlistLink()
    {
        if (!string.IsNullOrEmpty(wishlistUrl))
        {
            Application.OpenURL(wishlistUrl);
        }
    }
}
