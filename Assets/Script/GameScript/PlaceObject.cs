using Unity.VisualScripting;
using UnityEngine;

public class PlaceObject : Interaction
{
    public Sprite sprite;
    protected override void Start()
    {
        base.Start();
        spriteRenderer.enabled = false;
        sprite = NeedKey.Image;
        //texture2D = Utility.ToTexture2D(NeedKey.Image);
    }
    protected override void OnMouseDown()
    {
       
    }
}
