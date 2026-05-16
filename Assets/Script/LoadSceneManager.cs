using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("UI References")]
    public Slider progressBar;
    public Canvas Panel;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI LoadingText;
    public RectTransform transitionImage;
    public string CurrentScene;
    public float fadeDuration = 1.0f;
    private string baseText = "Loading";
    private int dotCount = 0;
    public float dotInterval = 0.5f;

    [Header("Animation Settings")]
    [Tooltip("ระยะเวลาที่ภาพเลื่อนเข้ามา (วินาที)")]
    public float slideInDuration = 1.0f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    // ตำแหน่งเริ่มต้นและสิ้นสุดของภาพ (นอกหน้าจอและเต็มหน้าจอ)
    [Header("Position Settings")]
    [Tooltip("ตำแหน่ง Anchored X เมื่อภาพอยู่เต็มหน้าจอ (ปกติเป็น 0)")]
    public float onScreenX = 0f;
    [Tooltip("ตำแหน่ง Anchored X เมื่อภาพอยู่นอกหน้าจอ (เช่น ความกว้างหน้าจอ)")]
    public float offScreenX = 1920f; // สมมติความกว้างหน้าจอ 1920

    public bool isReady = true;

    [Header("Airplane Path (Waypoints)")]
    public RectTransform planeIcon;

    [Tooltip("ใส่จุดอ้างอิงกี่จุดก็ได้ตามต้องการ เรียงจากซ้ายไปขวา")]
    public RectTransform[] waypoints;

    [Tooltip("ปรับองศาถ้ารูปเครื่องบินบินถอยหลัง (เช่น 0, 90, 180, -90)")]
    public float planeRotationOffset = 0f;

    public void AddNewScene(string sceneName)
    {
        if (!isReady) return;
        StartCoroutine(LoadAsyncScene(sceneName, true));
    }

    public void AddNewScene(string sceneName, System.Action onComplete)
    {
        if (!isReady) return;
        StartCoroutine(LoadAsyncScene(sceneName, true, onComplete));
    }

    public void LoadeScene(string sceneName)
    {
        if (!isReady) return;
        StartCoroutine(LoadAsyncScene(sceneName, false));
    }

    public void UnloadCurrentScene(System.Action onComplete = null)
    {
        StartCoroutine(UnloadSequen(onComplete));
    }

    IEnumerator UnloadSequen(System.Action onComplete = null)
    {
        if (!string.IsNullOrEmpty(CurrentScene))
        {
            if (progressBar != null) progressBar.value = 0;
            ResetPlanePosition();

            Panel.gameObject.SetActive(true);

            yield return StartCoroutine(Fade(0, 1));

            // ตรวจสอบก่อนว่า Scene ที่จะ Unload มีอยู่จริงและกำลังโหลดอยู่หรือไม่
            Scene sceneToUnload = SceneManager.GetSceneByName(CurrentScene);
            if (sceneToUnload.IsValid() && sceneToUnload.isLoaded)
            {
                AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(CurrentScene);
                if (asyncLoad != null)
                {
                    StartCoroutine(LoadUI(asyncLoad));
                }
            }
            else
            {
                Debug.LogWarning($"Skipped unloading scene '{CurrentScene}' because it is not currently loaded.");
            }
            
            CurrentScene = null;
            onComplete?.Invoke();
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    IEnumerator LoadAsyncScene(string sceneName, bool add = false, System.Action onComplete = null)
    {
        StartCoroutine(AnimateLoadingText());
        isReady = false;
        AsyncOperation asyncLoad;

        if (progressBar != null) progressBar.value = 0;
        ResetPlanePosition();

        yield return StartCoroutine(AnimateImage(onScreenX, 0, slideInDuration));
        Panel.gameObject.SetActive(true);
        canvasGroup.gameObject.SetActive(true);
        yield return StartCoroutine(AnimateImage(0, offScreenX, slideInDuration));

        //yield return StartCoroutine(Fade(0, 1));

        if (add)
        {
            if (!string.IsNullOrEmpty(CurrentScene))
            {
                StartCoroutine(UnloadSequen());
            }
            Debug.Log("กำลังโหลดฉากใหม่แบบ Additive: " + sceneName);
            asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return StartCoroutine(LoadUI(asyncLoad));
            CurrentScene = sceneName;
        }
        else
        {
            yield return new WaitForSeconds(1);
            asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            yield return StartCoroutine(LoadUI(asyncLoad));
        }
        onComplete?.Invoke();
        yield return StartCoroutine(AnimateImage(onScreenX, 0, slideInDuration));
        canvasGroup.gameObject.SetActive(false);
        yield return StartCoroutine(AnimateImage(0, offScreenX, slideInDuration));
        Panel.gameObject.SetActive(false);

        //yield return StartCoroutine(Fade(1, 0));

        isReady = true;
       
    }

    IEnumerator LoadUI(AsyncOperation asyncLoad)
    {
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 1. ดึงตำแหน่ง X ของจุดเริ่มต้น และ จุดสิ้นสุด เอาไว้คำนวณระยะทาง
        float startX = 0f;
        float endX = 0f;
        if (waypoints != null && waypoints.Length > 1)
        {
            startX = waypoints[0].position.x;
            endX = waypoints[waypoints.Length - 1].position.x;
        }

        float fakeLoadTime = 0f;
        while (fakeLoadTime < 1f)
        {
            fakeLoadTime += Time.deltaTime;
            float t = Mathf.Clamp01(fakeLoadTime);

            if (planeIcon != null && waypoints != null && waypoints.Length > 1)
            {
                // ขยับเครื่องบินก่อน
                Vector3 currentPos = GetSplinePosition(t);
                planeIcon.position = currentPos;

                // 2. ให้หลอดโหลด (Slider) เติมตามตำแหน่ง X ของเครื่องบินเป๊ะๆ
                if (progressBar != null)
                {
                    // InverseLerp จะแปลงตำแหน่ง X ตอนนี้ ให้กลายเป็นเปอร์เซ็นต์ (0 ถึง 1)
                    float progressX = Mathf.InverseLerp(startX, endX, currentPos.x);
                    progressBar.value = progressX;
                }

                // หันหัวเครื่องบิน
                if (t < 0.99f)
                {
                    Vector3 nextPos = GetSplinePosition(Mathf.Clamp01(t + 0.01f));
                    Vector3 direction = (nextPos - currentPos).normalized;
                    if (direction != Vector3.zero)
                    {
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        planeIcon.rotation = Quaternion.Euler(0, 0, angle + planeRotationOffset);
                    }
                }
            }
            else if (progressBar != null)
            {
                // ถ้าไม่มีเครื่องบิน ก็ให้หลอดโหลดวิ่งปกติ
                progressBar.value = t;
            }

            yield return null;
        }

        yield return new WaitForSeconds(1);
      
    }

    private void ResetPlanePosition()
    {
        if (planeIcon != null && waypoints != null && waypoints.Length > 0 && waypoints[0] != null)
        {
            planeIcon.position = waypoints[0].position;
        }
    }

    // ==========================================
    // คณิตศาสตร์คำนวณเส้นโค้ง (Catmull-Rom Spline)
    // ==========================================
    private Vector3 GetSplinePosition(float t)
    {
        int length = waypoints.Length;
        if (length == 0) return Vector3.zero;
        if (length == 1) return waypoints[0].position;
        if (length == 2) return Vector3.Lerp(waypoints[0].position, waypoints[1].position, t);

        t = Mathf.Clamp01(t);
        if (t >= 1f) return waypoints[length - 1].position;

        // คำนวณว่าตอนนี้เครื่องบินอยู่ระหว่างจุดที่เท่าไหร่
        float p = t * (length - 1);
        int i = Mathf.FloorToInt(p);
        float localT = p - i; // เวลาเฉพาะในท่อนนั้น (0 ถึง 1)

        // ดึงจุด 4 จุดเพื่อสร้างความโค้งในท่อนนั้น
        Vector3 p0 = waypoints[Mathf.Max(i - 1, 0)].position;
        Vector3 p1 = waypoints[i].position;
        Vector3 p2 = waypoints[Mathf.Min(i + 1, length - 1)].position;
        Vector3 p3 = waypoints[Mathf.Min(i + 2, length - 1)].position;

        return GetCatmullRomPosition(localT, p0, p1, p2, p3);
    }

    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        return 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
    }

    // ==========================================
    // ส่วนแสดงผลเส้นไกด์ไลน์ (Gizmos) ในหน้าต่าง Scene
    // ==========================================
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        // 1. วาดเส้นทางบิน (สีเขียว)
        Gizmos.color = Color.green;
        Vector3 lastPos = waypoints[0] != null ? waypoints[0].position : Vector3.zero;
        int resolution = waypoints.Length * 10; // ความละเอียดของเส้นวาด

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 currentPos = GetSplinePosition(t);
            Gizmos.DrawLine(lastPos, currentPos);
            lastPos = currentPos;
        }

        // 2. วาดจุด Waypoint แต่ละจุด (สีเหลือง)
        Gizmos.color = Color.yellow;
        foreach (var wp in waypoints)
        {
            if (wp != null) Gizmos.DrawSphere(wp.position, 10f); // เลข 10 คือขนาดจุด ถ้าใหญ่ไปปรับเล็กลงได้ครับ
        }
    }

    // --- ส่วน UI ทั่วไป ---
    IEnumerator AnimateLoadingText()
    {
        while (true)
        {
            string currentText = baseText;
            for (int i = 0; i < dotCount; i++) currentText += ".";
            if (LoadingText != null) LoadingText.text = currentText;
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(dotInterval);
        }
    }

    public void FadeIn() { StartCoroutine(Fade(0, 1)); }
    public void FadeOut() { StartCoroutine(Fade(1, 0)); }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (canvasGroup == null) yield break;
        float time = 0;
        while (time < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }

    // ฟังก์ชันนี้คือตัวที่ทำหน้าที่ "เลื่อนภาพด้วยโค้ด"
    private IEnumerator AnimateImage(float startX, float endX, float duration)
    {
        float timer = 0f;

        // จดจำพิกัดเริ่มต้น และ พิกัดเป้าหมาย
        Vector2 startPos = new Vector2(startX, transitionImage.anchoredPosition.y);
        Vector2 endPos = new Vector2(endX, transitionImage.anchoredPosition.y);

        while (timer < duration)
        {
            // คำนวณเวลาที่ผ่านไป (0.0 ถึง 1.0)
            float normalizedTime = duration > 0 ? timer / duration : 1f;

            // อ่านค่าจากเส้นกราฟ AnimationCurve ที่ตั้งไว้ใน Inspector
            float curveValue = slideCurve.Evaluate(normalizedTime);

            // 🌟 หัวใจหลัก: เลื่อนตำแหน่งของ UI (RectTransform) ด้วย LerpUnclamped
            transitionImage.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, curveValue);

            timer += Time.deltaTime;
            yield return null; // รอเฟรมถัดไป
        }

        // จัดตำแหน่งให้เป๊ะ 100% ตอนจบ
        transitionImage.anchoredPosition = endPos;
    }

    public void PlayLocalTransition(System.Action onMidpointComplete)
    {
        if (!isReady) return;
        StartCoroutine(LocalTransitionRoutine(onMidpointComplete));
    }

    public IEnumerator LocalTransitionRoutine(System.Action onMidpointComplete)
    {
        isReady = false;
        Panel.gameObject.SetActive(true);
        transitionImage.gameObject.SetActive(true);

        // 2. เลื่อนภาพเข้ามา "บังหน้าจอ" (จากนอกจอ -> กลางจอ)
        yield return StartCoroutine(AnimateImage(offScreenX, 0, slideInDuration));

        // 3. 🌟 จังหวะที่ภาพบังมิดจอแล้ว: สั่งให้ทำงานตามที่แนบมา (เช่น วาร์ปผู้เล่น, ย้ายกล้อง)
        onMidpointComplete?.Invoke();

        // รอให้เกมตั้งหลักสักนิดเผื่อกล้องขยับ (ปรับเลขได้ตามต้องการ)
        yield return new WaitForSeconds(1f);

        // 4. เลื่อนภาพ "ออกไปจากจอ" (จากกลางจอ -> ทะลุไปอีกฝั่ง หรือกลับไปที่เดิม)
        // ถ้าอยากให้ภาพไหลทะลุไปอีกด้าน ให้ใส่เครื่องหมายลบ (เช่น -offScreenX)
        yield return StartCoroutine(AnimateImage(0, onScreenX, slideInDuration));
        Panel.gameObject.SetActive(false);
        transitionImage.gameObject.SetActive(false);

        isReady = true;
    }
}