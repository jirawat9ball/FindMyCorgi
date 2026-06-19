using UnityEngine;

public class AutoTint : MonoBehaviour
{
    public SpriteRenderer mySprite; // ลาก Sprite Renderer ของตัวขนมมาใส่

    [Header("ความเนียน (1 = กลืนหายไปเลย, 0.5 = เด่นครึ่งนึง)")]
    [Range(0f, 1f)]
    public float blendStrength = 0.6f;

    void Start()
    {
        if (mySprite == null) mySprite = GetComponent<SpriteRenderer>();

        //ค้นหาภาพฉากหลังในด่าน (ต้องไปตั้ง Tag ให้ฉากหลังว่า "Background")
        GameObject bgObject = GameObject.FindGameObjectWithTag("Background");

        if (bgObject != null)
        {
            SpriteRenderer bgRenderer = bgObject.GetComponent<SpriteRenderer>();
            if (bgRenderer != null && bgRenderer.sprite != null)
            {
                // เรียกฟังก์ชันดูดสีจากตำแหน่งที่ขนมเกิด
                Color targetColor = GetColorFromBackground(bgRenderer, transform.position);

                // เอาสีที่ดูดมา ผสมกับสีขาวของขนม
                mySprite.color = Color.Lerp(Color.white, targetColor, blendStrength);
            }
        }
    }

    // ฟังก์ชันคำนวณตำแหน่งและดูดสี
    private Color GetColorFromBackground(SpriteRenderer bg, Vector3 worldPos)
    {
        Sprite bgSprite = bg.sprite;
        Texture2D tex = bgSprite.texture;

        // แปลงตำแหน่งของขนมในโลก ให้กลายเป็นตำแหน่งพิกัดบนรูปภาพพื้นหลัง
        Vector3 localPos = bg.transform.InverseTransformPoint(worldPos);

        // คำนวณหาว่าตรงกับพิกเซลที่เท่าไหร่ของรูป (แกน X และ Y)
        float pixelX = (localPos.x - bgSprite.bounds.min.x) * bgSprite.pixelsPerUnit;
        float pixelY = (localPos.y - bgSprite.bounds.min.y) * bgSprite.pixelsPerUnit;

        // ดูดสีจากพิกเซลนั้นออกมา!
        return tex.GetPixel(Mathf.RoundToInt(pixelX), Mathf.RoundToInt(pixelY));
    }
}