using UnityEngine;
using Nemesis.Combat;
using Nemesis.Presentation;
namespace Nemesis.Enemies {
 public sealed class EnemyBrain:MonoBehaviour {
  [SerializeField] private float aggroRadius=18f,attackRange=1.4f,moveSpeed=2.4f,damage=9f,attackCooldown=1.8f,alertDuration=12f;
  private Transform target;private Health targetHealth,ownHealth;private NemesisCharacterVisual visual;private float nextAttackTime,provokedUntil;
  public void Initialize(Transform chaseTarget,float aggro,float range,float speed,float hitDamage,float cooldown){target=chaseTarget;aggroRadius=aggro;attackRange=range;moveSpeed=speed;damage=hitDamage;attackCooldown=cooldown;}
  private void Awake(){ownHealth=GetComponent<Health>();if(ownHealth!=null)ownHealth.Damaged+=OnDamaged;}
  private void OnDestroy(){if(ownHealth!=null)ownHealth.Damaged-=OnDamaged;}
  private void OnDamaged(float amount){provokedUntil=Time.time+alertDuration;}
  private void Update(){
   if(visual==null)visual=GetComponentInChildren<NemesisCharacterVisual>();
   if(target==null||Time.time>provokedUntil)return;if(targetHealth==null)targetHealth=target.GetComponent<Health>();if(targetHealth==null||!targetHealth.IsAlive)return;
   Vector3 offset=target.position-transform.position;offset.y=0;float distance=offset.magnitude;
   if(distance>aggroRadius*2.5f){provokedUntil=0;return;}
   if(distance>attackRange){Vector3 dir=offset.normalized;transform.position+=dir*(moveSpeed*Time.deltaTime);if(dir.sqrMagnitude>0.001f)transform.rotation=Quaternion.LookRotation(dir);if(visual!=null)visual.SetMotion(moveSpeed,false);return;}
   if(visual!=null)visual.SetMotion(0f,false);
   if(Time.time>=nextAttackTime){nextAttackTime=Time.time+attackCooldown;if(visual!=null)visual.PlayAction("Attack",.35f);float dealt=CombatRules.CalculateDamage(damage,1f,targetHealth.Defense);targetHealth.ApplyDamage(dealt);}
  }
 }
}