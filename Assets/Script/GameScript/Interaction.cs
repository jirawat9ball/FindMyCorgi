using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;

[RequireComponent(typeof(Collider2D))]
public abstract class Interaction : MonoBehaviour
{

    Sprite spriteOutline;

    public KeyItem NeedKey;
    [Tooltip("ใส่ Key จากไฟล์ภาษา เช่น desc_box_locked")]
    public string Description;
    public AudioClip clip;

    BoxCollider2D _boxCollider;
    protected BoxCollider2D boxCollider
    {
        get
        {
            if (_boxCollider == null)
            {
                _boxCollider = GetComponent<BoxCollider2D>();
            }
            return _boxCollider;
        }
    }
    SpriteRenderer _spriteRenderer;
    protected SpriteRenderer spriteRenderer
    {
        get { 
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            return _spriteRenderer;
        }
    }
    [UnityEngine.Serialization.FormerlySerializedAs("onComplate")]
    public UnityEvent onComplete; // Event ที่จะถูกเรียกเมื่อพบไอเท็ม
    public UnityEvent onFail; //Event ที่จะถูกเรียกเมื่อคลิกไอเท็ม
    protected virtual void Awake()
    {
        spriteOutline = spriteRenderer.sprite;  
    }
    protected virtual void Start()
    {
        

    }


    protected void isLock()
    {
        if (BlockCheck())
        {
            Debug.Log("Clicked Item " + gameObject.name);
            DoOnLock();
            return;
        }
        onFail?.Invoke(); // เรียก Event เมื่อคลิกไอเท็ม
        Debug.Log("Clicked Item " + gameObject.name);
    }

    protected virtual bool BlockCheck() {

        bool isblock = false;
        if (NeedKey == null)
        {
            Debug.Log("do not need key");
            isblock = false;
        }
        else {
            Debug.Log("need key " + NeedKey.KeyName);
            isblock = !Gamemanager.Instance.IsHasKey(NeedKey);
            Debug.Log(isblock); 
        }
        return isblock;
    }
    protected virtual void DoOnLock() { 
        
    }

    protected virtual void OnUnlock() { 
    
    }

    public virtual void DoneState() { 
        
    }

    protected virtual void OnMouseDown()
    {

        if (!Gamemanager.Instance.isStateGamePlay())
        {
            return;
        }
        if (NeedKey != null) {
            if (Gamemanager.Instance.IsHasKey(NeedKey))
            {
                Gamemanager.Instance.dialogueUIManager.OnShowDialog("dialog_unlock_success");
                onComplete?.Invoke();
            }
            else
            {
                Gamemanager.Instance.dialogueUIManager.OnShowDialog(Description);
                onFail?.Invoke();
            }
        }
    }

    private void OnMouseEnter()
    {
        if (!Gamemanager.Instance.isStateGamePlay())
        {
            return;
        }
        // ถ้าของชิ้นนี้ต้องการกุญแจ (NeedKey ไม่เป็นความว่างเปล่า)
        if (NeedKey != null)
        {
            // 🌟 เช็คว่าในกระเป๋า (Collectable) มีกุญแจดอกนี้หรือยัง?
            if (Gamemanager.Instance.IsHasKey(NeedKey))
            {
                // ถ้ามีแล้ว -> แสดงไอคอนกุญแจจริงๆ
                CursorHandle.Instance.SetItemCursor(NeedKey.Cursericon);
            }
            else
            {
                // ถ้ายังไม่มี -> ดึงชุดภาพ investigateCursorTextures จาก CursorHandle มาแสดงแทน
                CursorHandle.Instance.SetItemCursor(CursorHandle.Instance.investigateCursorTextures);
            }
        }
    }

    private void OnMouseExit()
    {
        if (NeedKey != null)
        {
            CursorHandle.Instance.ResetToBaseCursor();
        }
    }

}