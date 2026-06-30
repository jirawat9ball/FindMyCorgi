using System.Collections;
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

                // กวาดหาปุ่มทุกอันที่อยู่ใน Country นี้
                Button[] allBtns = uIMapScenes[i].GetComponentsInChildren<Button>(true);
                foreach (Button btn in allBtns) btn.interactable = isCountryUnlocked;

                // กวาดหารูปภาพ (Pin, พื้นหลัง, ฯลฯ) ทุกชิ้นมาเปลี่ยนสี
                Image[] allImgs = uIMapScenes[i].GetComponentsInChildren<Image>(true);
                foreach (Image img in allImgs) img.color = isCountryUnlocked ? Color.white : Color.gray;

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