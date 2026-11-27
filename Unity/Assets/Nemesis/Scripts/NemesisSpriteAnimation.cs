using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class NemesisSpriteAnimation : MonoBehaviour
{
    public Sprite[] frames;
    [Min(0.5f)] public float framesPerSecond = 6f;
    public bool loop = true;
    public bool randomStart;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private int frame;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (frames == null || frames.Length == 0) return;
        if (randomStart)
            frame = Random.Range(0, frames.Length);
        Apply();
    }

    private void Update()
    {
        if (frames == null || frames.Length < 2 || Time.timeScale <= 0f) return;
        timer += Time.deltaTime;
        float step = 1f / Mathf.Max(0.5f, framesPerSecond);
        while (timer >= step)
        {
            timer -= step;
            if (frame < frames.Length - 1) frame++;
            else if (loop) frame = 0;
            else return;
            Apply();
        }
    }

    private void Apply()
    {
        if (spriteRenderer != null && frames != null && frames.Length > 0)
            spriteRenderer.sprite = frames[Mathf.Clamp(frame, 0, frames.Length - 1)];
    }
}