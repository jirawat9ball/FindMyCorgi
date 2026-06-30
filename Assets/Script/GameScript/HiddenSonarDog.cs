using UnityEngine;
using System.Collections;

public class HiddenSonarDog : MonoBehaviour
{
    [Header("ตั้งค่าการแสดงตัว")]
    public float fadeSpeed = 3f; // ความเร็วในการเฟดโผล่และเฟดหาย
    public float visibleDuration = 1.5f; // 🌟 เวลาที่จะโชว์ตัวให้ผู้เล่นคลิกก่อนจะจางหายไป

    private SpriteRenderer spriteRenderer;
    private bool isRevealed = false;
    private bool isCaptured = false;
    private BossSonarManager manager;
    private Coroutine revealCoroutine; // 🌟 ตัวแปรเก็บสถานะการโชว์ตัว เพื่อเอาไว้สั่งหยุดตอนโดนคลิก

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        manager = FindObjectOfType<BossSonarManager>();

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0f;
            spriteRenderer.color = c;
        }
    }

    public void OnHitBySonar()
    {
        // ถ้ายังไม่โผล่ และยังไม่ถูกจับ ถึงจะเริ่มโชว์ตัว
        if (!isRevealed && !isCaptured)
        {
            isRevealed = true;

            // ถ้ามี Coroutine เดิมค้างอยู่ให้หยุดก่อน (กันบั๊ก)
            if (revealCoroutine != null) StopCoroutine(revealCoroutine);

            // เริ่มการ โชว์ตัว -> รอเวลา -> จางหาย
            revealCoroutine = StartCoroutine(RevealAndHideRoutine());
        }
    }

    private IEnumerator RevealAndHideRoutine()
    {
        Color c = spriteRenderer.color;

        // 1. ค่อยๆ เฟดสว่างขึ้นมา
        while (c.a < 1f)
        {
            c.a += fadeSpeed * Time.deltaTime;
            spriteRenderer.color = c;
            yield return null;
        }
        c.a = 1f;
        spriteRenderer.color = c;

        // 2. 🌟 รอเวลาให้ผู้เล่นรีบตัดสินใจคลิก (ตามเวลา visibleDuration)
        yield return new WaitForSeconds(visibleDuration);

        // 3. 🌟 ถ้าผ่านไปแล้วยังไม่โดนคลิกจับ ค่อยๆ เฟดจางหายไป
        while (c.a > 0f)
        {
            c.a -= fadeSpeed * Time.deltaTime;
            spriteRenderer.color = c;
            yield return null;
        }
        c.a = 0f;
        spriteRenderer.color = c;

        // 4. 🌟 รีเซ็ตสถานะ ปลดล็อกให้โดนเรดาร์กวาดเจอใหม่ได้อีกครั้ง
        isRevealed = false;
    }

    private void OnMouseDown()
    {
        // ถ้าโผล่มาแล้ว และ "ยังไม่เคยโดนจับ" 
        if (isRevealed && !isCaptured && manager != null)
        {
            isCaptured = true;
            manager.CaptureDog(gameObject);

            // 🌟 สำคัญมาก: สั่งหยุด Coroutine ที่กำลังนับเวลาหรือกำลังทำเฟดหายไปทันที!
            if (revealCoroutine != null) StopCoroutine(revealCoroutine);

            // บังคับให้สีสว่างเต็ม 100% ค้างไว้ตลอดกาล (Found Stage แบบปกติ)
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }
        }
    }
}