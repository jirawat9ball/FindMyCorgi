using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossSonarManager : MonoBehaviour
{
    [Header("ตั้งค่าตัวคลื่นโซน่าร์ (Sonar)")]
    public GameObject sonarVisualPrefab;
    public float maxSonarRadius = 8f;
    public float fadeSpeed = 2f;
    public float sonarCooldown = 3f;

    [Header("ระบบเวลา (ลาก CountdownTimer มาใส่)")]
    public CountdownTimer matchTimer;

    private bool canUseSonar = true;
    private bool isGameActive = false;
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

        GameObject[] dogs = GameObject.FindGameObjectsWithTag("Dog");
        foreach (GameObject dog in dogs)
        {
            if (dog.GetComponent<HiddenSonarDog>() == null) dog.AddComponent<HiddenSonarDog>();
            remainingDogs.Add(dog);
        }

        isGameActive = true;

        // 🌟 เริ่มเวลาและผูก Event
        if (matchTimer != null)
        {
            matchTimer.onTimeOut.AddListener(GameOver);
            matchTimer.StartTimer();
        }

        Debug.Log($"🔊 [ระบบ] เริ่มเกม! มีหมาซ่อนอยู่ {remainingDogs.Count} ตัว");
    }

    private void Update()
    {
        if (!isGameActive || !Gamemanager.Instance.isStateGamePlay()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (canUseSonar)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0f;
                StartCoroutine(SonarPulseRoutine(mousePos));
            }
        }
    }

    private IEnumerator SonarPulseRoutine(Vector3 originPos)
    {
        if (sonarVisualPrefab == null) yield break;
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

        StartCoroutine(CooldownRoutine());

        while (c.a > 0)
        {
            c.a -= fadeSpeed * Time.deltaTime;
            sr.color = c;
            yield return null;
        }

        activeGizmos.Remove(gizmoData);
        Destroy(sonarObj);
    }

    private IEnumerator CooldownRoutine()
    {
        float timer = sonarCooldown;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        canUseSonar = true;
    }

    public void CaptureDog(GameObject dog)
    {
        if (remainingDogs.Contains(dog))
        {
            remainingDogs.Remove(dog);
            if (remainingDogs.Count == 0) GameWin();
        }
    }

    public void GameWin()
    {
        if (!isGameActive) return;
        isGameActive = false;
        if (matchTimer != null) matchTimer.StopTimer();

        if (currentScene != null && Gamemanager.Instance != null)
        {
            Gamemanager.Instance.ClearScene(currentScene.sceneObject.name);
            if (Gamemanager.Instance.dialogueUIManager != null)
            {
                UIManager.Instance.ShowDialog("dialog_found_all");
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
            UIManager.Instance.ShowDialog("dialog_lose");
        }
    }

    private void OnDrawGizmos()
    {
        if (activeGizmos == null) return;
        Gizmos.color = Color.yellow;
        foreach (SonarGizmoData data in activeGizmos) Gizmos.DrawWireSphere(data.position, data.radius);
    }
}