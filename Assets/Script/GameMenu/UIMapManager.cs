using System.Collections;
using UnityEngine;

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
        // 1. ปิดทุกด่านก่อน
        foreach (GameObject go in uIMapScenes)
        {
            if (go != null)
            {
                go.SetActive(false);
            }
        }

        yield return new WaitForSeconds(StartWait);

        if (uIMapScenes == null || uIMapScenes.Length == 0)
        {
            Debug.LogWarning("Objects to activate array is empty or null.");
            yield break;
        }

        // 2. ลูปตรวจสอบแต่ละ Set (Country)
        for (int i = 0; i < uIMapScenes.Length; i++)
        {
            if (uIMapScenes[i] != null)
            {
                CountryManager cm = uIMapScenes[i].GetComponentInChildren<CountryManager>(true);
                int unlockCount = 0;

                // 🌟 วิธีนับแบบใหม่ที่แม่นยำ 100%: 
                // เข้าไปนับจากตลับ Scene ย่อยๆ ข้างในเลย ว่าชื่อของด่านย่อยแต่ละด่าน มีอยู่ในไฟล์เซฟหรือไม่
                if (cm != null && cm.uIMapScene != null && cm.uIMapScene.parrent != null)
                {
                    foreach (GameObject mapNode in cm.uIMapScene.parrent)
                    {
                        if (mapNode != null && Gamemanager.Instance.IsSceneUnlocked(mapNode.name))
                        {
                            unlockCount++;
                        }
                    }
                }
                else
                {
                    // Fallback (สำรองกรณีไม่ได้ผูก CountryManager ไว้)
                    // 🌟 ปัจจุบันไฟล์เซฟแยกด่านกัน จึงจำเป็นต้องเช็คผ่านด่านที่อยู่ใน Memory (loadedScenesData) ไปก่อน
                    string setName = uIMapScenes[i].name.ToLower();
                    foreach (SceneSaveData sceneData in Gamemanager.Instance.loadedScenesData)
                    {
                        if (sceneData.isUnlocked && sceneData.sceneID.ToLower().Contains(setName))
                        {
                            unlockCount++;
                        }
                    }
                }

                // ถ้ามีด่านถูกปลดล็อคอย่างน้อย 1 ด่าน ให้เปิด Set นี้ขึ้นมา
                if (unlockCount > 0)
                {
                    uIMapScenes[i].SetActive(true); // เปิด CountryManager (Set)

                    // สั่งให้ CountryManager ภายในเซ็ตนี้ เปิดด่านย่อยตามจำนวนที่นับได้!
                    if (cm != null)
                    {
                        cm.SetupMapSequence(unlockCount);
                    }
                    
                    yield return new WaitForSeconds(0.15f); // รอแป๊บนึงก่อนเด้งประเทศต่อไป
                }
            }
            else
            {
                Debug.LogWarning($"Object at index {i} is null in the array.");
            }
        }
    }
}
