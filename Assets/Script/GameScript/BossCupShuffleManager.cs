using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CupRoundConfig
{
    public int numberOfCups = 3;
    public float shuffleSpeed = 10f;
    public int shuffleAmount = 5;
    public float showDogDuration = 2f;
}

public class BossCupShuffleManager : MonoBehaviour
{
    [Header("ตั้งค่าระบบรอบ (Rounds)")]
    public CupRoundConfig[] rounds;
    private int currentRoundIndex = 0;

    [Header("ตั้งค่าถ้วย")]
    public GameObject cupPrefab;
    public float cupSpacing = 2.5f;
    public Transform tableCenterPoint;

    [Header("เฉดสีถ้วย (เรียงลำดับ)")]
    public Color[] cupColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan };

    [Header("ระบบเวลา (ลาก CountdownTimer มาใส่ถ้ามี)")]
    public CountdownTimer matchTimer;

    private List<GameObject> activeCups = new List<GameObject>();
    private bool isShuffling = false;
    private bool canPick = false;
    private bool isGameActive = true;
    private string correctCupName;

    private void Start()
    {
        if (rounds.Length == 0) return;

        // 🌟 เริ่มเวลาและผูก Event (ถ้าเกมนี้มีเวลาจำกัด)
        if (matchTimer != null)
        {
            matchTimer.onTimeOut.AddListener(GameOver);
            matchTimer.StartTimer();
        }

        StartRound();
    }

    private void StartRound()
    {
        foreach (GameObject oldCup in activeCups) Destroy(oldCup);
        activeCups.Clear();

        CupRoundConfig currentConfig = rounds[currentRoundIndex];
        int correctCupIndex = Random.Range(0, currentConfig.numberOfCups);

        float totalWidth = (currentConfig.numberOfCups - 1) * cupSpacing;
        float startX = tableCenterPoint.position.x - (totalWidth / 2f);

        for (int i = 0; i < currentConfig.numberOfCups; i++)
        {
            Vector3 spawnPos = new Vector3(startX + (i * cupSpacing), tableCenterPoint.position.y, 0);
            GameObject newCup = Instantiate(cupPrefab, spawnPos, Quaternion.identity, transform);
            newCup.name = "BossCup_" + i;
            activeCups.Add(newCup);

            newCup.AddComponent<CupClickHandler>().manager = this;

            SpriteRenderer cupRenderer = newCup.GetComponent<SpriteRenderer>();
            if (cupRenderer != null) cupRenderer.color = cupColors[i % cupColors.Length];

            Transform dogTrans = newCup.transform.Find("Dog_Inside");
            if (dogTrans != null)
            {
                if (i == correctCupIndex)
                {
                    dogTrans.gameObject.SetActive(true);
                    correctCupName = newCup.name;
                }
                else dogTrans.gameObject.SetActive(false);
            }
        }

        StartCoroutine(PlayShuffleSequence(currentConfig));
    }

    private IEnumerator PlayShuffleSequence(CupRoundConfig config)
    {
        yield return new WaitForSeconds(config.showDogDuration);

        foreach (GameObject cup in activeCups)
        {
            Transform dogTrans = cup.transform.Find("Dog_Inside");
            if (dogTrans != null) dogTrans.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        isShuffling = true;
        for (int i = 0; i < config.shuffleAmount; i++) yield return StartCoroutine(ShuffleRoutine(config.shuffleSpeed));
        isShuffling = false;

        canPick = true;
    }

    private IEnumerator ShuffleRoutine(float speed)
    {
        int indexA = Random.Range(0, activeCups.Count);
        int indexB = Random.Range(0, activeCups.Count);
        while (indexA == indexB) indexB = Random.Range(0, activeCups.Count);

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
        if (!canPick || !isGameActive) return;
        canPick = false;

        clickedCup.transform.position += Vector3.up * 1.5f;
        Transform dogTrans = clickedCup.transform.Find("Dog_Inside");

        if (clickedCup.name == correctCupName)
        {
            if (dogTrans != null)
            {
                dogTrans.SetParent(null);
                dogTrans.gameObject.SetActive(true);
            }

            currentRoundIndex++;
            if (currentRoundIndex < rounds.Length) StartCoroutine(PrepareNextRound());
            else GameWin();
        }
        else
        {
            GameOver();
        }
    }

    private IEnumerator PrepareNextRound()
    {
        yield return new WaitForSeconds(2f);
        GameObject strayDog = GameObject.Find("Dog_Inside");
        if (strayDog != null) Destroy(strayDog);
        StartRound();
    }

    public void GameWin()
    {
        if (!isGameActive) return;
        isGameActive = false;
        if (matchTimer != null) matchTimer.StopTimer();

        SceneHandle currentScene = FindObjectOfType<SceneHandle>();
        if (currentScene != null && Gamemanager.Instance != null)
        {
            Gamemanager.Instance.ClearScene(currentScene.sceneObject.name);
            if (Gamemanager.Instance.dialogueUIManager != null)
            {
                Gamemanager.Instance.dialogueUIManager.OnShowDialog("dialog_found_all");
            }
        }
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;
        if (matchTimer != null) matchTimer.StopTimer();

        if (Gamemanager.Instance != null && Gamemanager.Instance.dialogueUIManager != null)
        {
            Gamemanager.Instance.dialogueUIManager.OnShowDialog("dialog_lose"); // 🌟 ใช้ Dialog เดียวกันหมด
        }
    }

    public class CupClickHandler : MonoBehaviour
    {
        public BossCupShuffleManager manager;
        private void OnMouseDown() { if (manager != null) manager.OnCupClicked(gameObject); }
    }
}