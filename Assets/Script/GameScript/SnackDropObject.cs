using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SnackDropObject : Interaction
{
    [Header("การตั้งค่าอนิเมชันตอนดรอป")]
    public float popDuration = 0.5f; // เวลาที่ใช้กระเด้งพื้นจนจบ
    public float jumpHeight = 1f; // ความสูงตอนลอยกระเด้ง
    public float tossSideRange = 1.5f; // ตีวงกว้างสุดที่มันจะกระเด็นไปด้านข้าง

    private Vector3 originalScale;

    protected override void Start()
    {
        base.Start();

        // 🌟 ดันเลเยอร์ภาพขนมให้อยู่ระดับสูงที่สุด (Level 100) จะได้ไม่โดนสุนัขหรือต้นไม้บัง
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 100;
        }

        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        
        SoundManager.Instance.PlayOnClickSound();

        // 🌟 รันการสุ่มกระเด้งไปด้านข้าง
        StartCoroutine(PopAndBounceAnimation());
    }

    private IEnumerator PopAndBounceAnimation()
    {
        Vector3 startPos = transform.position;
        
        // สุ่มทิศทางการกระเด็น (ซ้ายหรือขวา) ห้ามตกหล่นอยู่ตรงกลางพอดี
        float randomX = Random.Range(-tossSideRange, tossSideRange);
        if (Mathf.Abs(randomX) < 0.5f) randomX = randomX >= 0 ? 0.7f : -0.7f; 
        
        // 🌟 เพิ่มการสุ่มหมุนเคว้ง (Spin) ตอนลอยกลางอากาศ
        float randomSpin = Random.Range(180f, 360f);
        if (randomX < 0) randomSpin = -randomSpin; // ถ้ากระเด็นซ้าย ให้หมุนทวนเข็ม ซ้าย
        
        // จุดตกปลายทาง (สุ่มขยับแกน Y เล็กน้อยเพื่อความเป็นธรรมชาติ)
        Vector3 endPos = startPos + new Vector3(randomX, Random.Range(-0.5f, 0.5f), 0);

        float timer = 0;
        while (timer < popDuration)
        {
            float t = timer / popDuration; // t มีค่าตั้งแต่ 0 ถึง 1

            // 1. เด้งขยายขนาดอย่างรวดเร็วในช่วงครึ่งแรก
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, Mathf.Clamp01(t * 1.5f));

            // 2. เคลื่อนย้ายตำแหน่งไปด้านข้าง พร้อมตีโค้งรูปพาราโบล่าในแนวตั้ง
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight; // สูตรโค้งขึ้นและตกลงมา
            transform.position = currentPos;

            // 3. หมุนตีลังกากลางอากาศ
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, randomSpin, t));

            timer += Time.deltaTime;
            yield return null;
        }

        // จัดทรงให้สมบูรณ์ตอนจบ
        transform.localScale = originalScale;
        transform.position = endPos;
    }

    protected override void OnMouseDown()
    {
        if (!Gamemanager.Instance.isStateGamePlay()) return;

        // 🌟 เพิ่มจำนวน Snack (แอบใช้ฟังก์ชันเดียวกับดูโฆษณาไปก่อน)
        Gamemanager.Instance.AddSnackFromAd(1);
        
        // 🌟 เสียงตอนเก็บ
        SoundManager.Instance.PlayOnClickSound();

        // 🌟 ทำให้หายไป
        Destroy(gameObject);
    }
}
