using UnityEngine;

public class AutoRotateZ : MonoBehaviour
{
    public float rotationSpeed = 30f; // Degrees per second
    public bool invertRotation;
    void Update()
    {
        // Rotate the object around the Z-axis.
        float invert = invertRotation ? -1f : 1f; // Added invert calculation.

        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime * invert);
    }
}