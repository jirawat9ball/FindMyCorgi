using UnityEngine;

public class ClickObject : Obstacle
{
    [Header("Click Object setUp")]
    [Tooltip("Set null to for disappear")]
    public Sprite[] spriteAfterClick;
    public int RequiredClick = 1;
    int countClick = 0;
    int FailedClick = 0;
    protected override void OnMouseDown()
    {
        if (!Gamemanager.Instance.isStateGamePlay())
        {
            return;
        }

        // 🌟 ถ้าเงื่อนไขนี้คือ "ต้องใช้กุญแจ" และ "ผู้เล่นยังไม่มีกุญแจนั้น" -> จะทำงานท่อน Fail
        if (NeedKey != null && !Gamemanager.Instance.IsHasKey(NeedKey))
        {
            onFail?.Invoke();
            SoundManager.Instance.PlayOnClickSound();
            FailedClick++;
            if (FailedClick >= 3)
            {
                FailedClick = 0; // Reset the failed click count
                Gamemanager.Instance.dialogueUIManager.OnShowDialog(NeedKey.DialogueWhenNeedItem);
            }
            return; // แจ้งเตือนเสร็จให้เด้งออกไปเลย ไม่ต้องนับคลิกผ่าน
        }

        // 🌟 ถ้าหลุดมาถึงตรงนี้ได้ แปลว่า "ต้องการกุญแจและมีแล้ว" หรือ "ไม่ต้องใช้กุญแจเลย" ก็จะให้คลิกผ่านได้ปกติ
        countClick++;
        ChangeSprite();
        
        if (countClick >= RequiredClick)
        {
            DoneState();
        }
    }

    private void ChangeSprite()
    {
        // 🌟 แก้ไข Logic การเปลี่ยนรูปภาพ
        if (spriteAfterClick.Length > 0)
        {
            // คำนวณ Index โดยใช้สัดส่วนการคลิกเทียบกับจำนวนรูปที่มี
            // ใช้ Mathf.Min เพื่อป้องกันไม่ให้ Index เกินขนาดของ Array
            float progress = (float)countClick / RequiredClick;
            int index = Mathf.Min(Mathf.FloorToInt(progress * spriteAfterClick.Length), spriteAfterClick.Length - 1);

            spriteRenderer.sprite = spriteAfterClick[index];
        }
    }

    public override void DoneState()
    {
        base.DoneState();

        if (spriteAfterClick != null && spriteAfterClick.Length > 0)
        {
            // แบบปกติ: เปลี่ยนรูปไปเรื่อยๆ
            int index = spriteAfterClick.Length - 1;
            spriteRenderer.sprite = spriteAfterClick[index];
        }
        //else
        //{
        //    // 🌟 แบบโหมด _00 (Array ว่างเปล่า): สั่งปิด gameObject ทิ้งไปเลย ทั้งภาพและที่คลิกจะหายไป!
        //    gameObject.SetActive(false);
        //}

        boxCollider.enabled = false;
        onComplete?.Invoke();
        Debug.Log("Play Partical");
        Debug.Log("Play Sound");
        //Destroy(gameObject);
    }
}
