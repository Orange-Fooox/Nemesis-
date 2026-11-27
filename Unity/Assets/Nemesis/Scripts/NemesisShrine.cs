using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NemesisShrine : MonoBehaviour
{
    public int healAmount = 40;
    public bool used;

    public void Activate()
    {
        if (used)
        {
            NemesisHUD.Instance?.SetMessage("СВЯТИЛИЩЕ ЗАМОЛКЛО");
            return;
        }
        NemesisPlayer player = NemesisPlayer.Instance;
        if (player == null) return;
        if (player.health >= player.maxHealth)
        {
            NemesisHUD.Instance?.SetMessage("ЗДОРОВЬЕ УЖЕ ВОССТАНОВЛЕНО");
            return;
        }
        int before = player.health;
        player.health = Mathf.Min(player.maxHealth, player.health + healAmount);
        used = true;
        NemesisHUD.Instance?.SetHealth(player.health, player.maxHealth);
        NemesisHUD.Instance?.SetMessage("ПЕПЕЛЬНОЕ СВЯТИЛИЩЕ  |  +" + (player.health - before) + " ЗДОРОВЬЯ");
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(.55f, .55f, .55f, .75f);
    }
}
