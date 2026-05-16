using UnityEngine;
using TMPro; // Namespace for TextMeshPro
using System.Collections; // Required for IEnumerator

public class TMPColorScalePingPong : MonoBehaviour
{
    // --- Public Variables (Editable in Inspector) ---
    [Header("Target Component")]
    [Tooltip("The TextMeshPro component to change the color of.")]
    public TextMeshProUGUI textMeshProComponent; // Use TextMeshPro for 3D text

    [Header("Color Animation")]
    [Tooltip("The first color in the ping-pong cycle.")]
    public Color colorStart = Color.white;
    [Tooltip("The second color in the ping-pong cycle.")]
    public Color colorEnd = Color.red;

    [Header("Scale Animation")]
    [Tooltip("The starting scale in the ping-pong cycle.")]
    public Vector3 scaleStart = Vector3.one;
    [Tooltip("The ending scale in the ping-pong cycle.")]
    public Vector3 scaleEnd = new Vector3(1.2f, 1.2f, 1.2f);

    [Header("Timing & Easing")]
    [Tooltip("How long it takes to transition from start to end state (color and scale) in seconds.")]
    public float duration = 1.0f;

    [Tooltip("Controls the easing of the animation. Time (X-axis 0 to 1) maps to interpolation factor (Y-axis 0 to 1).")]
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Default to EaseInOut

    // --- Private Variables ---
    private Coroutine _animationCoroutine;
    private Vector3 _initialScale;

    void Awake()
    {
        _initialScale = transform.localScale;
    }

    void Start()
    {
        // --- Initialization & Error Handling ---
        if (textMeshProComponent == null)
        {
            textMeshProComponent = GetComponent<TextMeshProUGUI>();
        }
        if (textMeshProComponent == null)
        {
            Debug.LogError($"{nameof(TMPColorScalePingPong)}: TextMeshPro component not found or assigned!", this);
            this.enabled = false;
            return;
        }
        if (duration <= 0)
        {
            Debug.LogWarning($"{nameof(TMPColorScalePingPong)}: Duration must be greater than zero. Setting to 1.0.", this);
            duration = 1.0f;
        }

        // --- Start the Coroutine ---
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
        }
        _animationCoroutine = StartCoroutine(PingPongAnimationRoutine());
    }

    // --- The Main Ping-Pong Coroutine ---
    IEnumerator PingPongAnimationRoutine()
    {
        // Set initial state
        textMeshProComponent.color = colorStart;
        transform.localScale = scaleStart;

        while (true)
        {
            // --- Phase 1: Start to End ---
            yield return AnimateProperties(colorStart, colorEnd, scaleStart, scaleEnd, duration);

            // --- Phase 2: End to Start ---
            yield return AnimateProperties(colorEnd, colorStart, scaleEnd, scaleStart, duration);
        }
    }

    // --- Helper Coroutine for Smooth Transition with Easing ---
    IEnumerator AnimateProperties(Color fromColor, Color toColor, Vector3 fromScale, Vector3 toScale, float transitionDuration)
    {
        float timer = 0f;

        while (timer < transitionDuration)
        {
            // Calculate normalized time (0 to 1)
            float normalizedTime = timer / transitionDuration;

            // Evaluate the curve to get the eased interpolation factor
            // The curve maps the linear normalizedTime (0-1) to a potentially non-linear value (usually also 0-1)
            float curveValue = easingCurve.Evaluate(normalizedTime);

            // --- Lerp Color using the curve value ---
            // Using LerpUnclamped allows curves that go outside 0-1 (e.g., overshoot)
            textMeshProComponent.color = Color.LerpUnclamped(fromColor, toColor, curveValue);

            // --- Lerp Scale using the curve value ---
            transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, curveValue);

            // Increment timer
            timer += Time.deltaTime;

            // Wait until the next frame
            yield return null;
        }

        // Ensure the final state is set exactly (using the end points, not the curve's final value)
        textMeshProComponent.color = toColor;
        transform.localScale = toScale;
    }

    // --- Cleanup ---
    void OnDisable()
    {
        if (_animationCoroutine != null)
        {
            StopCoroutine(_animationCoroutine);
            _animationCoroutine = null;
            // Optional: Reset state
            // if (textMeshProComponent != null) textMeshProComponent.color = colorStart;
            // transform.localScale = _initialScale;
        }
    }
}