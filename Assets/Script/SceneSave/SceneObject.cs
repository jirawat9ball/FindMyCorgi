using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "SceneSave", menuName = "Scriptable Objects/SceneSave")]
public class SceneObject : ScriptableObject
{
    public string Zone;
    public string SceneName;
    public AudioClip soundBG;
    public SceneObject backScene;
    public RewardSet[] rewardSets;
}
[System.Serializable]
public class RewardSet {
    public KeyItem KeyItemInThisScene;
    public int AmontDogtoUnlockKeyItem = 10;
}