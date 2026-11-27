using UnityEngine;
using Nemesis.Combat;
using Nemesis.Player;
using Nemesis.AI;

namespace Nemesis.UI
{
    public sealed class NemesisHUD : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private PlayerResources resources;
        private GUIStyle label, title;

        private void Awake()
        {
            if (health == null) health = GetComponent<Health>();
            if (resources == null) resources = GetComponent<PlayerResources>();
        }

        private void OnGUI()
        {
            if (health == null || resources == null) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 13, normal = { textColor = new Color(0.94f, 0.88f, 0.76f) } };
                title = new GUIStyle(label) { fontSize = 16, fontStyle = FontStyle.Bold };
            }

            const float x = 22f, y = 20f, width = 250f, height = 16f;
            GUI.color = new Color(0.07f, 0.06f, 0.09f, 0.88f);
            GUI.Box(new Rect(x - 10, y - 8, width + 20, 140), GUIContent.none);
            GUI.color = Color.white;
            GUI.Label(new Rect(x, y - 3, width, 22), "N E M E Z I D A", title);
            DrawBar(x, y + 25, width, height, health.Current, health.Maximum, new Color(0.62f, 0.18f, 0.22f), "HP");
            DrawBar(x, y + 49, width, height, resources.Stamina, resources.MaxStamina, new Color(0.72f, 0.62f, 0.31f), "STAMINA");
            DrawBar(x, y + 73, width, height, resources.Essence, resources.MaxEssence, new Color(0.42f, 0.30f, 0.72f), "ESSENCE");
            GUI.Label(new Rect(x, y + 100, 500, 24), "LMB/F Attack   Space Dodge   Shift Sprint   Z/X/C/V/B Abilities", label);
            GUI.Label(new Rect(x, y + 121, 520, 20), NemesisPlayerMemory.Description, label);
        }

        private void DrawBar(float x, float y, float width, float height, float value, float max, Color color, string caption)
        {
            GUI.color = new Color(0.15f, 0.13f, 0.17f);
            GUI.Box(new Rect(x, y, width, height), GUIContent.none);
            GUI.color = color;
            GUI.DrawTexture(new Rect(x + 2, y + 2, (width - 4) * Mathf.Clamp01(value / Mathf.Max(1f, max)), height - 4), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(x + 6, y - 1, width - 12, height + 2), caption + "  " + Mathf.CeilToInt(value) + " / " + Mathf.CeilToInt(max), label);
        }
    }
}