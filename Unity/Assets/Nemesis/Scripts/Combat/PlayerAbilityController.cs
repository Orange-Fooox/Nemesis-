using UnityEngine;
using Nemesis.Player;
using Nemesis.Presentation;
using Nemesis.AI;
namespace Nemesis.Combat {
 [RequireComponent(typeof(PlayerResources))]
 public sealed class PlayerAbilityController:MonoBehaviour {
  [SerializeField] private float attack=12f,basicCooldown=0.65f;
  private PlayerResources resources; private TopDownPlayerController movement; private NemesisCharacterVisual visual; private float nextBasic;
  private readonly float[] readyAt=new float[5];
  private static readonly float[] Costs={15f,20f,30f,60f,25f},Cooldowns={6f,8f,14f,45f,18f};
  private void Awake(){resources=GetComponent<PlayerResources>();movement=GetComponent<TopDownPlayerController>(); visual=GetComponentInChildren<NemesisCharacterVisual>();}
  private void Update(){
   if(Input.GetMouseButtonDown(0)||Input.GetKeyDown(KeyCode.F))BasicAttack();
   if(Input.GetKeyDown(KeyCode.Space)&&resources.SpendStamina(25f)&&movement!=null)movement.Dash(8f,0.18f);
   if(Input.GetKeyDown(KeyCode.Z))Use(0);if(Input.GetKeyDown(KeyCode.X))Use(1);if(Input.GetKeyDown(KeyCode.C))Use(2);if(Input.GetKeyDown(KeyCode.V))Use(3);if(Input.GetKeyDown(KeyCode.B))Use(4);
  }
  private Health FindTarget(float range){
   Collider[] hits=Physics.OverlapSphere(transform.position,range);Health best=null;float nearest=range;
   foreach(Collider hit in hits){Health h=hit.GetComponentInParent<Health>();if(h==null||h.transform==transform||!h.IsAlive||h.GetComponent<Nemesis.Enemies.EnemyBrain>()==null&&h.GetComponent<Nemesis.Enemies.NemesisBossBrain>()==null)continue;float d=Vector3.Distance(transform.position,h.transform.position);if(d<nearest){nearest=d;best=h;}}
   return best;
  }
  private void BasicAttack(){if(Time.time<nextBasic)return;Health t=FindTarget(2.3f);if(t==null)return;nextBasic=Time.time+basicCooldown;if(visual==null)visual=GetComponentInChildren<NemesisCharacterVisual>();if(visual!=null)visual.PlayAction("Attack",.38f);Damage(t,1f);NemesisPlayerMemory.RecordMeleeHit();}
  private void Use(int id){
   if(Time.time<readyAt[id]||!resources.SpendEssence(Costs[id]))return;
   if(id==4){Health self=GetComponent<Health>();self.GrantShield(self.Maximum*0.30f,4f);NemesisPlayerMemory.RecordAbilityUse();readyAt[id]=Time.time+Cooldowns[id];return;}
   if(id==1){if(movement!=null){movement.Dash(12f,0.22f,false);Health dashTarget=FindTarget(13f);if(dashTarget!=null)Damage(dashTarget,1f);NemesisPlayerMemory.RecordAbilityUse();readyAt[id]=Time.time+Cooldowns[id];}else resources.RestoreEssence(Costs[id]);return;}
   float range=id==2?12f:id==3?14f:11f;Health target=FindTarget(range);if(target==null){resources.RestoreEssence(Costs[id]);return;}
   NemesisPlayerMemory.RecordAbilityUse();
   if(id==0)Damage(target,1.8f);
   if(id==2){foreach(Collider hit in Physics.OverlapSphere(target.transform.position,10f)){Health h=hit.GetComponentInParent<Health>();if(h!=null&&h.IsAlive&&(h.GetComponent<Nemesis.Enemies.EnemyBrain>()!=null||h.GetComponent<Nemesis.Enemies.NemesisBossBrain>()!=null))Damage(h,2.5f);}}
   if(id==3){Damage(target,4f);target.ApplyVulnerability(0.10f,5f);}
   readyAt[id]=Time.time+Cooldowns[id];
  }
  private void Damage(Health target,float multiplier){target.ApplyDamage(CombatRules.CalculateDamage(attack,multiplier,target.Defense,target.VulnerabilityMultiplier));}
 }
}