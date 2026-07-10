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
public enum DogTypeRequirement
{
    Any,            // นับรวมทุกประเภท 
    NormalOnly,     // นับเฉพาะหมาปกติ
    SpecialOnly,    // นับเฉพาะหมาพิเศษ
    RealDogOnly     // ให้รางวัลทันทีที่เจอหมาจริง (ไม่สนจำนวน)
}

[System.Serializable]
public class RewardSet {
    public KeyItem KeyItemInThisScene;
    
    [Tooltip("ประเภทของหมาที่ต้องการให้เป็นเงื่อนไข")]
    public DogTypeRequirement dogTypeRequirement = DogTypeRequirement.Any;
    
    [Tooltip("จำนวนที่ต้องหาเจอ (สำหรับ Any, NormalOnly, SpecialOnly)")]
    public int AmontDogtoUnlockKeyItem = 10;
}