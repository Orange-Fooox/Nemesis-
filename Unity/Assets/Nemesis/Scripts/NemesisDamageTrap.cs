using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NemesisDamageTrap : MonoBehaviour
{
    public int damage = 12;
    public float damageInterval = 1.1f;
    private float nextDamage;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        NemesisPlayer player = other.GetComponent<NemesisPlayer>();
        if (player == null || Time.time < nextDamage) return;
        nextDamage = Time.time + damageInterval;
        player.TakeDamage(damage);
        NemesisHUD.Instance?.SetMessage("ЛОВУШКА!  -" + damage + " ЗДОРОВЬЯ");
    }
}
