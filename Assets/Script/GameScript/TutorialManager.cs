using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    private GameObject darkOverlay;
    [Header("ปุ่มเป้าหมาย (กำหนดจาก Inspector หรือค้นหาเอง)")]
    public GameObject bagBtn;
    public GameObject htpBtn;

    private static TutorialManager instance;

    // ── บันทึก hierarchy เดิมของ bagBtn ──────────────────────────────
    private Transform originalBagParent;
    private int originalBagIndex;

    // ── บันทึก hierarchy และตำแหน่งเดิมของ htpBtn ───────────────────
    private Transform originalHtpParent;
    private int originalHtpIndex;
    private Vector2 originalHtpPos;

    private string currentSortingLayer = "Default";

    // ── รัน Tutorial อัตโนมัติเมื่อโหลด scene Tibet / Jordan ─────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
        {
            string sceneName = scene.name.ToLower();
            if (sceneName.Contains("tibet") || sceneName.Contains("jordan"))
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

    // ── ค้นหา Button ที่ Active อยู่ใน scene ────────────────────────
    private GameObject FindButtonByName(string keyword)
    {
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
            if (btn.gameObject.scene.name == null) continue; // ข้าม Prefab ที่ยังไม่ได้ instantiate
            if (btn.gameObject.name.ToLower().Contains(keyword)) return btn.gameObject;
        }
        return null;
    }

    // ── ค้นหา Object ที่ Inactive ก็ได้ ─────────────────────────────
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

    // ── สร้าง Dark Overlay Canvas ────────────────────────────────────
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

    // ── Tutorial Flow หลัก ───────────────────────────────────────────
    private IEnumerator TutorialFlow()
    {
        // ข้ามถ้าทำ tutorial ไปแล้ว
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            if (Gamemanager.Instance.currentSaveData.hasCompletedTutorial) yield break;
        }

        yield return new WaitForSeconds(1.5f); // รอให้ UI โหลดครบก่อน

        // หา hand pointer objects (อาจ inactive อยู่)
        GameObject invHand = FindInactiveObjectByName("inv_hand");
        GameObject htpHand = FindInactiveObjectByName("htp_hand");

        // ซ่อน hand ทั้งคู่ก่อนเริ่ม เผื่อมี state เก่าค้างใน scene
        if (invHand != null) invHand.SetActive(false);
        if (htpHand != null) htpHand.SetActive(false);

        // หาปุ่ม Inventory
        bagBtn = FindButtonByName("inventory");
        if (bagBtn == null)
        {
            Debug.LogWarning("[TutorialManager] หาปุ่มกระเป๋าไม่เจอ ยกเลิกการสอน");
            yield break;
        }

        // สร้าง Dark Overlay
        CreateOverlay(bagBtn);
        darkOverlay.SetActive(true);

        // ================= STEP 1: กดปุ่มกระเป๋า =================

        // ยกปุ่มกระเป๋าของจริงให้ทะลุจอดำขึ้นมา
        yield return StartCoroutine(ElevateUI(bagBtn));

        // แสดง hand pointer ชี้ปุ่มกระเป๋า
        if (invHand != null) invHand.SetActive(true);

        // รอจนกว่า Inventory จะเปิดขึ้นมา
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Inventory == null ||
               !Gamemanager.Instance.uiIngame.panelPopUpManager.Inventory.gameObject.activeSelf)
        {
            yield return null;
        }

        // ซ่อน hand และคืนปุ่มกระเป๋ากลับไปเหมือนเดิม
        if (invHand != null) invHand.SetActive(false);
        RestoreUI(bagBtn);

        // ================= STEP 2: กดปุ่ม How to play =================

        // รอให้แอนิเมชันเปิดหน้าหนังสือเสร็จก่อน ค่อยหาปุ่ม HTP
        yield return new WaitForSeconds(0.5f);

        htpBtn = FindButtonByName("how to");
        if (htpBtn == null)
        {
            Debug.LogWarning("[TutorialManager] หาปุ่ม How to play ไม่เจอ");
            yield break;
        }

        // ยกปุ่ม HTP ของจริงให้ทะลุจอดำขึ้นมา
        yield return StartCoroutine(ElevateUI(htpBtn));

        // แสดง hand pointer ชี้ปุ่ม HTP
        if (htpHand != null) htpHand.SetActive(true);

        // รอจนกว่าหน้าต่าง How to play จะเปิดขึ้นมา
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay == null ||
               !Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay.gameObject.activeSelf)
        {
            yield return null;
        }

        // ซ่อน hand และคืนปุ่ม HTP กลับไปเหมือนเดิม
        if (htpHand != null) htpHand.SetActive(false);
        RestoreUI(htpBtn);

        // ================= STEP 3: อ่านและปิด How to play =================

        // ปิดจอดำ เพื่อให้ผู้เล่นอ่าน HTP ได้ชัดและกดปิดเองได้
        darkOverlay.SetActive(false);

        // รอจนกว่าผู้เล่นจะกดปิด How to play ด้วยตัวเอง
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay != null &&
               Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay.gameObject.activeSelf)
        {
            yield return null;
        }

        // ================= จบการสอน =================

        // บันทึกว่าทำ tutorial เสร็จแล้ว
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            Gamemanager.Instance.currentSaveData.hasCompletedTutorial = true;
            Gamemanager.Instance.AutoSaveProgress();
        }

        Destroy(darkOverlay);
        Destroy(gameObject);
    }

    // ── ยกปุ่มให้ทะลุจอดำ (ย้าย Parent ชั่วคราว) ────────────────────
    // หลังย้ายแล้ว localPosition จะเปลี่ยน จึงต้อง update ตำแหน่ง hover ด้วย
    private IEnumerator ElevateUI(GameObject target)
    {
        if (target == null || darkOverlay == null) yield break;

        // 1. จำ hierarchy เดิมไว้
        if (target.name.ToLower().Contains("inventory"))
        {
            originalBagParent = target.transform.parent;
            originalBagIndex = target.transform.GetSiblingIndex();
        }
        else if (target.name.ToLower().Contains("how to"))
        {
            originalHtpParent = target.transform.parent;
            originalHtpIndex = target.transform.GetSiblingIndex();

            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                originalHtpPos = rect.anchoredPosition;
            }
        }

        // 2. ย้ายปุ่มมาเป็นลูกของ darkOverlay (worldPositionStays=true รักษาตำแหน่งบนจอ)
        target.transform.SetParent(darkOverlay.transform, true);

        // 3. วาดทับจอดำ (last sibling = ด้านหน้าสุด)
        target.transform.SetAsLastSibling();

        // 4. ปรับตำแหน่ง htpBtn ให้ตรงตำแหน่งที่ต้องการบนจอ
        if (target.name.ToLower().Contains("how to"))
        {
            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 pos = rect.anchoredPosition;
                pos.x = 261.59f;
                pos.y = -175f;
                rect.anchoredPosition = pos;
            }
        }

        // 5. รอ 1 frame ให้ Unity คำนวณ localPosition ใหม่หลัง SetParent
        yield return null;

        // 6. Update originalPosition ของ hover effect ให้ตรงกับตำแหน่งใหม่
        //    เพราะ localPosition เปลี่ยนหลังย้าย parent ทำให้ hover drift ออกนอกกรอบ
        ButtonHoverEffect hover = target.GetComponent<ButtonHoverEffect>();
        if (hover != null)
        {
            hover.UpdateOriginalPosition();
        }

        Debug.Log($"[TutorialManager] ยก {target.name} ขึ้นบนจอดำสำเร็จ!");
    }

    // ── คืนปุ่มกลับ hierarchy เดิม ───────────────────────────────────
    private void RestoreUI(GameObject target)
    {
        if (target == null) return;

        // คืนกลับ parent เดิม
        if (target.name.ToLower().Contains("inventory") && originalBagParent != null)
        {
            target.transform.SetParent(originalBagParent, true);
            target.transform.SetSiblingIndex(originalBagIndex);
        }
        else if (target.name.ToLower().Contains("how to") && originalHtpParent != null)
        {
            target.transform.SetParent(originalHtpParent, true);
            target.transform.SetSiblingIndex(originalHtpIndex);

            // คืนตำแหน่ง anchoredPosition เดิมของ HTP
            RectTransform rect = target.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = originalHtpPos;
            }
        }

        // Update originalPosition ของ hover effect ให้ตรงกับตำแหน่งหลัง restore
        ButtonHoverEffect hover = target.GetComponent<ButtonHoverEffect>();
        if (hover != null)
        {
            hover.UpdateOriginalPosition();
        }
    }

}
