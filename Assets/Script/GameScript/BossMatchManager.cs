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

    private void Start()
    {
        if (matchTimer != null) matchTimer.StartTimer();
    }

    private void Update()
    {
        // บังคับล็อคสีเหลืองทุกเฟรม ป้องกันสีหายตอนเลื่อนเมาส์
        if (firstSelectedDog != null) HighlightDog(firstSelectedDog, true);
        if (secondSelectedDog != null) HighlightDog(secondSelectedDog, true);
    }

    public void SelectDog(Dog clickedDog)
    {
        if (isCheckingMatch) return;
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

        if (firstSelectedDog.id == secondDog.id && !string.IsNullOrEmpty(secondDog.id))
        {
            MatchSuccess(secondDog);
        }
        else
        {
            MatchFail(secondDog);
        }

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

        // 🌟 โยนดาเมจให้บอสเลย ไม่ต้องมานั่งนับคู่แล้ว
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
        if (sr != null)
        {
            sr.color = isHighlight ? Color.yellow : Color.white;
        }
    }

    // ===================================================
    // 🌟 ฟังก์ชัน ชนะ/แพ้ รอรับคำสั่งจาก Event ของบอสและเวลา
    // ===================================================

    public void GameWin()
    {
        Debug.Log("👑 บอสตายแล้ว! ชนะด่าน!");
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
        Debug.Log("💀 เวลาหมด! แพ้แล้ว!");
    }
}