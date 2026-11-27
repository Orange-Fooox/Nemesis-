using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class NemesisHeroAnimator : MonoBehaviour
{
    [Header("Optional directional frame sheets")]
    [Min(1f)] public float idleFps = 4f;
    [Min(1f)] public float walkFps = 9f;
    [Min(1f)] public float runFps = 12f;
    [Min(1f)] public float attackFps = 14f;
    [Min(1f)] public float hurtFps = 12f;
    [Min(1f)] public float deathFps = 10f;

    private const string ResourceRoot = "NemesisCharacters/Player";
    private readonly Dictionary<string, Sprite[]> cache = new Dictionary<string, Sprite[]>();
    private SpriteRenderer spriteRenderer;
    private Transform visualRoot;
    private Sprite[] frames = Array.Empty<Sprite>();
    private string currentState = "Idle";
    private string facing = "Down";
    private Vector2 movement;
    private Vector3 baseScale;
    private Vector3 basePosition;
    private Quaternion baseRotation;
    private Color baseColor;
    private int frameIndex;
    private float frameTimer;
    private float actionStarted;
    private float actionDuration;
    private bool hasFrameClip;
    private bool dead;
    private bool warnedMissingClips;

    private void Awake()
    {
        SpriteRenderer source = GetComponent<SpriteRenderer>();
        // Keep animation offsets separate from the Rigidbody2D transform.
        if (GetComponent<Rigidbody2D>() != null && source != null)
        {
            GameObject visual = new GameObject("Hero Animated Visual");
            visual.transform.SetParent(transform, false);
            visualRoot = visual.transform;
            spriteRenderer = visual.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = source.sprite;
            spriteRenderer.color = source.color;
            spriteRenderer.sortingLayerID = source.sortingLayerID;
            spriteRenderer.sortingOrder = source.sortingOrder;
            spriteRenderer.flipX = source.flipX;
            spriteRenderer.flipY = source.flipY;
            source.enabled = false;
            NemesisDepthSort sort = visual.AddComponent<NemesisDepthSort>();
            sort.baseOrder = 1000;
            sort.orderOffset = 7;
        }
        else
        {
            spriteRenderer = source;
            visualRoot = transform;
        }

        baseScale = visualRoot.localScale;
        basePosition = visualRoot.localPosition;
        baseRotation = visualRoot.localRotation;
        baseColor = spriteRenderer.color;
        SetState("Idle", true);
    }

    public void SetMovement(Vector2 direction) => SetMovement(direction, false);

    public void SetMovement(Vector2 direction, bool running)
    {
        if (dead) return;
        movement = direction;
        if (direction.sqrMagnitude > 0.025f)
        {
            facing = ResolveFacing(direction);
            if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.05f)
                spriteRenderer.flipX = direction.x < 0f;
        }

        if (IsActionLocked()) return;
        SetState(direction.sqrMagnitude > 0.025f ? (running ? "Run" : "Walk") : "Idle");
    }

    public void PlayAttack() => PlayAction("Attack", 0.42f);
    public void PlayHurt() => PlayAction("Hurt", 0.26f);
    public void PlayDodge() => PlayAction("Dodge", 0.22f);

    public void PlayDeath()
    {
        if (dead) return;
        dead = true;
        PlayAction("Death", 1f);
    }

    public void ResetAnimation()
    {
        dead = false;
        movement = Vector2.zero;
        SetState("Idle", true);
        visualRoot.localScale = baseScale;
        visualRoot.localPosition = basePosition;
        visualRoot.localRotation = baseRotation;
        spriteRenderer.color = baseColor;
    }

    private void Update()
    {
        if (spriteRenderer == null || Time.timeScale <= 0f) return;

        if (currentState == "Death")
        {
            UpdateDeathPose();
            return;
        }

        if (IsActionLocked() && Time.time >= actionStarted + actionDuration)
            SetState(movement.sqrMagnitude > 0.025f ? "Walk" : "Idle", true);

        if (hasFrameClip) AdvanceFrames();
        else UpdateProceduralPose();
    }

    private void PlayAction(string state, float duration)
    {
        if (dead && state != "Death") return;
        SetState(state, true);
        actionStarted = Time.time;
        actionDuration = duration;
    }

    private bool IsActionLocked()
    {
        return currentState == "Attack" || currentState == "Hurt" || currentState == "Dodge" ||
               (currentState == "Death" && dead);
    }

    private void SetState(string state, bool restart = false)
    {
        if (!restart && currentState == state) return;
        currentState = state;
        actionStarted = Time.time;
        frameIndex = 0;
        frameTimer = 0f;
        frames = LoadFrames(state, facing);
        hasFrameClip = frames.Length > 0;

        if (!hasFrameClip && (state == "Run" || state == "Dodge"))
        {
            frames = LoadFrames("Walk", facing);
            hasFrameClip = frames.Length > 0;
        }
        if (!hasFrameClip && state != "Idle" && state != "Walk")
        {
            frames = LoadFrames("Idle", facing);
            hasFrameClip = frames.Length > 0;
        }
        if (hasFrameClip) spriteRenderer.sprite = frames[0];
        else if (!warnedMissingClips && state == "Walk")
        {
            warnedMissingClips = true;
            Debug.LogWarning("NEMESIS HERO: directional frame sheets are not installed; animated visual fallback is active.");
        }
    }

    private Sprite[] LoadFrames(string state, string direction)
    {
        string key = state + "/" + direction;
        if (cache.TryGetValue(key, out Sprite[] cached)) return cached;
        Sprite[] loaded = Resources.LoadAll<Sprite>(ResourceRoot + "/" + key);
        Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.name, b.name));
        cache[key] = loaded;
        return loaded;
    }

    private void AdvanceFrames()
    {
        if (frames.Length == 0) return;
        frameTimer += Time.deltaTime;
        float step = 1f / Mathf.Max(1f, GetFps());
        while (frameTimer >= step)
        {
            frameTimer -= step;
            if (frameIndex < frames.Length - 1)
            {
                frameIndex++;
                spriteRenderer.sprite = frames[frameIndex];
            }
            else if (currentState == "Idle" || currentState == "Walk" || currentState == "Run")
            {
                frameIndex = 0;
                spriteRenderer.sprite = frames[0];
            }
            else if (currentState != "Death" && Time.time >= actionStarted + actionDuration)
            {
                SetState(movement.sqrMagnitude > 0.025f ? "Walk" : "Idle", true);
                return;
            }
        }
    }

    private float GetFps()
    {
        switch (currentState)
        {
            case "Walk": return walkFps;
            case "Run": return runFps;
            case "Attack": return attackFps;
            case "Hurt": return hurtFps;
            case "Death": return deathFps;
            default: return idleFps;
        }
    }

    private void UpdateProceduralPose()
    {
        float t = Time.time;
        float bob = 0f, squash = 1f, rotation = 0f;
        Vector2 offset = Vector2.zero;
        Color tint = baseColor;

        switch (currentState)
        {
            case "Idle":
                bob = Mathf.Sin(t * 3.4f) * 0.018f;
                squash = 1f + Mathf.Sin(t * 3.4f) * 0.012f;
                break;
            case "Walk":
                bob = Mathf.Abs(Mathf.Sin(t * 12f)) * 0.045f;
                squash = 1f - Mathf.Abs(Mathf.Sin(t * 12f)) * 0.025f;
                rotation = Mathf.Sin(t * 12f) * 2.2f;
                break;
            case "Run":
                bob = Mathf.Abs(Mathf.Sin(t * 16f)) * 0.075f;
                squash = 1f - Mathf.Abs(Mathf.Sin(t * 16f)) * 0.045f;
                rotation = Mathf.Sin(t * 16f) * 4f;
                break;
            case "Attack":
                float attack = Mathf.Clamp01((t - actionStarted) / Mathf.Max(0.01f, actionDuration));
                float swing = Mathf.Sin(attack * Mathf.PI);
                offset = FacingVector() * (0.14f * swing);
                rotation = (facing == "Left" ? 1f : -1f) * Mathf.Sin(attack * Mathf.PI * 2f) * 11f;
                squash = 1f + swing * 0.055f;
                tint = Color.Lerp(baseColor, new Color(1f, 0.82f, 0.62f, 1f), swing * 0.35f);
                break;
            case "Hurt":
                float hit = Mathf.Clamp01((t - actionStarted) / Mathf.Max(0.01f, actionDuration));
                offset.x = Mathf.Sin(t * 72f) * (1f - hit) * 0.055f;
                rotation = Mathf.Sin(t * 72f) * (1f - hit) * 5f;
                tint = Color.Lerp(new Color(1f, 0.34f, 0.3f, 1f), baseColor, hit);
                break;
            case "Dodge":
                float dodge = Mathf.Clamp01((t - actionStarted) / Mathf.Max(0.01f, actionDuration));
                offset = FacingVector() * (0.18f * Mathf.Sin(dodge * Mathf.PI));
                squash = 1f - Mathf.Sin(dodge * Mathf.PI) * 0.13f;
                tint = Color.Lerp(baseColor, new Color(0.68f, 0.72f, 0.9f, 1f), 0.35f);
                break;
        }

        visualRoot.localPosition = basePosition + new Vector3(offset.x, offset.y + bob, 0f);
        visualRoot.localScale = new Vector3(baseScale.x * (2f - squash), baseScale.y * squash, baseScale.z);
        visualRoot.localRotation = baseRotation * Quaternion.Euler(0f, 0f, rotation);
        spriteRenderer.color = tint;
    }

    private void UpdateDeathPose()
    {
        float t = Mathf.Clamp01((Time.time - actionStarted) / Mathf.Max(0.05f, actionDuration));
        visualRoot.localRotation = Quaternion.Lerp(baseRotation, baseRotation * Quaternion.Euler(0f, 0f, facing == "Left" ? -68f : 68f), t);
        visualRoot.localScale = Vector3.Lerp(baseScale, new Vector3(baseScale.x * 1.12f, baseScale.y * 0.42f, baseScale.z), t);
        Color c = baseColor; c.a = 1f - Mathf.Clamp01((t - 0.55f) / 0.45f) * 0.28f;
        spriteRenderer.color = c;
    }

    private Vector2 FacingVector()
    {
        switch (facing)
        {
            case "Up": return Vector2.up;
            case "Down": return Vector2.down;
            case "Left": return Vector2.left;
            default: return Vector2.right;
        }
    }

    private static string ResolveFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) return direction.x >= 0f ? "Right" : "Left";
        return direction.y >= 0f ? "Up" : "Down";
    }
}