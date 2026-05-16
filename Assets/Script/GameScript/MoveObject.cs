using UnityEngine;

public class MoveObject : Interaction
{

    [Header("Move Object setUp")]
    public float DistanTarget = 0.5f;
    public Transform Target;
    public KeyItem RequiteAbilities;
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 StartPosition;
    protected override void Start()
    {
        base.Start();
        StartPosition = transform.position;
    }
    protected override void OnMouseDown()
    {
        if (!Gamemanager.Instance.isStateGamePlay())
        {
            return;
        }
        isDragging = true;
        Gamemanager.Instance.cameraPan.enabled = false;

    }
    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
            transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
            float getRange = Vector3.Distance(transform.position, Target.position);
            if (getRange < DistanTarget)
            {
                spriteRenderer.color = Color.green;
            }
            else {
                spriteRenderer.color = Color.white;
            }
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        Gamemanager.Instance.cameraPan.enabled = true;
        float getRange = Vector3.Distance(transform.position, Target.position);
        if (getRange < DistanTarget)
        {
            transform.position = Target.position;
            boxCollider.enabled = false;
        }
        else {
            transform.position = StartPosition;
        }
        spriteRenderer.color = Color.white;
    }

}
