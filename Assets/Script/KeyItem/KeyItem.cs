using UnityEngine;

public enum TypeState {
    UnDiscovered, Discovered, Used
}

[CreateAssetMenu(fileName = "KeyItem", menuName = "item/KeyItem", order = 1)]
public class KeyItem : ScriptableObject
{
    public string KeyName;
    public string KeyInfomation;
    public Sprite Image;
    public Texture2D[] Cursericon;
    public Sprite imageShowGotItem;
    public bool isStone;
    public string DialogueWhenNeedItem;
}
