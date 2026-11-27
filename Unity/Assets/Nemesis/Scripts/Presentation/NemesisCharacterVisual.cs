using UnityEngine;
using Nemesis.Content;

namespace Nemesis.Presentation
{
    public enum NemesisCharacterStyle { Player, Raider, Boss, Merchant }

    /// <summary>
    /// Lightweight stylized low-poly character kit. Geometry is generated from Unity primitives
    /// so the prototype has visible characters before external licensed art is imported.
    /// </summary>
    public sealed class NemesisCharacterVisual : MonoBehaviour
    {
        private Transform torso, head, leftArm, rightArm, leftLeg, rightLeg, weapon;
        private Vector3 leftArmRest, rightArmRest, leftLegRest, rightLegRest;
        private Material cloth, metal, accent;
        private float gait;
        private float actionUntil;
        private string action = "Idle";

        public static GameObject Create(string objectName, NemesisCharacterStyle style)
        {
            var root = new GameObject(objectName);
            var visual = root.AddComponent<NemesisCharacterVisual>();
            visual.Build(style);
            return root;
        }

        private void Build(NemesisCharacterStyle style)
        {
            Color clothColor = style == NemesisCharacterStyle.Player ? new Color(.12f,.10f,.17f) :
                style == NemesisCharacterStyle.Boss ? new Color(.20f,.055f,.075f) :
                style == NemesisCharacterStyle.Merchant ? new Color(.24f,.18f,.11f) : new Color(.25f,.10f,.12f);
            Color metalColor = style == NemesisCharacterStyle.Boss ? new Color(.16f,.17f,.20f) : new Color(.32f,.34f,.38f);
            Color glowColor = style == NemesisCharacterStyle.Boss ? new Color(1f,.16f,.055f) :
                style == NemesisCharacterStyle.Merchant ? new Color(.8f,.57f,.18f) : new Color(.35f,.65f,.8f);
            cloth = MakeMaterial(clothColor); metal = MakeMaterial(metalColor); accent = MakeMaterial(glowColor);

            torso = Part("Torso", PrimitiveType.Capsule, new Vector3(0,1.05f,0), new Vector3(.62f,.72f,.38f), cloth);
            head = Part("Head", PrimitiveType.Sphere, new Vector3(0,1.95f,0), new Vector3(.42f,.44f,.42f), metal);
            Part("Shoulder_L", PrimitiveType.Sphere, new Vector3(-.43f,1.48f,0), new Vector3(.34f,.28f,.34f), metal);
            Part("Shoulder_R", PrimitiveType.Sphere, new Vector3(.43f,1.48f,0), new Vector3(.34f,.28f,.34f), metal);
            leftArm = Limb("Arm_L", new Vector3(-.52f,1.12f,0), new Vector3(.20f,.58f,.22f), cloth);
            rightArm = Limb("Arm_R", new Vector3(.52f,1.12f,0), new Vector3(.20f,.58f,.22f), cloth);
            leftLeg = Limb("Leg_L", new Vector3(-.19f,.42f,0), new Vector3(.23f,.68f,.25f), metal);
            rightLeg = Limb("Leg_R", new Vector3(.19f,.42f,0), new Vector3(.23f,.68f,.25f), metal);
            leftArmRest=leftArm.localEulerAngles; rightArmRest=rightArm.localEulerAngles;
            leftLegRest=leftLeg.localEulerAngles; rightLegRest=rightLeg.localEulerAngles;

            Part("Belt", PrimitiveType.Cube, new Vector3(0,.76f,0), new Vector3(.68f,.13f,.42f), accent);
            Part("ChestSigil", PrimitiveType.Sphere, new Vector3(0,1.25f,-.205f), new Vector3(.16f,.20f,.08f), accent);
            weapon = Part("Weapon_Handle", PrimitiveType.Cylinder, new Vector3(.77f,1.03f,0), new Vector3(.07f,.50f,.07f), metal);
            weapon.localRotation = Quaternion.Euler(0,0,-28);
            Part("Weapon_Blade", PrimitiveType.Cube, new Vector3(.88f,1.43f,0), new Vector3(.12f,.66f,.045f), accent).localRotation=Quaternion.Euler(0,0,-28);

            if (style == NemesisCharacterStyle.Boss)
            {
                Part("CrownHorn_L",PrimitiveType.Cylinder,new Vector3(-.22f,2.28f,0),new Vector3(.18f,.62f,.18f),accent).localRotation=Quaternion.Euler(0,0,22);
                Part("CrownHorn_R",PrimitiveType.Cylinder,new Vector3(.22f,2.28f,0),new Vector3(.18f,.62f,.18f),accent).localRotation=Quaternion.Euler(0,0,-22);
                Part("BackSpikes",PrimitiveType.Cube,new Vector3(0,1.48f,.27f),new Vector3(.72f,.70f,.20f),metal);
            }
            else if (style == NemesisCharacterStyle.Merchant)
            {
                Part("Pack",PrimitiveType.Cube,new Vector3(0,1.18f,.28f),new Vector3(.48f,.62f,.30f),accent);
                weapon.gameObject.SetActive(false);
            }

            AttachImportedEquipment(style);
        }

