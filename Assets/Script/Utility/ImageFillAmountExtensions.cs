using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class ImageFillAmountExtensions
{
    public static IEnumerator TweenFillAmount(this Image image, float targetFillAmount, float duration)
    {
        if (image == null) yield break; // Safety check

        float startFillAmount = image.fillAmount;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            image.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, time / duration);
            yield return null;
        }

        image.fillAmount = targetFillAmount; // Ensure final fill amount is set precisely.
    }
}