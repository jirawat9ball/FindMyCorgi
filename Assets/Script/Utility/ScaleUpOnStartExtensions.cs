using UnityEngine;
using System.Collections;

public static class ScaleUpOnStartExtensions
{
    public static IEnumerator ScaleUp(this Transform transform, Vector3 startScale, Vector3 targetScale, float duration, AnimationCurve scaleCurve , System.Action onComplete = null)
    {
        float timer = 0f;

        while (timer < duration)
        {
            float normalizedTime = (duration > 0) ? timer / duration : 1f;
            float curveValue = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale; // Ensure final scale is exact.

        onComplete?.Invoke(); // Invoke the optional completion callback.
    }

    public static IEnumerator ScaleDown(this Transform transform, Vector3 startScale, Vector3 targetScale, float duration, AnimationCurve scaleCurve, System.Action onComplete = null)
    {
        float timer = 0f;

        while (timer < duration)
        {
            float normalizedTime = (duration > 0) ? timer / duration : 1f;
            float curveValue = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;

        onComplete?.Invoke();
    }
}

// Example usage within a MonoBehaviour:
public class ScaleAnimator : MonoBehaviour
{
    public Vector3 startScale = Vector3.zero;
    public Vector3 targetScale = Vector3.one;
    public float duration = 0.7f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public void StartScaleUp()
    {
        StartCoroutine(transform.ScaleUp(startScale, targetScale, duration, scaleCurve, () => Debug.Log("Scale up complete!")));
    }

    public void StartScaleDown()
    {
        StartCoroutine(transform.ScaleDown(targetScale, startScale, duration, scaleCurve, () => Debug.Log("Scale down complete!")));
    }
}