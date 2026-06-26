using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// 🌟 1. สร้างโครงสร้างเก็บข้อมูลของแต่ละรอบ (ใส่ Serializable เพื่อให้โชว์ใน Inspector)
[System.Serializable]
public class CupRoundConfig
{
    [Header("ตั้งค่าความยากรอบนี้")]
    public int numberOfCups = 3;       // จำนวนถ้วย
    public float shuffleSpeed = 10f;    // ความเร็วสลับ
    public int shuffleAmount = 5;      // สลับกี่ครั้ง
    public float showDogDuration = 2f;  // เวลาโชว์หมาก่อนสลับ
}

public class BossCupShuffleManager : MonoBehaviour
{
    [Header("ตั้งค่าระบบรอบ (Rounds)")]
    [Tooltip("เพิ่มจำนวนรอบและตั้งค่าความยากแต่ละรอบได้ที่นี่")]
    public CupRoundConfig[] rounds;
    private int currentRoundIndex = 0; // จำว่าตอนนี้อยู่รอบที่เท่าไหร่

    [Header("ตั้งค่าถ้วย")]
    public GameObject cupPrefab;
    public float cupSpacing = 2.5f;     // ระยะห่างระหว่างถ้วย
    public Transform tableCenterPoint;  // 🌟 เปลี่ยนมาใช้จุดกึ่งกลางโต๊ะแทน เพื่อให้ถ้วยบาลานซ์ตรงกลางเสมอ

