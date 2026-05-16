using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Gate : Interaction
{
    [Header("Gate setUp")]
    public SceneHandle GotoScene;
    public Obstacle obstacle;
    public bool GateIslock = true;

    private bool isProcessing = false; // 🌟 State ป้องกันการกดทำงานซ้อนทับกัน
    protected override void Start()
    {
        base.Start();

        // 🌟 ถ้าปลายทางเคยถูกเซฟว่าปลดล็อคแล้ว ให้ Gate นี้เปิดตัวเองอัตโนมัติตั้งแต่เริ่ม
        if (GotoScene != null && GotoScene.sceneObject != null)
        {
            if (Gamemanager.Instance.IsSceneUnlocked(GotoScene.sceneObject.name))
            {
                GateIslock = false;
                obstacle.DoneState();
                DoneState(); // 🌟 เรียก State Done เพื่อให้ระบบอื่นๆ อัปเดต (เช่นเปลี่ยนภาพประตูเป็นเปิดแล้ว)
            }
        }
    }
    public void SetlockGate(bool t) {
        GateIslock = t;
    }
    protected override void OnMouseDown()
    {
        // 🌟 ถ้าระบบกำลังโหลดหรือทำทรานสิชันอยู่ จะอยู่ใน State Processing ไม่ให้เข้าซ้ำ
        if (isProcessing) return;

        // 🌟 ป้องกันการกดปุ่มรัวๆ (Global Spam Check) 
        // ถ้ากำลังอยู่ในช่วงทรานสิชัน (เลื่อนจอภาพ) ไม่อนุญาตให้กดซ้ำเด็ดขาด
        if (!LoadSceneManager.Instance.isReady) return;

        Debug.Log("OnMouseDown" + GotoScene);
        if (!Gamemanager.Instance.isStateGamePlay()) { 
            return;
        }
        if (NeedKey != null)
        {
            GateIslock = BlockCheck();
        }
        if (GateIslock)
        {
            Debug.Log("Clicked Item " + gameObject.name);
            DoOnLock();
        }
        else {
            isProcessing = true; // 🌟 ปรับ State ให้รู้ว่ากำลังทำงานอยู่
            StartCoroutine(EnumGotoScene());
            Debug.Log("go to scene " + GotoScene);
        }
    }
    IEnumerator EnumGotoScene() {

        // 🌟 ปลดล็อคและเซฟด่านปลายทาง (ถ้าผู้เล่นได้เดินเข้า Gate นี้ถือว่ามีสิทธิ์เข้าด่านนั้นแล้ว)
        if (GotoScene != null && GotoScene.sceneObject != null)
        {
            Gamemanager.Instance.UnlockScene(GotoScene.sceneObject.name);
        }

        yield return StartCoroutine(LoadSceneManager.Instance.LocalTransitionRoutine(() =>
        {
            GotoScene.Setup();
            GotoScene.SetToGamemanager();
        }));
     
        // 🌟 เลิกล็อค State เมื่อโหลดทุกอย่างเสร็จสิ้น
        isProcessing = false;
    }

}
