using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class NemesisDepthSort : MonoBehaviour
{
    [Min(-1000)] public int baseOrder = 100;
    public int orderOffset;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.sortingOrder = baseOrder - Mathf.RoundToInt(transform.position.y * 100f) + orderOffset;
    }
}
