using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMapManager : MonoBehaviour
{
    public GameObject[] uIMapScenes;
    public float delayPerObject = 1f;
    public float StartWait = 1f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private void OnEnable()
    {
        StartCoroutine(ActivateObjectsWithDelayRoutine());
    }

    private Dictionary<TMPro.TextMeshProUGUI, string> originalTextTMP = new Dictionary<TMPro.TextMeshProUGUI, string>();
    private Dictionary<Text, string> originalTextNormal = new Dictionary<Text, string>();

    public IEnumerator ActivateObjectsWithDelayRoutine()
    {
        foreach (GameObject go in uIMapScenes)
        {
            if (go != null) go.SetActive(false);
        }

        yield return new WaitForSeconds(StartWait);

        if (uIMapScenes == null || uIMapScenes.Length == 0) yield break;

        for (int i = 0; i < uIMapScenes.Length; i++)
        {
            if (uIMapScenes[i] != null)
            {
                uIMapScenes[i].SetActive(true);

                CountryManager cm = uIMapScenes[i].GetComponentInChildren<CountryManager>(true);
                int unlockCount = 0;

                UIMapScene mapScene = null;
                if (cm != null && cm.uIMapScene != null)
                {
                    mapScene = cm.uIMapScene;
                }
                else
                {
                    mapScene = uIMapScenes[i].GetComponentInChildren<UIMapScene>(true);
                }

                // 🌟 ขั้นที่ 1: นับจำนวนด่านย่อยที่ผ่านแล้ว เพื่อหาค่า unlockCount ก่อน
                if (mapScene != null && mapScene.parrent != null)
                {
                    foreach (GameObject mapNode in mapScene.parrent)
                    {
                        if (mapNode != null && Gamemanager.Instance.IsSceneUnlocked(mapNode.name))
                        {
                            unlockCount++;
                        }
                    }
                }

                // 🌟 ขั้นที่ 2: สั่งกวาด "ด่านใหญ่ (Country)" ให้ย้อมสีและล็อกปุ่มแบบถอนรากถอนโคน
                bool isCountryUnlocked = unlockCount > 0;

                // 🌟 ระบบ Tutorial บังคับให้เข้า Tibet ในการเล่นครั้งแรกสุด
                if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
                {
                    if (!Gamemanager.Instance.currentSaveData.hasCompletedTutorial)
                    {
                        bool isTibet = uIMapScenes[i].name.ToLower().Contains("tibet");
                        if (isTibet)
                        {
                            isCountryUnlocked = true; // ปลดล็อคแค่ทิเบต
                        }
                        else
                        {
                            isCountryUnlocked = false; // ล็อกประเทศอื่นให้หมด
                        }
                    }
                }

                // กวาดหาปุ่มทุกอันที่อยู่ใน Country นี้
                Button[] allBtns = uIMapScenes[i].GetComponentsInChildren<Button>(true);
                foreach (Button btn in allBtns)
                {
                    btn.interactable = isCountryUnlocked;

                    // 🌟 แก้ปัญหาความโปร่งใสที่เกิดจากระบบ Button ของ Unity
                    // โดยปกติปุ่มที่กดไม่ได้ (Disabled) จะถูกลดค่า Alpha ลงครึ่งนึง (0.5)
                    // เราต้องไปบังคับให้สีตอนปุ่มพัง (Disabled Color) เป็นแบบทึบ 100%
                    ColorBlock cb = btn.colors;
                    Color dc = cb.disabledColor;
                    dc.a = 1f; 
                    cb.disabledColor = dc;
                    btn.colors = cb;
                }

                // กวาดหารูปภาพ (Pin, พื้นหลัง, ฯลฯ) ทุกชิ้นมาเปลี่ยนสี
                Image[] allImgs = uIMapScenes[i].GetComponentsInChildren<Image>(true);
                foreach (Image img in allImgs)
                {
                    Color targetColor = isCountryUnlocked ? Color.white : Color.gray;
                    targetColor.a = 1f; // 🌟 บังคับตั้งค่าทึบ 100% (ลบความโปร่งใสทิ้งทั้งหมด)
                    img.color = targetColor;
                }

                // 🌟 ขั้นที่ 2.5: เปลี่ยนชื่อด่านของประเทศที่ล็อกอยู่ให้แสดงเป็น "???"
                // และถ้าปลดล็อกแล้ว ให้คืนค่าชื่อด่านเดิมกลับมา
                TMPro.TextMeshProUGUI[] allTextsTMP = uIMapScenes[i].GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (TMPro.TextMeshProUGUI txt in allTextsTMP)
                {
                    if (!originalTextTMP.ContainsKey(txt))
                        originalTextTMP[txt] = txt.text; // จดจำชื่อด่านเดิมไว้

                    txt.text = isCountryUnlocked ? originalTextTMP[txt] : "???";
                }

                Text[] allTextsNormal = uIMapScenes[i].GetComponentsInChildren<Text>(true);
                foreach (Text txt in allTextsNormal)
                {
                    if (!originalTextNormal.ContainsKey(txt))
                        originalTextNormal[txt] = txt.text; // จดจำชื่อด่านเดิมไว้

                    txt.text = isCountryUnlocked ? originalTextNormal[txt] : "???";
                }

                // 🌟 ขั้นที่ 3: สั่งลงสี "ด่านย่อย" ทับอีกรอบ! 
                // (เพราะขั้นที่ 2 อาจจะเผลอสาดสีโดนด่านย่อยไปด้วย เราเลยต้องมาเซ็ตด่านย่อยให้กลับมาตรงตามสถานะจริงของมัน)
                if (mapScene != null && mapScene.parrent != null)
                {
                    foreach (GameObject mapNode in mapScene.parrent)
                    {
                        if (mapNode != null)
                        {
                            bool isUnlocked = Gamemanager.Instance.IsSceneUnlocked(mapNode.name);
                            mapScene.SetNodeState(mapNode, isUnlocked);
                        }
                    }
                }

                if (cm != null)
                {
                    cm.SetupMapSequence(unlockCount);
                }

                yield return new WaitForSeconds(0.15f);
            }
        }
    }
}