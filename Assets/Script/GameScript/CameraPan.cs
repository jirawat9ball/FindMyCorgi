using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraPan : MonoBehaviour
{
    public enum TypePan { Fix, Horizontal, Vertical, Free }

    [Header("Zoom Setup")]
    public float zoomSpeed = 1.0f;
    public float minZoom = 5f;
    public float ToggleRangeZoom = 20f;
    public float maxZoom = 20f;

    [Header("Pan Setup")]
    public float panSpeed = 1.0f;
    public Camera targetCamera;
    public float smoothSpeed = 0.125f;
    public TypePan panType = TypePan.Free; // Added pan type
    TypePan ifMaxZoom;
    SpriteRenderer targetSpriteRenderer;

    private Vector3 dragOrigin;
    private Bounds spriteBounds;
    private float cameraHalfWidth;
    private float cameraHalfHeight;
    private bool isPanning = false;

    [Header("Leap Animation Settings")]
    [Tooltip("ระยะเวลาในการเลื่อน/ซูมกล้อง (วินาที)")]
    public float leapDuration = 1.0f;
    [Tooltip("กราฟความเร็วในการขยับกล้อง")]
    public AnimationCurve leapCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private void Start()
    {
        Gamemanager.Instance.cameraPan = this;
    }

    public void SetUpCamera(SceneHandle sceneHandle)
    {
        if (sceneHandle.targetSpriteRenderer == null)
        {
            Debug.LogError("Target SpriteRenderer not assigned!");
            enabled = false;
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("No camera found or assigned!");
                enabled = false;
                return;
            }
        }
        maxZoom = sceneHandle.maxZoom;
        ToggleRangeZoom = sceneHandle.ToggleRangeZoom;
        panType = sceneHandle.panType;
        ifMaxZoom = sceneHandle.panType;
        targetSpriteRenderer = sceneHandle.targetSpriteRenderer;
        spriteBounds = targetSpriteRenderer.bounds;
        UpdateCameraDimensions();

        Vector3 centerPosition = spriteBounds.center;
        centerPosition.z = -10; // ล็อกแกน Z ไว้ที่เดิม (กล้องจะได้ไม่ทะลุจอ)
        targetCamera.transform.position = centerPosition;
    }

    void Update()
    {
        if (Gamemanager.Instance.stateGame == StateGame.gameplay)
        {
            HandleZoom();
            HandlePan();
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetCamera.orthographicSize -= scroll * zoomSpeed;
            targetCamera.orthographicSize = Mathf.Clamp(targetCamera.orthographicSize, minZoom, maxZoom);
            UpdateCameraDimensions();
            ConstrainCameraPosition();
        }
        if (targetCamera.orthographicSize < ToggleRangeZoom)
        {
            panType = TypePan.Free;
        }
        else {
            panType = ifMaxZoom;
        }
    }

    void HandlePan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = targetCamera.ScreenToWorldPoint(Input.mousePosition);
            isPanning = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 difference = dragOrigin - targetCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPosition = targetCamera.transform.position + difference;

            newPosition = ConstrainPosition(newPosition);
            targetCamera.transform.position = newPosition;
        }
    }

    Vector3 ConstrainPosition(Vector3 position)
    {
        float minX = spriteBounds.min.x + cameraHalfWidth;
        float maxX = spriteBounds.max.x - cameraHalfWidth;
        float minY = spriteBounds.min.y + cameraHalfHeight;
        float maxY = spriteBounds.max.y - cameraHalfHeight;
        Vector3 center = spriteBounds.center;

        bool FitsideX = position.x <= minX && position.x >= maxX;
        bool FitsideY = position.y <= minY && position.y >= maxY;
        center.z = position.z;

        //Debug.Log("FitsideX " + FitsideX);
        //Debug.Log("FitsideY " + FitsideY);

        position.x = (FitsideX) ?  center.x : Mathf.Clamp(position.x, minX, maxX);
        position.y =(FitsideY)  ? center.y : Mathf.Clamp(position.y, minY, maxY);

        switch (panType)
        {
            case TypePan.Horizontal:
                position.y = targetCamera.transform.position.y; // Lock Y
                position.x = Mathf.Clamp(position.x, minX, maxX);
                break;
            case TypePan.Vertical:
                position.x = targetCamera.transform.position.x; // Lock X
                position.y = Mathf.Clamp(position.y, minY, maxY);
                break;
            case TypePan.Free:
                // If inside, clamp normally.
                position.x = Mathf.Clamp(position.x, minX, maxX);
                position.y = Mathf.Clamp(position.y, minY, maxY);
                break;
            case TypePan.Fix:
                center.z = position.z; // Keep the original Z.
                position = center;
                break;
        }

        return position;
    }

    private Vector3 TryTocenter(Vector3 position, float minX, float maxX, float minY, float maxY)
    {
        // Check if the camera is outside the bounds.
        bool outsideX = position.x < minX || position.x > maxX;
        bool outsideY = position.y < minY || position.y > maxY;

        if (outsideX || outsideY)
        {
            // If outside, smoothly move towards the center.
            Vector3 center = spriteBounds.center;
            center.z = position.z; // Keep the original Z.
            position = Vector3.Lerp(position, center, 0.1f); // Adjust the 0.1f for smoothing.
        }
        else
        {
            // If inside, clamp normally.
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.y = Mathf.Clamp(position.y, minY, maxY);
        }

        return position;
    }

    void ConstrainCameraPosition()
    {
        targetCamera.transform.position = ConstrainPosition(targetCamera.transform.position);
    }

    void UpdateCameraDimensions()
    {
        cameraHalfHeight = targetCamera.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * targetCamera.aspect;
    }

    public void TriggerLeap(Vector3 target, bool zomeOut = false, System.Action onComplete = null)
    {
        StartCoroutine(StartLeap(target, zomeOut, onComplete));
    }

    public IEnumerator StartLeap(Vector3 targetPosition, bool zomeOut = false, System.Action onComplete = null)
    {
        float timer = 0f; // เปลี่ยนมาใช้ timer แบบเดียวกับ ScaleUpOnStart
        Vector3 startPosition = transform.position;
        float startZoom = targetCamera.orthographicSize;

        targetPosition.z = -10;

        // 🌟 ป้องกันเป้าหมาย (เป้าหมายปลายทาง) ไม่ให้อยู่นอกระยะกล้อง
        // โดยทำการจำลองซูมไปยังจุดสุดท้ายเพื่อคำนวณตำแหน่งที่ถูกล็อกไว้แล้ว
        float targetZoom = zomeOut ? maxZoom : minZoom;
        targetCamera.orthographicSize = targetZoom;
        UpdateCameraDimensions();
        targetPosition = ConstrainPosition(targetPosition); 
        
        // 🌟 คืนค่ากลับที่เดิมก่อนเริ่มอนิเมชัน
        targetCamera.orthographicSize = startZoom;
        UpdateCameraDimensions();

        // 🌟 เปลี่ยนจากลูปเวลา Time.time มาใช้ timer เทียบกับ leapDuration
        while (timer < leapDuration)
        {
            // คำนวณเวลาที่ผ่านไป (0.0 ถึง 1.0)
            float normalizedTime = (leapDuration > 0) ? timer / leapDuration : 1f;

            // 🌟 ดึงค่าจากเส้นกราฟ Curve
            float curveValue = leapCurve.Evaluate(normalizedTime);

            if (zomeOut)
            {
                targetCamera.orthographicSize = Mathf.LerpUnclamped(startZoom, maxZoom, curveValue);
            }
            else
            {
                targetCamera.orthographicSize = Mathf.LerpUnclamped(startZoom, minZoom, curveValue);
            }

            // 🌟 สำคัญ: อัปเดตขอบเขตของกล้องตามขนาดซูม (ณ เฟรมนี้) เพื่อป้องกันจอทะลุขอบ
            UpdateCameraDimensions();

            // 🌟 ใช้ LerpUnclamped ขยับหาเป้าหมาย แต่ครอบด้วย ConstrainPosition คุมกำเนิดไม่ให้หลุดแผนที่เสมอ
            Vector3 lerpedPos = Vector3.LerpUnclamped(startPosition, targetPosition, curveValue);
            transform.position = ConstrainPosition(lerpedPos);

            timer += Time.deltaTime;
            yield return null;
        }

        // เมื่อจบ Animation ให้บังคับค่าเป๊ะๆ อีกครั้ง ป้องกันทศนิยมคลาดเคลื่อน
        targetCamera.orthographicSize = targetZoom;
        UpdateCameraDimensions();
        transform.position = targetPosition;
        ConstrainCameraPosition();
        
        onComplete?.Invoke();
    }
}