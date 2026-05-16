using UnityEngine;

public class UIMapScene : MonoBehaviour
{
    public GameObject[] parrent;

    public void SetAllDisable() {
        foreach (GameObject go in parrent) { 
            go.SetActive(false);
        }
    }
    public void UnlockSceneto(int unlockscene) {
        for (int i = 0; i < unlockscene; i++) {
            parrent[i].SetActive(false);
        }
    }
    public void SetActive(int i) {
        parrent[i].SetActive(true);
    }
}
