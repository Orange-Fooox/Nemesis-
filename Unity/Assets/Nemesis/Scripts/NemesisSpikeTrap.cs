using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public sealed class NemesisSpikeTrap : MonoBehaviour
{
    public int damage = 18;
    public float hitCooldown = 1f;
    private float nextHit;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time < nextHit) return;
        NemesisPlayer player = other.GetComponent<NemesisPlayer>();
        if (player == null) return;
        nextHit = Time.time + hitCooldown;
        player.TakeDamage(damage);
        NemesisHUD.Instance?.SetMessage("ШИПЫ БЕЗДНЫ: -" + damage);
    }
}