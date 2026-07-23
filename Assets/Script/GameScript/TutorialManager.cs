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
    [Header("ตั้งค่า Effect (รูปภาพแอนิเมชัน)")]
    public Sprite[] tutorialEffectSprites;
    public float effectFrameDuration = 0.05f;
    public float effectScaleMultiplier = 1.2f; // ขนาดของ Effect เทียบกับปุ่ม

    private static TutorialManager instance;

    // ── บันทึก hierarchy เดิมของ bagBtn ──────────────────────────────
    private Transform originalBagParent;
    private int originalBagIndex;

    // ── บันทึก hierarchy และตำแหน่งเดิมของ htpBtn ───────────────────
    private Transform originalHtpParent;
    private int originalHtpIndex;
    private Vector2 originalHtpPos;

    private string currentSortingLayer = "Default";
    private Coroutine activeEffectCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // ── รัน Tutorial อัตโนมัติเมื่อโหลด scene Tibet / Jordan ─────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
        {
            string sceneName = scene.name.ToLower();
            if (sceneName.Contains("tibet") || sceneName.Contains("jordan"))
            {
                // ลองหาตัวที่มีอยู่ใน Scene (ที่คุณตั้งค่าไว้ใน Inspector)
                if (instance == null)
                {
                    instance = Object.FindObjectOfType<TutorialManager>();
                }
                
                // ถ้ายังไม่มี ค่อยสร้างใหม่
                if (instance == null)
                {
                    GameObject go = new GameObject("TutorialManager");
                    instance = go.AddComponent<TutorialManager>();
                }
                
                // ถ้ามีรันอยู่แล้ว ให้หยุดก่อน
                instance.StopAllCoroutines();
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
        // รอให้ UI เริ่มต้นสักนิดก่อนหาปุ่ม
        yield return new WaitForSeconds(0.1f);

        // หาปุ่ม Inventory และ HTP
        bagBtn = FindButtonByName("inventory");
        htpBtn = FindButtonByName("how to") ?? FindButtonByName("htpbtn");

        // 🌟 ปิด Effect ที่อาจจะถูกเปิดค้างไว้ใน Editor ตั้งแต่แรกให้หมดก่อน
        if (bagBtn != null)
        {
            Transform bagFx = bagBtn.transform.Find("TutorialEffect");
            if (bagFx != null) bagFx.gameObject.SetActive(false);
        }
        if (htpBtn != null)
        {
            Transform htpFx = htpBtn.transform.Find("TutorialEffect");
            if (htpFx != null) htpFx.gameObject.SetActive(false);

            if (htpBtn.transform.parent != null)
            {
                Transform parentFx = htpBtn.transform.parent.Find("TutorialEffect");
                if (parentFx != null) parentFx.gameObject.SetActive(false);
            }
        }

        // ข้ามถ้าทำ tutorial ไปแล้ว
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            if (Gamemanager.Instance.currentSaveData.hasCompletedTutorial)
            {
                // ถ้าสคริปต์นี้ถูกแปะไว้ที่ Gamemanager ห้ามลบ gameObject ทิ้ง!
                if (gameObject.name == "TutorialManager") Destroy(gameObject);
                else Destroy(this);
                
                yield break;
            }
        }

        yield return new WaitForSeconds(1.4f); // รอให้ UI โหลดครบตามเวลาเดิม

        // หา hand pointer objects (อาจ inactive อยู่)
        GameObject invHand = FindInactiveObjectByName("inv_hand");
        GameObject htpHand = FindInactiveObjectByName("htp_hand");

        // ซ่อน hand ทั้งคู่ก่อนเริ่ม เผื่อมี state เก่าค้างใน scene
        if (invHand != null) invHand.SetActive(false);
        if (htpHand != null) htpHand.SetActive(false);

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

        // แสดง hand pointer ชี้ปุ่มกระเป๋าแบบไม่เฟด แต่มีอนิเมชันลอย
        Vector2 invHandStartPos = Vector2.zero;
        Coroutine invHandFloatCoroutine = null;
        if (invHand != null) 
        {
            invHand.SetActive(true);
            invHandStartPos = invHand.GetComponent<RectTransform>().anchoredPosition;
            invHandFloatCoroutine = StartCoroutine(FloatAnimation(invHand, invHandStartPos));
        }

        // รอจนกว่า Inventory จะเปิดขึ้นมา
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Inventory == null ||
               !Gamemanager.Instance.uiIngame.panelPopUpManager.Inventory.gameObject.activeSelf)
        {
            yield return null;
        }

        // ซ่อน hand และคืนปุ่มกระเป๋ากลับไปเหมือนเดิม
        if (invHand != null) 
        {
            if (invHandFloatCoroutine != null) StopCoroutine(invHandFloatCoroutine);
            invHand.GetComponent<RectTransform>().anchoredPosition = invHandStartPos;
            invHand.SetActive(false);
        }
        RestoreUI(bagBtn);

        // ================= STEP 2: กดปุ่ม How to play =================

        // รอให้แอนิเมชันเปิดหน้าหนังสือเสร็จก่อน ค่อยหาปุ่ม HTP
        yield return new WaitForSeconds(0.5f);

        htpBtn = FindButtonByName("how to") ?? FindButtonByName("htpbtn");
        if (htpBtn == null)
        {
            Debug.LogWarning("[TutorialManager] หาปุ่ม How to play ไม่เจอ");
            yield break;
        }

        // ยกปุ่ม HTP ของจริงให้ทะลุจอดำขึ้นมา
        yield return StartCoroutine(ElevateUI(htpBtn));

        // แสดง hand pointer ชี้ปุ่ม HTP แบบนุ่มนวล
        Vector2 htpHandStartPos = Vector2.zero;
        Coroutine htpHandFloatCoroutine = null;
        if (htpHand != null) 
        {
            htpHandStartPos = htpHand.GetComponent<RectTransform>().anchoredPosition;
            yield return StartCoroutine(SmoothShowHand(htpHand));
            htpHandFloatCoroutine = StartCoroutine(FloatAnimation(htpHand, htpHandStartPos));
        }

        // รอจนกว่าหน้าต่าง How to play จะเปิดขึ้นมา
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay == null ||
               !Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay.gameObject.activeSelf)
        {
            yield return null;
        }

        // ซ่อน hand และคืนปุ่ม HTP กลับไปเหมือนเดิม
        if (htpHand != null) 
        {
            if (htpHandFloatCoroutine != null) StopCoroutine(htpHandFloatCoroutine);
            htpHand.GetComponent<RectTransform>().anchoredPosition = htpHandStartPos;
            StartCoroutine(SmoothHideHand(htpHand)); // ไม่ต้อง yield return
        }
        RestoreUI(htpBtn);

        // ================= STEP 3: อ่านและปิด How to play =================

        // 🌟 จอดำของ Tutorial จะ FadeOut ค่อยๆ จางหายไปพร้อมกับจอดำของ HTP ที่กำลัง FadeIn พอดี
        // ป้องกันไม่ให้สีดำมันซ้อนกันจนเข้มเกินไป
        Image darkImage = darkOverlay.GetComponent<Image>();
        if (darkImage != null)
        {
            darkImage.raycastTarget = false; // 🌟 ปิดการบล็อกคลิก เพื่อให้กดปุ่มปิด How to play ได้
            StartCoroutine(darkImage.FadeOut(0.25f)); // ใช้เวลา 0.25s เท่ากับที่ HTP เฟดเข้า
        }

        // รอจนกว่าผู้เล่นจะกดปิด How to play ด้วยตัวเอง
        while (Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay != null &&
               Gamemanager.Instance.uiIngame.panelPopUpManager.Howtoplay.gameObject.activeSelf)
        {
            yield return null;
        }

        // ปิดจอดำทิ้งไปเลย
        darkOverlay.SetActive(false);

        // ================= จบการสอน =================

        // บันทึกว่าทำ tutorial เสร็จแล้ว
        if (Gamemanager.Instance != null && Gamemanager.Instance.currentSaveData != null)
        {
            Gamemanager.Instance.currentSaveData.hasCompletedTutorial = true;
            Gamemanager.Instance.AutoSaveProgress();
        }

        Destroy(darkOverlay);
        
        // ลบแค่สคริปต์ตัวเองทิ้ง ป้องกันการลบ Gamemanager โดยไม่ได้ตั้งใจ
        if (gameObject.name == "TutorialManager") Destroy(gameObject);
        else Destroy(this);
    }

    // ── ยกปุ่มให้ทะลุจอดำ (ย้าย Parent ชั่วคราว) ────────────────────
    // หลังย้ายแล้ว localPosition จะเปลี่ยน จึงต้อง update ตำแหน่ง hover ด้วย
    private IEnumerator ElevateUI(GameObject target)
    {
        if (target == null || darkOverlay == null) yield break;

        // 🌟 ถ้านี่คือปุ่ม How to play และถูกคลุมด้วย Empty Object (Mask) 
        // เราจะทำการยกตัวคลุม (Parent) ขึ้นมาแทน เพื่อให้ Mask ยังทำงานอยู่
        GameObject objectToElevate = target;
        if (target.name.ToLower().Contains("how to") || target.name.ToLower().Contains("htp"))
        {
            // เช็คว่า Parent ของปุ่ม ไม่ใช่ Book/Canvas โดยตรง (เช่นชื่อมีคำว่า Mask หรือเป็น Empty Object ที่เพิ่งสร้าง)
            // คุณสามารถตั้งชื่อ Empty object ว่า "HTPMask" ได้
            if (target.transform.parent != null)
            {
                objectToElevate = target.transform.parent.gameObject;
            }
        }

        // 1. จำ hierarchy เดิมไว้
        if (target.name.ToLower().Contains("inventory"))
        {
            originalBagParent = objectToElevate.transform.parent;
            originalBagIndex = objectToElevate.transform.GetSiblingIndex();
        }
        else if (target.name.ToLower().Contains("how to") || target.name.ToLower().Contains("htp"))
        {
            originalHtpParent = objectToElevate.transform.parent;
            originalHtpIndex = objectToElevate.transform.GetSiblingIndex();

            RectTransform rect = objectToElevate.GetComponent<RectTransform>();
            if (rect != null)
            {
                originalHtpPos = rect.anchoredPosition;
            }
        }

        // 2. ย้าย Object มาเป็นลูกของ darkOverlay
        objectToElevate.transform.SetParent(darkOverlay.transform, true);

        // 3. วาดทับจอดำ
        objectToElevate.transform.SetAsLastSibling();

        // 4. ปรับตำแหน่งให้ตรงกับบนจอดำ
        if (target.name.ToLower().Contains("how to") || target.name.ToLower().Contains("htp"))
        {
            RectTransform rect = objectToElevate.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 pos = rect.anchoredPosition;
                pos.x = 50f;
                pos.y = 4.7f;
                rect.anchoredPosition = pos;
            }
        }

        // 5. รอ 1 frame
        yield return null;

        // 6. Update originalPosition ของ hover
        ButtonHoverEffect hover = target.GetComponent<ButtonHoverEffect>();
        if (hover != null)
        {
            hover.UpdateOriginalPosition();
        }

        // 🌟 เอฟเฟกต์เด้งป๊อปอัพ (Ultrasmooth Pop)
        StartCoroutine(PopScaleEffect(objectToElevate.transform, objectToElevate.transform.localScale));

        // 🌟 เริ่มเล่น Effect (Frame-by-frame) วนซ้ำ
        // โดยจะหา Object ลูกที่ชื่อ "TutorialEffect" ที่คุณสร้างรอไว้ใน Editor
        Transform effectChild = objectToElevate.transform.Find("TutorialEffect");
        if (effectChild != null && tutorialEffectSprites != null && tutorialEffectSprites.Length > 0)
        {
            effectChild.gameObject.SetActive(true);
            
            // 🌟 จัดตำแหน่งและขนาดให้ตรงกลางและใหญ่กว่าปุ่มนิดนึง (เผื่อลืมจัดใน Editor)
            RectTransform effectRect = effectChild.GetComponent<RectTransform>();
            RectTransform targetRect = objectToElevate.GetComponent<RectTransform>();
            if (effectRect != null && targetRect != null)
            {
                effectRect.sizeDelta = targetRect.rect.size * effectScaleMultiplier;
                effectRect.anchoredPosition = Vector2.zero;
                effectRect.SetAsLastSibling();
            }

            Image effectImg = effectChild.GetComponent<Image>();
            if (effectImg == null)
            {
                effectImg = effectChild.gameObject.AddComponent<Image>();
                effectImg.raycastTarget = false;
            }

            if (activeEffectCoroutine != null) StopCoroutine(activeEffectCoroutine);
            activeEffectCoroutine = StartCoroutine(SpriteAnimationEffect(effectImg));
        }

        Debug.Log($"[TutorialManager] ยก {objectToElevate.name} ขึ้นบนจอดำสำเร็จ!");
    }

    // ── คืนปุ่มกลับ hierarchy เดิม ───────────────────────────────────
    private void RestoreUI(GameObject target)
    {
        if (target == null) return;

        // หา Parent หลักที่ถูกยกขึ้นมา (ถ้ามีการครอบ Mask)
        GameObject objectToRestore = target;
        if (target.transform.parent != null && target.transform.parent.gameObject != darkOverlay)
        {
            objectToRestore = target.transform.parent.gameObject;
        }

        // 🌟 หยุด Effect ภาพแอนิเมชัน และปิด GameObject
        if (activeEffectCoroutine != null)
        {
            StopCoroutine(activeEffectCoroutine);
            activeEffectCoroutine = null;
        }
        
        Transform effectChild = objectToRestore.transform.Find("TutorialEffect");
        if (effectChild != null)
        {
            effectChild.gameObject.SetActive(false);
        }

        // คืนกลับ parent เดิม
        if (target.name.ToLower().Contains("inventory") && originalBagParent != null)
        {
            target.transform.SetParent(originalBagParent, true);
            target.transform.SetSiblingIndex(originalBagIndex);
        }
        else if ((target.name.ToLower().Contains("how to") || target.name.ToLower().Contains("htp")) && originalHtpParent != null)
        {
            objectToRestore.transform.SetParent(originalHtpParent, true);
            objectToRestore.transform.SetSiblingIndex(originalHtpIndex);

            // คืนตำแหน่ง anchoredPosition เดิมของ HTP (และ Mask ถ้ามี)
            RectTransform rect = objectToRestore.GetComponent<RectTransform>();
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

    // ── ฟังก์ชันเสริมความ Ultrasmooth ───────────────────────────────────
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float targetAlpha, float duration)
    {
        if (cg == null) yield break;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    private IEnumerator SmoothShowHand(GameObject hand)
    {
        if (hand == null) yield break;
        hand.SetActive(true);
        CanvasGroup cg = hand.GetComponent<CanvasGroup>();
        if (cg == null) cg = hand.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        yield return StartCoroutine(FadeCanvasGroup(cg, 0f, 1f, 0.3f));
    }

    private IEnumerator SmoothHideHand(GameObject hand)
    {
        if (hand == null) yield break;
        CanvasGroup cg = hand.GetComponent<CanvasGroup>();
        if (cg == null) cg = hand.AddComponent<CanvasGroup>();
        yield return StartCoroutine(FadeCanvasGroup(cg, cg.alpha, 0f, 0.3f));
        hand.SetActive(false);
    }

    private IEnumerator FloatAnimation(GameObject obj, Vector2 startPos)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) yield break;
        
        float speed = 6f;
        float amplitude = 12f;
        
        while (true)
        {
            rect.anchoredPosition = startPos + new Vector2(0, Mathf.Sin(Time.time * speed) * amplitude);
            yield return null;
        }
    }

    private IEnumerator PopScaleEffect(Transform target, Vector3 originalScale)
    {
        if (target == null) yield break;
        float duration = 0.2f;
        float time = 0;
        Vector3 targetScale = originalScale * 1.15f;
        
        // Scale up
        while (time < duration)
        {
            if (target == null) yield break;
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            target.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        time = 0;
        duration = 0.15f;
        // Scale back to original
        while (time < duration)
        {
            if (target == null) yield break;
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration);
            target.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        if (target != null) target.localScale = originalScale;
    }

    // 🌟 Effect สลับรูปภาพไปเรื่อยๆ
    private IEnumerator SpriteAnimationEffect(Image effectImage)
    {
        int index = 0;
        while (true)
        {
            if (tutorialEffectSprites != null && tutorialEffectSprites.Length > 0)
            {
                Sprite currentSprite = tutorialEffectSprites[index];
                if (currentSprite != null)
                {
                    effectImage.sprite = currentSprite;
                    effectImage.color = Color.white; // แสดงสีปกติเมื่อมีรูป
                }
                else
                {
                    effectImage.color = new Color(1, 1, 1, 0); // ซ่อนกล่องสี่เหลี่ยมขาวถ้ายังไม่ได้ใส่รูป
                }
                
                index = (index + 1) % tutorialEffectSprites.Length;
            }
            yield return new WaitForSeconds(effectFrameDuration);
        }
    }
}
