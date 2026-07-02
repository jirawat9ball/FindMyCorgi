using UnityEngine;

public class ZoneHandle : MonoBehaviour
{
    public SceneHandle[] sceneHandles;
    public SceneHandle currentScene;
    public int indexScene = 0; 
    
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
        SceneObject targetSceneObj = Gamemanager.Instance.sceneObject;

        if (targetSceneObj == null)
        {
            if (sceneHandles != null && indexScene < sceneHandles.Length)
            {
                targetSceneObj = sceneHandles[indexScene].sceneObject;
                Gamemanager.Instance.sceneObject = targetSceneObj;
                Gamemanager.Instance.SetStateGamePlay(); 

                Debug.Log($"🛠️ [Gamemanager Sync] ไม่พบข้อมูลฉาก ระบบจึงดึงฉาก Index {indexScene} มาใส่ใน Gamemanager ให้แล้วครับ");
            }
            else
            {
                Debug.LogError("⚠️ หาฉากเริ่มต้นไม่เจอ! ตรวจสอบช่อง sceneHandles หรือค่า indexScene ใน Inspector ด้วยครับ");
                return;
            }
        }

        SceneHandle sceneHandle = GetScene(targetSceneObj);

        if (sceneHandle != null)
        {
            currentScene = sceneHandle; 
            sceneHandle.Setup();        
            sceneHandle.SetToGamemanager(); 
        }
        else
        {
            Debug.LogError($"⚠️ ไม่พบ SceneHandle สำหรับ '{targetSceneObj.name}' ในโซนนี้! ตรวจสอบว่าลาก SceneHandle ใส่ Array หรือยัง");
        }
    }

    public void BackScene()
    {
        // 🌟 ดักกัน Error ถ้าเกิดไม่มีฉากให้กลับ
        if (currentScene != null && currentScene.sceneObject.backScene != null)
        {
            GotoScene(currentScene.sceneObject.backScene);
        }
    }

    void GotoScene(SceneObject sceneObject)
    {
        SceneHandle sceneHandle = GetScene(sceneObject);
        if (sceneHandle != null)
        {
            // ==========================================
            // 🌟 แก้ไข: เพิ่มการสั่ง Setup ฉากใหม่ และจำว่าตอนนี้อยู่ห้องไหน
            // ==========================================
            currentScene = sceneHandle;     // 1. อัปเดตให้โซนรู้ว่าผู้เล่นย้ายมาห้องนี้แล้ว
            sceneHandle.Setup();            // 2. สั่งรัน Setup (สร้างแผ่นฟิล์ม, เปลี่ยนเพลง, นับหมาใหม่)
            sceneHandle.SetToGamemanager(); // 3. ส่งข้อมูลให้กล้องตามปกติ
        }
    }

    SceneHandle GetScene(SceneObject _sceneObject)
    {
        if (sceneHandles == null) return null;

        for (int i = 0; i < sceneHandles.Length; i++)
        {
            // 🌟 ดักเช็ค null ป้องกัน Error กรณีลืมใส่ฉากใน Array
            if (sceneHandles[i] != null && sceneHandles[i].sceneObject == _sceneObject)
            {
                return sceneHandles[i];
            }
        }
        return null;
    }
}