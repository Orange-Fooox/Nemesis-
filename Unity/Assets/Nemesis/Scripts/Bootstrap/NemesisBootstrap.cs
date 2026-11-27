using UnityEngine;
using Nemesis.CameraSystem;
using Nemesis.Combat;
using Nemesis.Enemies;
using Nemesis.Player;
using Nemesis.Presentation;
using Nemesis.UI;
using Nemesis.Content;

namespace Nemesis.Bootstrap
{
    public sealed class NemesisBootstrap : MonoBehaviour
    {
        [SerializeField] private Vector3 playerSpawn = new Vector3(-3f,0f,0f);
        [SerializeField] private Vector3 enemySpawn = new Vector3(3f,0f,2f);
        [SerializeField] private Vector3 bossSpawn = new Vector3(0f,0f,8f);
        private NemesisAssetCatalog assetCatalog;

        private void Start()
        {
            assetCatalog = Resources.Load<NemesisAssetCatalog>("NemesisAssetCatalog");
            CreateLighting(); CreateArena();
            Transform player=CreatePlayer();
            CreateEnemy(player); CreateBoss(player); CreateMerchant(); CreateCamera(player);
        }

        private static void CreateLighting()
        {
            RenderSettings.ambientLight=new Color(.24f,.22f,.27f);
            if(FindObjectOfType<Light>()!=null)return;
            var go=new GameObject("Ashen Key Light");
            var light=go.AddComponent<Light>();
            light.type=LightType.Directional; light.intensity=1.1f;
            light.color=new Color(.78f,.76f,.9f);
            light.transform.rotation=Quaternion.Euler(50f,-35f,0f);
        }

        private void CreateArena()
        {
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name="Greybox_AshenFloor"; floor.transform.position=new Vector3(0,-.3f,0);
            floor.transform.localScale=new Vector3(28,.6f,28);
            floor.GetComponent<Renderer>().material.color=new Color(.16f,.15f,.18f);

            SpawnArenaProp("pillar","RuinedPillar_A",new Vector3(-9,0,5),new Vector3(1.2f,3.2f,1.2f),PrimitiveType.Cube,new Color(.22f,.21f,.24f));
            SpawnArenaProp("pillar","RuinedPillar_B",new Vector3(9,0,-5),new Vector3(1.2f,3.2f,1.2f),PrimitiveType.Cube,new Color(.22f,.21f,.24f));
            SpawnArenaProp("wall","BrokenWall",new Vector3(0,0,12),new Vector3(9,1.6f,.5f),PrimitiveType.Cube,new Color(.24f,.22f,.25f));
            Block("AshenAltar",new Vector3(0,.5f,-8),new Vector3(3,1,2));

            SpawnArenaProp("torch","SoulLantern_A",new Vector3(-11,0,-3),new Vector3(.35f,2.2f,.35f),PrimitiveType.Cylinder,new Color(.20f,.62f,.85f));
            SpawnArenaProp("torch","SoulLantern_B",new Vector3(11,0,7),new Vector3(.35f,2.2f,.35f),PrimitiveType.Cylinder,new Color(.20f,.62f,.85f));

            if (assetCatalog != null && assetCatalog.FindPrefab("tree") != null)
            {
                SpawnArenaProp("tree","AshenTree_A",new Vector3(-12,0,10),new Vector3(2.8f,4f,2.8f),PrimitiveType.Sphere,new Color(.18f,.29f,.19f));
                SpawnArenaProp("tree","AshenTree_B",new Vector3(12,0,-10),new Vector3(2.8f,4f,2.8f),PrimitiveType.Sphere,new Color(.18f,.29f,.19f));
            }

            if (assetCatalog != null && assetCatalog.FindPrefab("chest") != null)
                SpawnArenaProp("chest","AshenSupplyChest",new Vector3(-7,0,-7),new Vector3(1.2f,.9f,1.2f),PrimitiveType.Cube,new Color(.42f,.29f,.17f));
        }

