using UnityEngine;

public class ZoneHandle : MonoBehaviour
{
    public SceneHandle[] sceneHandles;
    public SceneHandle currentScene;
    public int indexScene = 0; // เพิ่มตัวแปร indexScene เพื่อเก็บค่า Index ของฉากเริ่มต้น
    private void Awake()
    {
        Gamemanager.Instance.currentZone = this;

        if (sceneHandles != null)
        {
            foreach (var item in sceneHandles)
            {
                if (item != null)
                {
                    item.setZone(this);
                }
            }
        }

        GotoScene();
    }

    void GotoScene()
    {
        // 🌟 1. พยายามอ่านค่าเป้าหมายจาก Gamemanager ก่อนเป็นอันดับแรก
        SceneObject targetSceneObj = Gamemanager.Instance.sceneObject;

        // 🌟 2. ถ้าใน Gamemanager ยังไม่มีข้อมูล (กรณี Test Mode หรือกด Play ในฉากนี้ตรงๆ)
        if (targetSceneObj == null)
        {
            if (sceneHandles != null && indexScene < sceneHandles.Length)
            {
                // ดึงฉากเริ่มต้นจาก Index ที่เราตั้งไว้ใน Inspector
                targetSceneObj = sceneHandles[indexScene].sceneObject;

                // บันทึกกลับไปที่ Gamemanager เพื่อให้ทุกระบบรับรู้ตรงกัน
                Gamemanager.Instance.sceneObject = targetSceneObj;
                Gamemanager.Instance.SetStateGamePlay(); // บังคับเริ่มสถานะ Gameplay

                Debug.Log($"🛠️ [Gamemanager Sync] ไม่พบข้อมูลฉาก ระบบจึงดึงฉาก Index {indexScene} มาใส่ใน Gamemanager ให้แล้วครับ");
            }
            else
            {
                Debug.LogError("⚠️ หาฉากเริ่มต้นไม่เจอ! ตรวจสอบช่อง sceneHandles หรือค่า indexScene ใน Inspector ด้วยครับ");
                return;
            }
        }

        // 🌟 3. ตอนนี้เรามีข้อมูลใน targetSceneObj (ที่อ่านมาจาก Manager) แล้ว
        // จึงทำการค้นหา SceneHandle ที่ตรงกับข้อมูลนั้น
        SceneHandle sceneHandle = GetScene(targetSceneObj);

        if (sceneHandle != null)
        {
            currentScene = sceneHandle; // อัปเดตฉากปัจจุบันของโซน
            sceneHandle.Setup();        // เตรียมความพร้อมสคริปต์ต่างๆ ในฉาก
            sceneHandle.SetToGamemanager(); // สั่งให้ฉากนั้นไปจัดการกล้องและ UI ต่อ
        }
        else
        {
            Debug.LogError($"⚠️ ไม่พบ SceneHandle สำหรับ '{targetSceneObj.name}' ในโซนนี้! ตรวจสอบว่าลาก SceneHandle ใส่ Array หรือยัง");
        }
    }

    public void BackScene()
    {
        GotoScene(currentScene.sceneObject.backScene);
    }

    void GotoScene(SceneObject sceneObject)
    {
        SceneHandle sceneHandle = GetScene(sceneObject);
        if (sceneHandle != null)
        {
            sceneHandle.SetToGamemanager();
        }
    }

    SceneHandle GetScene(SceneObject _sceneObject)
    {
        for (int i = 0; i < sceneHandles.Length; i++)
        {
            if (sceneHandles[i].sceneObject == _sceneObject)
            {
                return sceneHandles[i];
            }
        }
        return null;
    }
}