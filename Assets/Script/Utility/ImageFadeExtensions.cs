using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class ImageFadeExtensions
{
    public static IEnumerator FadeIn(this Image image, float duration)
    {
        return image.Fade(0f, 0.9f, duration);
    }

    public static IEnumerator FadeOut(this Image image, float duration)
    {
        return image.Fade(0.9f, 0f, duration);
    }

    public static IEnumerator Fade(this Image image, float startAlpha, float endAlpha, float duration)
    {
        if (image == null) yield break; // Safety check

        float time = 0;
        Color color = image.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            image.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        image.color = new Color(color.r, color.g, color.b, endAlpha); // Ensure final alpha is set precisely.
    }
}