    [Header("เฉดสีถ้วย (เรียงลำดับ)")]
    public Color[] cupColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };

    private List<GameObject> activeCups = new List<GameObject>();
    private bool isShuffling = false;
    private bool canPick = false;
    private string correctCupName;

    private SceneHandle currentScene;
    private TextMeshProUGUI timerText;

    private void Start()
    {
        currentScene = FindObjectOfType<SceneHandle>();
        GameObject timerObj = GameObject.Find("Text_Timer");
        if (timerObj != null) timerText = timerObj.GetComponent<TextMeshProUGUI>();

        if (rounds.Length == 0)
        {
            Debug.LogError("ยังไม่ได้ตั้งค่า Rounds ใน Inspector ครับ!");
            return;
        }

        StartRound(); // เริ่มรอบแรก
    }

    private void StartRound()
    {
        Debug.Log($"====== 🏁 เริ่มด่านสลับถ้วย รอบที่ {currentRoundIndex + 1} / {rounds.Length} ======");

        // 1. ทำลายถ้วยเก่าทิ้งก่อน (ถ้านี่ไม่ใช่รอบแรก)
        foreach (GameObject oldCup in activeCups)
        {
            Destroy(oldCup);
        }
        activeCups.Clear();

        // 2. โหลดการตั้งค่าของรอบปัจจุบัน
        CupRoundConfig currentConfig = rounds[currentRoundIndex];

        // สุ่มหาตำแหน่งหมา
        int correctCupIndex = Random.Range(0, currentConfig.numberOfCups);

        // 🌟 3. คำนวณหาตำแหน่ง X เริ่มต้น เพื่อให้ถ้วยทั้งหมดวางกึ่งกลางจอพอดี
        float totalWidth = (currentConfig.numberOfCups - 1) * cupSpacing;
        float startX = tableCenterPoint.position.x - (totalWidth / 2f);

        for (int i = 0; i < currentConfig.numberOfCups; i++)
        {
            Vector3 spawnPos = new Vector3(startX + (i * cupSpacing), tableCenterPoint.position.y, 0);
            GameObject newCup = Instantiate(cupPrefab, spawnPos, Quaternion.identity, transform);
            newCup.name = "BossCup_" + i;
            activeCups.Add(newCup);

            newCup.AddComponent<CupClickHandler>().manager = this;

            // ใส่สีถ้วย (ถ้าถ้วยเยอะกว่าสีที่มี จะวนกลับไปใช้สีแรกใหม่)
            SpriteRenderer cupRenderer = newCup.GetComponent<SpriteRenderer>();
            if (cupRenderer != null)
            {
                cupRenderer.color = cupColors[i % cupColors.Length];
            }

            Transform dogTrans = newCup.transform.Find("Dog_Inside");
            if (dogTrans != null)
            {
                if (i == correctCupIndex)
                {
                    dogTrans.gameObject.SetActive(true);
                    correctCupName = newCup.name;
                    Debug.Log($"[ระบบ] 👀 โชว์หมาที่ถ้วยใบที่ {i + 1}");
                }
                else
                {
                    dogTrans.gameObject.SetActive(false);
                }
            }
        }

        StartCoroutine(PlayShuffleSequence(currentConfig));
    }

    private IEnumerator PlayShuffleSequence(CupRoundConfig config)
    {
        yield return new WaitForSeconds(config.showDogDuration);

        // ซ่อนหมาทั้งหมด
        foreach (GameObject cup in activeCups)
        {
            Transform dogTrans = cup.transform.Find("Dog_Inside");
            if (dogTrans != null) dogTrans.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        isShuffling = true;
        for (int i = 0; i < config.shuffleAmount; i++)
        {
            yield return StartCoroutine(ShuffleRoutine(config.shuffleSpeed));
        }
        isShuffling = false;

        canPick = true;
        Debug.Log("🎲 สลับเสร็จแล้ว! เลือกเลย!");
    }

    private IEnumerator ShuffleRoutine(float speed)
    {
        int indexA = Random.Range(0, activeCups.Count);
        int indexB = Random.Range(0, activeCups.Count);
        while (indexA == indexB) { indexB = Random.Range(0, activeCups.Count); }

        GameObject cupA = activeCups[indexA];
        GameObject cupB = activeCups[indexB];

        Vector3 posA = cupA.transform.position;
        Vector3 posB = cupB.transform.position;

        float elapsed = 0f;
        float duration = 1f / speed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            cupA.transform.position = Vector3.Lerp(posA, posB, t);
            cupB.transform.position = Vector3.Lerp(posB, posA, t);
            yield return null;
        }

        cupA.transform.position = posB;
        cupB.transform.position = posA;

        activeCups[indexA] = cupB;
        activeCups[indexB] = cupA;
    }

    public void OnCupClicked(GameObject clickedCup)
    {
        if (!canPick) return;
        canPick = false;

        clickedCup.transform.position += Vector3.up * 1.5f;
        Transform dogTrans = clickedCup.transform.Find("Dog_Inside");

        bool hasDog = (clickedCup.name == correctCupName);

        if (hasDog)
        {
            if (dogTrans != null)
            {
                dogTrans.SetParent(null);
                dogTrans.gameObject.SetActive(true);
            }

            Debug.Log($"🎉 ถูกต้อง! ผ่านรอบที่ {currentRoundIndex + 1} แล้ว!");

            // ตรวจสอบว่ามีรอบต่อไปไหม?
            currentRoundIndex++;
            if (currentRoundIndex < rounds.Length)
            {
                // ถ้ามี ให้หน่วงเวลาพักหายใจ 2 วินาที แล้วเริ่มรอบใหม่
                StartCoroutine(PrepareNextRound());
            }
            else
            {
                // ถ้าครบทุกรอบแล้ว ชนะด่านบอสจริงๆ!
                GameWin();
            }
        }
        else
        {
            Debug.Log("💀 ว่างเปล่า... ทายผิด Game Over!");
            GameOver();
        }
    }

    private IEnumerator PrepareNextRound()
    {
        Debug.Log("⏳ กำลังเตรียมตัวสู่รอบถัดไป...");
        yield return new WaitForSeconds(2f); // หน่วงเวลาให้ผู้เล่นดีใจแป๊บนึง

        // อย่าลืมลบหมาที่หลุดจากการเป็นลูกถ้วยตอนทายถูกทิ้งด้วย ไม่งั้นหมาจะค้างในฉาก
        GameObject strayDog = GameObject.Find("Dog_Inside");
        if (strayDog != null) Destroy(strayDog);

        StartRound(); // เรียกรอบต่อไป
    }

    public void GameWin()
    {
        Debug.Log("🏆 [WIN] เอาชนะบอสครบทุกรอบเรียบร้อย!!");
    }

    public void GameOver()
    {
        Debug.Log("💥 [LOSE] แพ้แล้ว! กลับไปเริ่มใหม่นะ!");
    }
    public class CupClickHandler : MonoBehaviour
    {
        public BossCupShuffleManager manager;
        private void OnMouseDown()
        {
            if (manager != null) manager.OnCupClicked(gameObject);
        }
    }
}

