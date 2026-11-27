using UnityEngine;
using Nemesis.CameraSystem;
using Nemesis.Combat;
using Nemesis.Enemies;
using Nemesis.Player;
using Nemesis.Presentation;
using Nemesis.UI;
using Nemesis.Content;
using Nemesis.World;

namespace Nemesis.Bootstrap
{
    public sealed class NemesisBootstrap : MonoBehaviour
    {
        [SerializeField] private int worldSeed = 20261127;
        [SerializeField] private float cameraSize = 11f;
        private NemesisAssetCatalog assetCatalog;

        private void Start()
        {
            assetCatalog = Resources.Load<NemesisAssetCatalog>("NemesisAssetCatalog");
            CreateLighting();

            NemesisWorldLayout layout = NemesisWorldGenerator.Generate(
                transform, assetCatalog, worldSeed);

            Transform player = CreatePlayer(layout.PlayerSpawn);
            CreateEnemies(player, layout.EnemySpawns);
            CreateBoss(player, layout.BossSpawn);
            CreateMerchant(layout.MerchantSpawn);
            CreateCamera(player);
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientLight = new Color(.24f, .22f, .27f);
            RenderSettings.ambientIntensity = .9f;

            if (Object.FindObjectOfType<Light>() != null) return;

            GameObject lightObject = new GameObject("Ashen Reach Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(.82f, .79f, .88f);
            light.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
        }

        private Transform CreatePlayer(Vector3 spawn)
        {
            GameObject player = new GameObject("Player_Nemesis");
            player.transform.position = spawn;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.9f;
            controller.radius = .38f;
            controller.center = new Vector3(0f, .95f, 0f);

            player.AddComponent<Health>().Initialize(100f, 5f);
            player.AddComponent<PlayerResources>();
            player.AddComponent<TopDownPlayerController>();
            player.AddComponent<PlayerAbilityController>();
            player.AddComponent<NemesisHUD>();

            NemesisCharacterVisual.Create("Player_Armored", NemesisCharacterStyle.Player)
                .transform.SetParent(player.transform, false);

            return player.transform;
        }

        private void CreateEnemies(Transform target, Vector3[] spawns)
        {
            if (spawns == null) return;

            for (int i = 0; i < spawns.Length; i++)
            {
                GameObject enemy = new GameObject("Enemy_AshRaider_" + (i + 1));
                enemy.transform.position = spawns[i];

                CapsuleCollider collider = enemy.AddComponent<CapsuleCollider>();
                collider.height = 2f;
                collider.radius = .42f;

                enemy.AddComponent<Health>().Initialize(55f, 2f);
                enemy.AddComponent<EnemyBrain>().Initialize(target, 18f, 1.4f, 2.4f, 9f, 1.8f);

                NemesisCharacterVisual.Create("Ash_Raider_Visual_" + (i + 1), NemesisCharacterStyle.Raider)
                    .transform.SetParent(enemy.transform, false);
            }
        }

        private void CreateBoss(Transform target, Vector3 spawn)
        {
            GameObject boss = new GameObject("Boss_Ashen_Warlord");
            boss.transform.position = spawn;

            CapsuleCollider collider = boss.AddComponent<CapsuleCollider>();
            collider.height = 2.8f;
            collider.radius = .72f;

            boss.AddComponent<Health>().Initialize(450f, 8f);
            boss.AddComponent<NemesisBossBrain>().Initialize(target);

            NemesisCharacterVisual.Create("Ashen_Warlord_Visual", NemesisCharacterStyle.Boss)
                .transform.SetParent(boss.transform, false);
        }

        private static void CreateMerchant(Vector3 spawn)
        {
            GameObject npc = new GameObject("NPC_Mira_Merchant");
            npc.transform.position = spawn;

            NemesisCharacterVisual.Create("Mira_Visual", NemesisCharacterStyle.Merchant)
                .transform.SetParent(npc.transform, false);
        }

        private void CreateCamera(Transform target)
        {
            GameObject cameraObject = new GameObject("IsometricCamera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = cameraSize;
            camera.backgroundColor = new Color(.055f, .045f, .065f);
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 250f;
            cameraObject.tag = "MainCamera";

            cameraObject.AddComponent<IsometricCameraRig>().SetTarget(target);
        }
    }
}