        private void AttachImportedEquipment(NemesisCharacterStyle style)
        {
            NemesisAssetCatalog catalog = Resources.Load<NemesisAssetCatalog>("NemesisAssetCatalog");
            if (catalog == null || style == NemesisCharacterStyle.Merchant) return;

            if (style == NemesisCharacterStyle.Raider)
            {
                AttachImported(catalog.FindPrefab("dagger"), "Imported_Dagger",
                    new Vector3(.48f, 1.0f, -.08f), new Vector3(0f, 0f, -18f), .85f);
                return;
            }

            bool hasHelmet = AttachImported(catalog.FindPrefab("helmet"), "Imported_Helmet",
                new Vector3(0f, 2.05f, 0f), Vector3.zero, .52f);
            bool hasArmor = AttachImported(catalog.FindPrefab("chest_armor"), "Imported_ChestArmor",
                new Vector3(0f, 1.18f, -.015f), Vector3.zero, .82f);

            GameObject weaponPrefab = catalog.FindPrefab(style == NemesisCharacterStyle.Boss ? "greatsword" : "sword");
            if (weaponPrefab == null) weaponPrefab = catalog.FindPrefab("greatsword");
            bool hasWeapon = AttachImported(weaponPrefab, "Imported_Weapon",
                new Vector3(.68f, 1.02f, -.06f), new Vector3(0f, 0f, -28f),
                style == NemesisCharacterStyle.Boss ? 1.55f : 1.18f);

            bool hasShield = style == NemesisCharacterStyle.Player &&
                AttachImported(catalog.FindPrefab("shield"), "Imported_Shield",
                    new Vector3(-.62f, 1.05f, -.02f), new Vector3(0f, 0f, 90f), .78f);

            if (hasWeapon && weapon != null) weapon.gameObject.SetActive(false);
        }

        private bool AttachImported(GameObject prefab, string objectName, Vector3 localAnchor,
            Vector3 localEuler, float targetSize)
        {
            if (prefab == null) return false;

            GameObject instance = Instantiate(prefab, transform, false);
            instance.name = objectName;
            instance.transform.localPosition = localAnchor;
            instance.transform.localRotation = Quaternion.Euler(localEuler);

            Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
            foreach (Collider item in colliders) item.enabled = false;

            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

                float largestDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                float scaleFactor = targetSize / Mathf.Max(.001f, largestDimension);
                instance.transform.localScale *= scaleFactor;

                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                Vector3 target = transform.TransformPoint(localAnchor);
                instance.transform.position += target - bounds.center;
            }

            return true;
        }

        private Transform Part(string partName, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            GameObject go=GameObject.CreatePrimitive(type);
            go.name=partName; go.transform.SetParent(transform,false);
            go.transform.localPosition=position; go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>();
            if(collider!=null)
            {
#if UNITY_EDITOR
                if(!Application.isPlaying)DestroyImmediate(collider); else Destroy(collider);
#else
                Destroy(collider);
#endif
            }
            var renderer=go.GetComponent<Renderer>(); renderer.sharedMaterial=material;
            return go.transform;
        }

        private Transform Limb(string partName, Vector3 position, Vector3 scale, Material material)
        {
            Transform pivot=new GameObject(partName+"_Pivot").transform;
            pivot.SetParent(transform,false); pivot.localPosition=position;
            Transform mesh=Part(partName,PrimitiveType.Capsule,Vector3.zero,scale,material);
            mesh.SetParent(pivot,false); mesh.localPosition=new Vector3(0,-.23f,0);
            return pivot;
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader=Shader.Find("Universal Render Pipeline/Lit");
            if(shader==null) shader=Shader.Find("Standard");
            var material=new Material(shader); material.color=color; return material;
        }

        public void SetMotion(float speed, bool sprinting)
        {
            gait=Mathf.MoveTowards(gait,speed>0.1f?(sprinting?1.8f:1f):0f,Time.deltaTime*5f);
            if(Time.time>=actionUntil) action="Idle";
        }

        public void PlayAction(string actionName,float duration=.35f)
        { action=actionName; actionUntil=Time.time+duration; }

        private void LateUpdate()
        {
            float phase=Time.time*(action=="Idle"?2.2f:10f);
            float swing=Mathf.Sin(phase)*gait*34f;
            leftLeg.localRotation=Quaternion.Euler(swing,0,0);
            rightLeg.localRotation=Quaternion.Euler(-swing,0,0);
            leftArm.localRotation=Quaternion.Euler(-swing*.65f,0,0);
            rightArm.localRotation=Quaternion.Euler(swing*.65f,0,0);
            torso.localPosition=new Vector3(0,1.05f,0);
            torso.localPosition+=Vector3.up*(Mathf.Abs(Mathf.Sin(phase*2f))*.035f*gait);
            if(action=="Attack") rightArm.localRotation=Quaternion.Euler(-105f+Mathf.Sin(Time.time*25f)*12f,0,-18f);
            else if(action=="Dodge") torso.localRotation=Quaternion.Euler(0,0,32f);
            else if(action=="BossSlam") { rightArm.localRotation=Quaternion.Euler(125f,0,0); leftArm.localRotation=Quaternion.Euler(125f,0,0); }
            else torso.localRotation=Quaternion.identity;
        }
    }
}
