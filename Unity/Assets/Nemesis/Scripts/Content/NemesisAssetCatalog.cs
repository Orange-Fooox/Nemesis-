using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nemesis.Content
{
    [Serializable]
    public sealed class NemesisAssetEntry
    {
        public string category;
        public string assetName;
        public string sourcePath;
        public GameObject prefab;
        public int matchScore;
    }

    [CreateAssetMenu(menuName = "Nemesis/Imported Asset Catalog", fileName = "NemesisAssetCatalog")]
    public sealed class NemesisAssetCatalog : ScriptableObject
    {
        [SerializeField] private List<NemesisAssetEntry> entries = new List<NemesisAssetEntry>();

        public IReadOnlyList<NemesisAssetEntry> Entries => entries;

        public GameObject FindPrefab(string category)
        {
            NemesisAssetEntry best = null;
            if (string.IsNullOrWhiteSpace(category)) return null;

            foreach (NemesisAssetEntry entry in entries)
            {
                if (entry == null || entry.prefab == null ||
                    !string.Equals(entry.category, category, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (best == null || entry.matchScore > best.matchScore)
                    best = entry;
            }

            return best != null ? best.prefab : null;
        }

        public void ReplaceEntries(List<NemesisAssetEntry> newEntries)
        {
            entries = newEntries ?? new List<NemesisAssetEntry>();
        }
    }
}
