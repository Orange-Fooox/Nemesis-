using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class NemesisCamera : MonoBehaviour
{
    public Transform target;
    [Min(0.01f)] public float smoothTime = 0.18f;
    [Min(0f)] public float maxSpeed = 70f;
    public Vector2 lookAhead = new Vector2(0.12f, 0.06f);
    public float zOffset = -10f;

    private Vector3 velocity;
    private Vector3 lastTargetPosition;

    private void OnEnable()
    {
        if (target != null) lastTargetPosition = target.position;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position;
        Vector3 delta = targetPosition - lastTargetPosition;
        lastTargetPosition = targetPosition;

        Vector3 aim = new Vector3(
            targetPosition.x + Mathf.Clamp(delta.x, -0.25f, 0.25f) * lookAhead.x,
            targetPosition.y + Mathf.Clamp(delta.y, -0.25f, 0.25f) * lookAhead.y,
            zOffset);

        Vector3 current = transform.position;
        current.z = zOffset;
        transform.position = Vector3.SmoothDamp(current, aim, ref velocity, smoothTime, maxSpeed, Time.deltaTime);
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        Vector3 p = target.position;
        transform.position = new Vector3(p.x, p.y, zOffset);
        lastTargetPosition = p;
        velocity = Vector3.zero;
    }
}