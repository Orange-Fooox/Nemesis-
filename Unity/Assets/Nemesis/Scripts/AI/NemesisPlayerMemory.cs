using UnityEngine;

namespace Nemesis.AI
{
    public enum NemesisPlayerStyle
    {
        Balanced,
        Melee,
        Abilities,
        Dodge
    }

    /// <summary>
    /// Small local combat-history profile used to make later encounters react to repeated habits.
    /// Values are capped and periodically compressed so a single old play session cannot dominate forever.
    /// </summary>
    public static class NemesisPlayerMemory
    {
        private const string MeleeKey = "Nemesis.Memory.Melee.v1";
        private const string AbilityKey = "Nemesis.Memory.Abilities.v1";
        private const string DodgeKey = "Nemesis.Memory.Dodge.v1";
        private const int MaximumCounter = 120;

        private static bool loaded;
        private static int melee;
        private static int abilities;
        private static int dodges;
        private static int pendingSave;

        public static NemesisPlayerStyle DominantStyle
        {
            get
            {
                Load();
                int total = melee + abilities + dodges;
                if (total < 6) return NemesisPlayerStyle.Balanced;

                int highest = Mathf.Max(melee, Mathf.Max(abilities, dodges));
                if ((float)highest / total < .5f) return NemesisPlayerStyle.Balanced;

                if (highest == melee) return NemesisPlayerStyle.Melee;
                if (highest == abilities) return NemesisPlayerStyle.Abilities;
                return NemesisPlayerStyle.Dodge;
            }
        }

        public static string Description
        {
            get
            {
                switch (DominantStyle)
                {
                    case NemesisPlayerStyle.Melee: return "Немезида замечает частые удары оружием";
                    case NemesisPlayerStyle.Abilities: return "Немезида изучает применение способностей";
                    case NemesisPlayerStyle.Dodge: return "Немезида привыкает к твоим уклонениям";
                    default: return "Немезида пока изучает твой стиль";
                }
            }
        }

        public static void RecordMeleeHit()
        {
            Load();
            melee++;
            Changed();
        }

        public static void RecordAbilityUse()
        {
            Load();
            abilities++;
            Changed();
        }

        public static void RecordDodge()
        {
            Load();
            dodges++;
            Changed();
        }

        private static void Load()
        {
            if (loaded) return;
            melee = PlayerPrefs.GetInt(MeleeKey, 0);
            abilities = PlayerPrefs.GetInt(AbilityKey, 0);
            dodges = PlayerPrefs.GetInt(DodgeKey, 0);
            loaded = true;
        }

        private static void Changed()
        {
            int total = melee + abilities + dodges;
            if (total > MaximumCounter)
            {
                melee = Mathf.RoundToInt(melee * .5f);
                abilities = Mathf.RoundToInt(abilities * .5f);
                dodges = Mathf.RoundToInt(dodges * .5f);
            }

            pendingSave++;
            if (pendingSave >= 5) Save();
        }

        public static void Save()
        {
            Load();
            PlayerPrefs.SetInt(MeleeKey, melee);
            PlayerPrefs.SetInt(AbilityKey, abilities);
            PlayerPrefs.SetInt(DodgeKey, dodges);
            PlayerPrefs.Save();
            pendingSave = 0;
        }
    }
}
