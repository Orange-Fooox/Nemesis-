using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class NemesisEnemyAnimator : MonoBehaviour
{
    public float idleBob = 0.006f;
    public float walkBob = 0.018f;
    private SpriteRenderer sr;
    private Vector3 baseScale;
    private Vector3 basePosition;
    private Quaternion baseRotation;
    private Color baseColor;
    private Vector2 moveDirection;
    private bool moving;
    private bool dead;
    private float attackUntil;
    private float hurtUntil;
    private float phase;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        basePosition = transform.localPosition;
        baseRotation = transform.localRotation;
        baseColor = sr.color;
        phase = Random.Range(0f, 6.28318f);
    }

    public void SetMovement(Vector2 direction)
    {
        if (dead) return;
        moveDirection = direction;
        moving = direction.sqrMagnitude > 0.025f;
        if (sr != null && Mathf.Abs(direction.x) > 0.05f)
            sr.flipX = direction.x < 0f;
    }

    public void PlayAttack(Vector2 direction)
    {
        if (dead) return;
        if (direction.sqrMagnitude > 0.01f) SetMovement(direction);
        attackUntil = Time.time + 0.48f;
    }

    public void PlayHurt()
    {
        if (dead) return;
        hurtUntil = Time.time + 0.24f;
    }

    public void PlayDeath()
    {
        if (dead) return;
        dead = true;
        moving = false;
        StartCoroutine(DeathRoutine());
    }

    private void Update()
    {
        if (dead || sr == null || Time.timeScale <= 0f) return;

        float t = Time.time;
        float bob = moving ? Mathf.Abs(Mathf.Sin(t * 8f + phase)) * walkBob : Mathf.Sin(t * 2.2f + phase) * idleBob;
        float stretch = moving ? 1f - Mathf.Abs(Mathf.Sin(t * 8f + phase)) * 0.035f : 1f + Mathf.Sin(t * 2.2f + phase) * 0.012f;
        float tilt = moving ? Mathf.Sin(t * 8f + phase) * 1.1f : Mathf.Sin(t * 1.8f + phase) * 1f;
        Vector3 offset = Vector3.zero;
        Color tint = baseColor;

        if (t < attackUntil)
        {
            float p = 1f - (attackUntil - t) / 0.48f;
            float lunge = Mathf.Sin(p * Mathf.PI);
            offset = new Vector3(moveDirection.x, moveDirection.y, 0f) * (0.11f * lunge);
            stretch += lunge * 0.06f;
            tilt += Mathf.Sin(p * Mathf.PI * 2f) * 6f;
            tint = Color.Lerp(baseColor, new Color(1f, 0.62f, 0.48f, 1f), lunge * 0.45f);
        }
        else if (t < hurtUntil)
        {
            float p = (hurtUntil - t) / 0.24f;
            offset.x += Mathf.Sin(t * 74f) * p * 0.045f;
            tint = Color.Lerp(baseColor, new Color(1f, 0.32f, 0.28f, 1f), p);
        }

        transform.localPosition = basePosition + offset + Vector3.up * bob;
        transform.localScale = new Vector3(baseScale.x * (2f - stretch), baseScale.y * stretch, baseScale.z);
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, tilt);
        sr.color = tint;
    }

    private IEnumerator DeathRoutine()
    {
        if (sr == null) { Destroy(gameObject); yield break; }
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null) body.linearVelocity = Vector2.zero;

        float duration = 0.62f;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float p = Mathf.Clamp01(time / duration);
            transform.localRotation = Quaternion.Lerp(baseRotation, baseRotation * Quaternion.Euler(0f, 0f, sr.flipX ? 62f : -62f), p);
            transform.localScale = Vector3.Lerp(baseScale, new Vector3(baseScale.x * 1.1f, baseScale.y * 0.42f, baseScale.z), p);
            Color c = baseColor; c.a = 1f - p;
            sr.color = c;
            yield return null;
        }
        Destroy(gameObject);
    }
}
