using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BossSonarManager : MonoBehaviour
{
    [Header("ตั้งค่าเวลา")]
    public float timeLimit = 60f;

    [Header("ตั้งค่าตัวคลื่นโซน่าร์ (Sonar)")]
    public GameObject sonarVisualPrefab;
    public float maxSonarRadius = 8f;
    public float fadeSpeed = 2f;

    // 🌟 1. เพิ่มตัวแปรตั้งค่าคูลดาวน์
    [Tooltip("เวลาที่ต้องรอก่อนจะคลิกโซน่าร์ครั้งต่อไปได้ (นับหลังจากหมุนเสร็จ)")]
    public float sonarCooldown = 3f;
    private bool canUseSonar = true; // เอาไว้เช็คว่าโซน่าร์พร้อมใช้งานไหม

    private float currentTime;
    private bool isGameActive = false;
    private TextMeshProUGUI timerText;
    private SceneHandle currentScene;
    private List<GameObject> remainingDogs = new List<GameObject>();

    private class SonarGizmoData
    {
        public Vector3 position;
        public float radius;
    }
    private List<SonarGizmoData> activeGizmos = new List<SonarGizmoData>();

    private void Start()
    {
        currentScene = FindObjectOfType<SceneHandle>();

        GameObject timerObj = GameObject.Find("Text_Timer");
        if (timerObj != null) timerText = timerObj.GetComponent<TextMeshProUGUI>();

        GameObject[] dogs = GameObject.FindGameObjectsWithTag("Dog");
        foreach (GameObject dog in dogs)
        {
            if (dog.GetComponent<HiddenSonarDog>() == null)
            {
                dog.AddComponent<HiddenSonarDog>();
            }
            remainingDogs.Add(dog);
        }

        currentTime = timeLimit;
        isGameActive = true;
        Debug.Log($"🔊 [ระบบ] เริ่มเกม! มีหมาซ่อนอยู่ {remainingDogs.Count} ตัว");
    }

    private void Update()
    {
        if (!isGameActive || !Gamemanager.Instance.isStateGamePlay()) return;

        // 🌟 2. ดักจับการคลิก โดยจะกดได้ก็ต่อเมื่อ canUseSonar เป็น true เท่านั้น
        if (Input.GetMouseButtonDown(0))
        {
            if (canUseSonar)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;
                StartCoroutine(SonarPulseRoutine(mousePos));
            }
            else
            {
                Debug.Log("⏳ โซน่าร์กำลังชาร์จพลัง! รอแป๊บนึง...");
            }
        }

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            if (timerText != null) timerText.text = Mathf.CeilToInt(currentTime).ToString();
            if (currentTime <= 0) GameOver();
        }
    }

    private IEnumerator SonarPulseRoutine(Vector3 originPos)
    {
        if (sonarVisualPrefab == null) yield break;

        // 🌟 3. ล็อกการกดยิงโซน่าร์ทันทีที่เริ่มยิง
        canUseSonar = false;

        GameObject sonarObj = Instantiate(sonarVisualPrefab, originPos, Quaternion.identity);
        sonarObj.transform.localScale = new Vector3(maxSonarRadius, maxSonarRadius, 1f);

        SpriteRenderer sr = sonarObj.GetComponent<SpriteRenderer>();
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;

        float realRadius = (maxSonarRadius * sr.sprite.bounds.size.x) / 2f;

        SonarGizmoData gizmoData = new SonarGizmoData { position = originPos, radius = realRadius };
        activeGizmos.Add(gizmoData);

        float rotatedAmount = 0f;
        float rotationSpeed = 360f / 1.5f;

        // ขั้นตอนหมุนเรดาร์ 360 องศา
        while (rotatedAmount < 360f)
        {
            float step = rotationSpeed * Time.deltaTime;
            sonarObj.transform.Rotate(0, 0, -step);
            rotatedAmount += step;

            Collider2D[] hits = Physics2D.OverlapCircleAll(originPos, realRadius);
            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Dog"))
                {
                    HiddenSonarDog dogScript = hit.GetComponent<HiddenSonarDog>();
                    if (dogScript != null) dogScript.OnHitBySonar();
                }
            }

            yield return null;
        }

        // 🌟 4. พอเรดาร์หมุนเสร็จครบวงแล้ว ให้เรียกตัวนับคูลดาวน์ 3 วินาทีทันที!
        StartCoroutine(CooldownRoutine());

        // หมุนเสร็จแล้ว ค่อยๆ เฟดหายไปตามปกติ
        while (c.a > 0)
        {
            c.a -= fadeSpeed * Time.deltaTime;
            sr.color = c;
            yield return null;
        }

        activeGizmos.Remove(gizmoData);
        Destroy(sonarObj);
    }

    // 🌟 5. ฟังก์ชันนับถอยหลังคูลดาวน์แยกต่างหาก
    private IEnumerator CooldownRoutine()
    {
        float timer = sonarCooldown; // ตั้งค่าเริ่มต้นที่ 3 วินาที

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        // 🌟 ครบ 3 วินาทีแล้ว ปลดล็อกให้กดยิงโซน่าร์ครั้งต่อไปได้
        canUseSonar = true;
        Debug.Log("✅ โซน่าร์ชาร์จเสร็จแล้ว! กดคลิกครั้งต่อไปได้เลย");
    }

    public void CaptureDog(GameObject dog)
    {
        if (remainingDogs.Contains(dog))
        {
            remainingDogs.Remove(dog);
            Debug.Log($"🎉 จับได้! เหลืออีก {remainingDogs.Count} ตัว");

            if (remainingDogs.Count == 0) GameWin();
        }
    }

    public void GameWin()
    {
        isGameActive = false;
        Debug.Log("🏆 [WIN] ชนะแล้ว!");
    }

    public void GameOver()
    {
        currentTime = 0;
        isGameActive = false;
        if (timerText != null) timerText.text = "0";
        Debug.Log("💥 [LOSE] เวลาหมด!");
    }

    private void OnDrawGizmos()
    {
        if (activeGizmos == null) return;

        Gizmos.color = Color.yellow;
        foreach (SonarGizmoData data in activeGizmos)
        {
            Gizmos.DrawWireSphere(data.position, data.radius);
        }
    }
}