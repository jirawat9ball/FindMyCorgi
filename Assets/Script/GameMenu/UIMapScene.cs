using UnityEngine;
using UnityEngine.UI;

public class UIMapScene : MonoBehaviour
{
    public GameObject[] parrent;

    public void SetAllDisable()
    {
        foreach (GameObject go in parrent)
        {
            SetNodeState(go, false);
        }
    }

    public void UnlockSceneto(int unlockscene)
    {
        for (int i = 0; i < unlockscene; i++)
        {
            SetNodeState(parrent[i], false);
        }
    }

    public void SetActive(int i)
    {
        SetNodeState(parrent[i], true);
    }

    // 🌟 เปลี่ยนใหม่: จัดการโชว์/ซ่อน ด่านย่อยแทนการเปลี่ยนสี
    public void SetNodeState(GameObject node, bool isUnlocked)
    {
        if (node == null) return;

        // ถ้าปลดล็อกแล้ว (true) ให้เปิดโชว์ 
        // ถ้ายังไม่ปลดล็อก (false) ให้ปิดซ่อนไปเลย (ไม่ต้องโชว์)
        node.SetActive(isUnlocked);
    }
}