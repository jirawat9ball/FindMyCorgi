using UnityEngine;
using System.Collections;

public class BossMatchManager : MonoBehaviour
{
    [Header("เชื่อมต่อระบบอื่นๆ")]
    public BossHealth bossHealth;
    public CountdownTimer matchTimer;

    [Header("ตั้งค่าการเล่น")]
    public float timePenaltyOnFail = 5f;

    public Dog firstSelectedDog = null;
    private Dog secondSelectedDog = null;
    private bool isCheckingMatch = false;
    private bool isGameActive = true;

    private void Start()
    {
        // 🌟 เริ่มเวลาและผูก Event เวลาหมด
        if (matchTimer != null)
        {
            matchTimer.onTimeOut.AddListener(GameOver);
            matchTimer.StartTimer();
        }
    }

    private void Update()
    {
        if (!isGameActive) return;
        if (firstSelectedDog != null) HighlightDog(firstSelectedDog, true);
        if (secondSelectedDog != null) HighlightDog(secondSelectedDog, true);
    }

    public void SelectDog(Dog clickedDog)
    {
        if (!isGameActive || isCheckingMatch) return;
        if (clickedDog.currentState == DogState.Found || clickedDog == firstSelectedDog) return;

        if (firstSelectedDog == null)
        {
            firstSelectedDog = clickedDog;
            HighlightDog(firstSelectedDog, true);
            return;
        }
        StartCoroutine(CheckMatchRoutine(clickedDog));
    }

    private IEnumerator CheckMatchRoutine(Dog secondDog)
    {
        isCheckingMatch = true;
        secondSelectedDog = secondDog;
        HighlightDog(secondDog, true);

        yield return new WaitForSeconds(0.4f);

        if (firstSelectedDog.id == secondDog.id && !string.IsNullOrEmpty(secondDog.id)) MatchSuccess(secondDog);
        else MatchFail(secondDog);

        isCheckingMatch = false;
    }

    private void MatchSuccess(Dog secondDog)
    {
        Dog tempFirst = firstSelectedDog;
        Dog tempSecond = secondSelectedDog;

        firstSelectedDog = null;
        secondSelectedDog = null;

        HighlightDog(tempFirst, false);
        HighlightDog(tempSecond, false);

        tempFirst.ChangeState(DogState.Found);
        tempSecond.ChangeState(DogState.Found);

        if (bossHealth != null) bossHealth.TakeDamage(20);
    }

    private void MatchFail(Dog secondDog)
    {
        Dog tempFirst = firstSelectedDog;
        Dog tempSecond = secondSelectedDog;

        firstSelectedDog = null;
        secondSelectedDog = null;

        HighlightDog(tempFirst, false);
        HighlightDog(tempSecond, false);

        if (matchTimer != null) matchTimer.ReduceTime(timePenaltyOnFail);
    }

    private void HighlightDog(Dog dog, bool isHighlight)
    {
        if (dog == null) return;
        SpriteRenderer sr = dog.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = isHighlight ? Color.yellow : Color.white;
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
}