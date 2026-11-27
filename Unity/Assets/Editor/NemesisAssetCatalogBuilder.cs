#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nemesis.Content;
using UnityEditor;
using UnityEngine;

namespace Nemesis.EditorTools
{
    public static class NemesisAssetCatalogBuilder
    {
        private const string CatalogPath = "Assets/Resources/NemesisAssetCatalog.asset";
        private const string ReportPath = "Assets/NemesisContent/ImportedAssetReport.md";
        private const string PreferredPackRoot = "Assets/Daniel Mistage/Stylized Fantasy Armory";

        private sealed class Rule
        {
            public string Category;
            public string[] Keywords;

            public Rule(string category, params string[] keywords)
            {
                Category = category;
                Keywords = keywords;
            }
        }

        private static readonly Rule[] Rules =
        {
            new Rule("helmet", "helmet", "helm", "headgear", "great helm"),
            new Rule("chest_armor", "chestplate", "chest armor", "chest armour", "cuirass", "breastplate", "armor torso"),
            new Rule("greatsword", "greatsword", "claymore", "zweihander", "two handed sword", "two-handed sword"),
            new Rule("sword", "longsword", "broadsword", "sword"),
            new Rule("shield", "shield", "buckler", "tower shield"),
            new Rule("dagger", "dagger", "short blade", "knife"),
            new Rule("bow", "longbow", "shortbow", "crossbow", "bow"),
            new Rule("pillar", "pillar", "column"),
            new Rule("wall", "wall", "archway"),
            new Rule("floor", "floor tile", "floor", "ground tile"),
            new Rule("tree", "tree", "oak", "pine"),
            new Rule("torch", "torch", "lantern", "brazier"),
            new Rule("chest", "treasure chest", "chest", "supply crate"),
            new Rule("altar", "altar", "shrine", "obelisk")
        };

        [MenuItem("Tools/Nemesis/Scan Imported Asset Packs")]
        public static void Scan()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets", "NemesisContent");

            NemesisAssetCatalog catalog = AssetDatabase.LoadAssetAtPath<NemesisAssetCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<NemesisAssetCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            string[] folders = AssetDatabase.IsValidFolder(PreferredPackRoot)
                ? new[] { PreferredPackRoot, "Assets" }
                : new[] { "Assets" };

            var guidSet = new HashSet<string>(AssetDatabase.FindAssets("t:Prefab", folders));
            foreach (string modelGuid in AssetDatabase.FindAssets("t:Model", folders))
                guidSet.Add(modelGuid);
            string[] guids = guidSet.ToArray();

            var bestByCategory = new Dictionary<string, NemesisAssetEntry>(StringComparer.OrdinalIgnoreCase);
            var allMatches = new List<NemesisAssetEntry>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                path = path.Replace(Path.DirectorySeparatorChar, '/');

                if (path.StartsWith("Assets/NemesisContent/", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("Assets/Nemesis/", StringComparison.OrdinalIgnoreCase))
                    continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                string fileName = Path.GetFileNameWithoutExtension(path);
                string searchable = (fileName + " " + path).ToLowerInvariant();

                foreach (Rule rule in Rules)
                {
                    string normalizedName = fileName.ToLowerInvariant();
                    int score = Score(searchable, normalizedName, rule.Keywords);

                    if (rule.Category == "chest" &&
                        (normalizedName.Contains("chestplate") ||
                         normalizedName.Contains("chest armor") ||
                         normalizedName.Contains("chest armour")))
                        score = 0;

                    if (rule.Category == "sword" &&
                        (normalizedName.Contains("greatsword") ||
                         normalizedName.Contains("claymore") ||
                         normalizedName.Contains("zweihander")))
                        score = 0;

                    if (score > 0 && path.StartsWith(PreferredPackRoot, StringComparison.OrdinalIgnoreCase))
                        score += 30;
                    if (score <= 0) continue;

                    var entry = new NemesisAssetEntry
                    {
                        category = rule.Category,
                        assetName = fileName,
                        sourcePath = path,
                        prefab = prefab,
                        matchScore = score
                    };
                    allMatches.Add(entry);

                    if (!bestByCategory.TryGetValue(rule.Category, out NemesisAssetEntry current) ||
                        score > current.matchScore)
                        bestByCategory[rule.Category] = entry;
                }
            }

            allMatches.Sort((a, b) =>
            {
                int byCategory = string.Compare(a.category, b.category, StringComparison.OrdinalIgnoreCase);
                if (byCategory != 0) return byCategory;
                int byScore = b.matchScore.CompareTo(a.matchScore);
                return byScore != 0 ? byScore : string.Compare(a.assetName, b.assetName, StringComparison.OrdinalIgnoreCase);
            });

            catalog.ReplaceEntries(allMatches);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            string report = BuildReport(bestByCategory, allMatches.Count, guids.Length);
            File.WriteAllText(ReportPath, report, new UTF8Encoding(false));
            AssetDatabase.Refresh();

            if (bestByCategory.Count == 0)
            {
                Debug.LogWarning("[Nemesis] No matching imported prefabs were found. The prototype will keep its generated fallback visuals.");
                EditorUtility.DisplayDialog("Nemesis Asset Scan",
                    "No matching imported prefabs were found. The catalog was created, but fallback visuals remain active.\n\nExpected pack folder: " + PreferredPackRoot,
                    "OK");
            }
            else
            {
                Debug.Log("[Nemesis] Imported asset catalog created. Categories: " +
                    string.Join(", ", bestByCategory.Keys.OrderBy(x => x).ToArray()));
                EditorUtility.DisplayDialog("Nemesis Asset Scan",
                    "Imported prefabs indexed: " + allMatches.Count +
                    "\nCategories detected: " + bestByCategory.Count +
                    "\n\nReport: " + ReportPath,
                    "OK");
            }
        }

        private static int Score(string searchable, string fileName, string[] keywords)
        {
            int best = 0;
            foreach (string keyword in keywords)
            {
                string token = keyword.ToLowerInvariant();
                if (!searchable.Contains(token)) continue;

                int score = fileName.Contains(token) ? 100 + token.Length : 20 + token.Length;
                if (score > best) best = score;
            }
            return best;
        }

        private static string BuildReport(Dictionary<string, NemesisAssetEntry> best, int matchCount, int prefabCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Nemesis imported asset scan");
            sb.AppendLine();
            sb.AppendLine("Generated by Tools > Nemesis > Scan Imported Asset Packs.");
            sb.AppendLine();
            sb.AppendLine("- Prefabs scanned: " + prefabCount);
            sb.AppendLine("- Category matches indexed: " + matchCount);
            sb.AppendLine("- Categories with a selected asset: " + best.Count);
            sb.AppendLine();
            sb.AppendLine("| Role | Selected prefab | Source path |");
            sb.AppendLine("|---|---|---|");

            foreach (string category in best.Keys.OrderBy(x => x))
            {
                NemesisAssetEntry entry = best[category];
                sb.AppendLine("| " + category + " | " + entry.assetName.Replace("|", "\\|") +
                    " | " + entry.sourcePath.Replace("|", "\\|") + " |");
            }

            if (best.Count == 0)
                sb.AppendLine("No matching imported prefabs were found. This is expected when the source asset folders were not included in the project archive.");

            sb.AppendLine();
            sb.AppendLine("The scan indexes prefabs already present under Assets. It does not download third-party packages or bypass publisher licensing.");
            return sb.ToString();
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