        private void SpawnArenaProp(string category,string objectName,Vector3 groundPosition,Vector3 fallbackScale,PrimitiveType fallbackType,Color fallbackColor)
        {
            GameObject prefab=assetCatalog!=null?assetCatalog.FindPrefab(category):null;
            GameObject instance;

            if(prefab!=null)
            {
                instance=Instantiate(prefab,groundPosition,Quaternion.identity);
                Renderer[] renderers=instance.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length>0)
                {
                    Bounds bounds=renderers[0].bounds;
                    for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
                    float largest=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
                    instance.transform.localScale*=Mathf.Max(fallbackScale.x,Mathf.Max(fallbackScale.y,fallbackScale.z))/Mathf.Max(.001f,largest);
                    bounds=renderers[0].bounds;
                    for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);
                    Vector3 targetCenter=new Vector3(groundPosition.x,groundPosition.y+bounds.extents.y,groundPosition.z);
                    instance.transform.position+=targetCenter-bounds.center;
                }
            }
            else
            {
                instance=GameObject.CreatePrimitive(fallbackType);
                instance.transform.position=groundPosition+Vector3.up*(fallbackScale.y*.5f);
                instance.transform.localScale=fallbackScale;
                Renderer renderer=instance.GetComponent<Renderer>();
                if(renderer!=null)renderer.material.color=fallbackColor;
            }

            instance.name=objectName;
        }

        private static void Block(string name,Vector3 position,Vector3 scale)
        {
            var block=GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name=name; block.transform.position=position; block.transform.localScale=scale;
            block.GetComponent<Renderer>().material.color=new Color(.25f,.23f,.27f);
        }

        private Transform CreatePlayer()
        {
            var player=new GameObject("Player_Nemesis");
            player.transform.position=playerSpawn;
            var controller=player.AddComponent<CharacterController>();
            controller.height=1.9f; controller.radius=.38f; controller.center=new Vector3(0,.95f,0);
            player.AddComponent<Health>().Initialize(100,5);
            player.AddComponent<PlayerResources>();
            player.AddComponent<TopDownPlayerController>();
            player.AddComponent<PlayerAbilityController>();
            player.AddComponent<NemesisHUD>();
            NemesisCharacterVisual.Create("Player_Armored",NemesisCharacterStyle.Player).transform.SetParent(player.transform,false);
            return player.transform;
        }

        private void CreateEnemy(Transform target)
        {
            var enemy=new GameObject("Enemy_AshRaider");
            enemy.transform.position=enemySpawn;
            enemy.AddComponent<CapsuleCollider>().height=2f;
            enemy.AddComponent<Health>().Initialize(55,2);
            enemy.AddComponent<EnemyBrain>().Initialize(target,18,1.4f,2.4f,9,1.8f);
            NemesisCharacterVisual.Create("Ash_Raider_Visual",NemesisCharacterStyle.Raider).transform.SetParent(enemy.transform,false);
        }

        private void CreateBoss(Transform target)
        {
            var boss=new GameObject("Boss_Ashen_Warlord");
            boss.transform.position=bossSpawn;
            boss.AddComponent<CapsuleCollider>().height=2.8f;
            boss.AddComponent<Health>().Initialize(450,8);
            boss.AddComponent<NemesisBossBrain>().Initialize(target);
            NemesisCharacterVisual.Create("Ashen_Warlord_Visual",NemesisCharacterStyle.Boss).transform.SetParent(boss.transform,false);
        }

        private static void CreateMerchant()
        {
            var npc=new GameObject("NPC_Ashen_Merchant");
            npc.transform.position=new Vector3(-8f,0f,-7f);
            NemesisCharacterVisual.Create("Ashen_Merchant_Visual",NemesisCharacterStyle.Merchant).transform.SetParent(npc.transform,false);
        }

        private static void CreateCamera(Transform target)
        {
            var go=new GameObject("IsometricCamera");
            var camera=go.AddComponent<Camera>();
            camera.orthographic=true; camera.orthographicSize=9f;
            camera.backgroundColor=new Color(.055f,.045f,.065f);
            go.tag="MainCamera";
            go.AddComponent<IsometricCameraRig>().SetTarget(target);
        }
    }
}
