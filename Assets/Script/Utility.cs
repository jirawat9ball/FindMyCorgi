using UnityEngine;

public static class Utility
{
    public static Texture2D ToTexture2D(this Sprite sprite)
    {
        if (sprite == null)
            return null;

        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        Color[] pixels = sprite.texture.GetPixels(
            (int)sprite.rect.x,
            (int)sprite.rect.y,
            (int)sprite.rect.width,
            (int)sprite.rect.height
        );
        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }
    public static Sprite ToSprite(this Texture2D texture, float pixelsPerUnit = 100f)
    {
        if (texture == null)
            return null;

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
}
  
