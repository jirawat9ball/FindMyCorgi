using UnityEngine;
using System.Collections;

public class ScaleUpOnStart : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("How long the scale-up animation should take in seconds.")]
    public float duration = 0.7f; // Slightly longer duration might feel better with overshoot

    [Tooltip("The scale the object starts at before animating.")]
    public Vector3 startScale = Vector3.zero;

    [Tooltip("Easing curve for the scale animation. Edit this curve to create overshoot!")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Default EaseInOut

    // Private variables
    private Vector3 targetScale;
    private bool hasAnimated = false;

    void Awake()
    {
        targetScale = transform.localScale;
        if (!hasAnimated)
        {
            transform.localScale = startScale;
        }
    }

    void Start()
    {
        if (!hasAnimated)
        {
            StartCoroutine(ScaleUpRoutine());
        }
    }
    private void OnEnable()
    {
        StartCoroutine(ScaleUpRoutine());
    }
    public void ScaleDown()
    {
        StartCoroutine(ScaleDownRoutine());
    }

     IEnumerator ScaleUpRoutine()
    {
        float timer = 0f;
        Vector3 originalScale = targetScale;

        while (timer < duration)
        {
            float normalizedTime = (duration > 0) ? timer / duration : 1f;
            float curveValue = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.LerpUnclamped(startScale, originalScale, curveValue);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        hasAnimated = true;
    }

    public IEnumerator ScaleDownRoutine()
    {
        float timer = 0f;
        Vector3 originalScale = transform.localScale; // start from current scale.

        while (timer < duration)
        {
            float normalizedTime = (duration > 0) ? timer / duration : 1f;
            float curveValue = scaleCurve.Evaluate(normalizedTime);
            transform.localScale = Vector3.LerpUnclamped(originalScale, startScale, curveValue);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = startScale;
        hasAnimated = false; // reset hasAnimated so the object can scale up again.
    }
}