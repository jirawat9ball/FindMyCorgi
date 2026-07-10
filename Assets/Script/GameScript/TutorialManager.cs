using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    private GameObject darkOverlay; 
    [Header("ใช้ดูค่า (ไม่ต้องลากใส่)")]
    public GameObject bagBtn;       
    public GameObject htpBtn;       

    private static TutorialManager instance;

    // 🌟 สร้างระบบ Tutorial ทันทีที่เข้าด่าน Tibet
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => {
            if (scene.name.ToLower().Contains("tibet"))
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("TutorialManager");
                    instance = go.AddComponent<TutorialManager>();
                }
                instance.StartCoroutine(instance.TutorialFlow());
            }
        };
    }

    private GameObject FindButtonByName(string keyword)
    {
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
            if (btn.gameObject.scene.name == null) continue; // ข้าม Prefab ในโปรเจกต์
            if (btn.gameObject.name.ToLower().Contains(keyword)) return btn.gameObject;
        }
        return null;
    }

    // 🌟 ฟังก์ชันหา Object อัตโนมัติ (หาเจอแม้จะโดนปิด Active ไว้)
    private GameObject FindInactiveObjectByName(string keyword)
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.gameObject.scene.name == null) continue;
            if (t.name.ToLower().Contains(keyword)) return t.gameObject;
        }
        return null;
    }

    private IEnumerator TutorialFlow()
    {
        // 1. เช็คเซฟเกมว่าเคยเรียนรู้ไปหรือยัง
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            if (Gamemanager.Instance.currentSaveData.hasCompletedTutorial) yield break;
        }

        yield return new WaitForSeconds(1.5f); // รอให้ UI โหลดเข้าฉากเสร็จก่อน

        // หา Object มือชี้ที่เตรียมไว้
        GameObject invHand = FindInactiveObjectByName("inv_hand");
        GameObject htpHand = FindInactiveObjectByName("htp_hand");

        // ปิดมือชี้ไว้ก่อนเผื่อเปิดค้างไว้ใน Scene
        if (invHand != null) invHand.SetActive(false);
        if (htpHand != null) htpHand.SetActive(false);

        // 2. ค้นหาปุ่มกระเป๋า
        bagBtn = FindButtonByName("inventory"); 

        if (bagBtn == null)
        {
            Debug.LogWarning("[TutorialManager] หาปุ่มเป้าหมายไม่เจอ ยกเลิกการสอน");
            yield break;
        }

        // 3. สร้างจอดำบังทั้งจอ
        CreateOverlay(bagBtn);
        darkOverlay.SetActive(true);

        // ================= STEP 1: กดปุ่มกระเป๋า =================

        // ยกปุ่มกระเป๋าของจริงให้ทะลุจอดำขึ้นมา (sorting 1001)
        yield return StartCoroutine(ElevateUI(bagBtn));
        
        // โชว์มือชี้กระเป๋า
        if (invHand != null) invHand.SetActive(true);

        // รอจนกว่ากระเป๋า (หน้าหนังสือ) จะเปิดขึ้นมา
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Inventory == null || 
               !Gamemanager.Instance.uiIngame.panelPopUpManager.Inventory.gameObject.activeSelf) 
        {
            yield return null;
        }

        // ปิดมือชี้กระเป๋า
        if (invHand != null) invHand.SetActive(false);

        // พอเปิดกระเป๋าแล้ว คืนสภาพปุ่มกระเป๋ากลับไปเหมือนเดิม
        RestoreUI(bagBtn);

        // ================= STEP 2: กดปุ่ม How to play =================
        // รอให้แอนิเมชันเปิดหน้าต่างหนังสือเสร็จก่อน ค่อยหาตำแหน่งปุ่ม
        yield return new WaitForSeconds(0.5f); 

        htpBtn = FindButtonByName("how to");    
        if (htpBtn == null)
        {
            Debug.LogWarning("[TutorialManager] หาปุ่ม How to play ไม่เจอ");
            yield break;
        }

        // ตอนนี้หน้าหนังสือเปิดอยู่ **ภายใต้จอดำ** (จอดำทับไปแล้ว)
        // ยกปุ่ม How to play ของจริงให้ทะลุจอดำขึ้นมา
        yield return StartCoroutine(ElevateUI(htpBtn));

        // โชว์มือชี้ How to play
        if (htpHand != null) htpHand.SetActive(true);

        // รอจนกว่าหน้าต่าง How to play จะเด้งเปิดขึ้นมา
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay == null || 
               !Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay.gameObject.activeSelf)
        {
            yield return null;
        }

        // ปิดมือชี้ How to play
        if (htpHand != null) htpHand.SetActive(false);

        // พอกดเปิด howtoplay ปุ๊บ คืนสภาพปุ่มคู่มือ
        RestoreUI(htpBtn);

        // ================= STEP 3: อ่านและปิด How to play =================
        // ปิดจอดำทิ้ง เพื่อให้ผู้เล่นอ่าน How to play ชัดๆ และกดปุ่ม 'กากบาท' ปิดหน้าต่างของจริงได้
        darkOverlay.SetActive(false);

        // รอจนกว่าผู้เล่นจะกดปิดหน้าต่าง How to play ด้วยตัวเอง
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay != null && 
               Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay.gameObject.activeSelf)
        {
            yield return null;
        }

        // ================= จบการสอน =================
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            Gamemanager.Instance.currentSaveData.hasCompletedTutorial = true;
            Gamemanager.Instance.AutoSaveProgress();
        }

        Destroy(darkOverlay);
        Destroy(gameObject);
    }

    private string currentSortingLayer = "Default";

    private void CreateOverlay(GameObject referenceBtn)
    {
        darkOverlay = new GameObject("Tutorial_DarkOverlay");
        
        Canvas rootCanvas = referenceBtn.GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            darkOverlay.transform.SetParent(rootCanvas.rootCanvas.transform, false);
            currentSortingLayer = rootCanvas.rootCanvas.sortingLayerName;
        }

        Canvas darkCanvas = darkOverlay.AddComponent<Canvas>();
        darkCanvas.overrideSorting = true;
        darkCanvas.sortingLayerName = currentSortingLayer; 
        darkCanvas.sortingOrder = 1000; 
        
        darkOverlay.AddComponent<GraphicRaycaster>();

        Image darkImage = darkOverlay.AddComponent<Image>();
        darkImage.color = new Color(0, 0, 0, 0.95f); 
        
        RectTransform rect = darkOverlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        darkOverlay.SetActive(false);
    }

    private Transform originalBagParent;
    private int originalBagIndex;
    
    private Transform originalHtpParent;
    private int originalHtpIndex;

    // 🌟 ฟังก์ชันยกปุ่มจริงให้ทะลุจอดำขึ้นมา (ใช้วิธีย้าย Parent ชั่วคราว 100% สำเร็จแน่นอน!)
    private IEnumerator ElevateUI(GameObject target)
    {
        if (target == null || darkOverlay == null) yield break;

        // 1. จำบ้านเดิมของมันไว้ก่อน
        if (target.name.ToLower().Contains("inventory"))
        {
            originalBagParent = target.transform.parent;
            originalBagIndex = target.transform.GetSiblingIndex();
        }
        else if (target.name.ToLower().Contains("how to"))
        {
            originalHtpParent = target.transform.parent;
            originalHtpIndex = target.transform.GetSiblingIndex();
        }

        // 2. ย้ายปุ่มมาเป็นลูกของ "จอดำ" (darkOverlay)
        // SetParent(..., true) จะรักษาสเกลและตำแหน่งบนจอไว้เหมือนเดิมเป๊ะๆ
        target.transform.SetParent(darkOverlay.transform, true);
        
        // 3. เอาปุ่มมาไว้ล่างสุดของ Hierarchy ลูก เพื่อให้ถูกวาดทับจอดำ
        target.transform.SetAsLastSibling();

        Debug.Log($"[TutorialManager] ย้ายปุ่ม {target.name} มาไว้บนจอดำสำเร็จ!");
        
        yield return null;
    }

    // 🌟 ฟังก์ชันคืนสภาพปุ่มกลับสู่ปกติ
    private void RestoreUI(GameObject target)
    {
        if (target == null) return;

        // ย้ายกลับบ้านเดิม
        if (target.name.ToLower().Contains("inventory") && originalBagParent != null)
        {
            target.transform.SetParent(originalBagParent, true);
            target.transform.SetSiblingIndex(originalBagIndex);
        }
        else if (target.name.ToLower().Contains("how to") && originalHtpParent != null)
        {
            target.transform.SetParent(originalHtpParent, true);
            target.transform.SetSiblingIndex(originalHtpIndex);
        }
    }

}
