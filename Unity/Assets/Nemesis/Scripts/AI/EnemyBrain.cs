using UnityEngine;
using Nemesis.Combat;
using Nemesis.Presentation;
using Nemesis.AI;
namespace Nemesis.Enemies {
 public sealed class EnemyBrain:MonoBehaviour {
  [SerializeField] private float aggroRadius=18f,attackRange=1.4f,moveSpeed=2.4f,damage=9f,attackCooldown=1.8f,alertDuration=12f;
  private Transform target;private Health targetHealth,ownHealth;private CharacterController motor;private NemesisCharacterVisual visual;private float nextAttackTime,provokedUntil,verticalVelocity;private bool adapted;
  public void Initialize(Transform chaseTarget,float aggro,float range,float speed,float hitDamage,float cooldown){target=chaseTarget;aggroRadius=aggro;attackRange=range;moveSpeed=speed;damage=hitDamage;attackCooldown=cooldown;}
  private void Awake(){ownHealth=GetComponent<Health>();motor=GetComponent<CharacterController>();if(ownHealth!=null)ownHealth.Damaged+=OnDamaged;}
  private void OnDestroy(){if(ownHealth!=null)ownHealth.Damaged-=OnDamaged;}
  private void OnDamaged(float amount)
  {
   provokedUntil=Time.time+alertDuration;
   if(adapted)return;
   adapted=true;
   switch(NemesisPlayerMemory.DominantStyle)
   {
    case NemesisPlayerStyle.Melee: if(ownHealth!=null)ownHealth.SetDamageResistance(.10f);attackRange+=.2f;break;
    case NemesisPlayerStyle.Abilities: moveSpeed*=1.12f;attackCooldown*=.94f;break;
    case NemesisPlayerStyle.Dodge: attackRange+=.55f;attackCooldown*=.92f;break;
   }
  }
  private void Update(){
   if(visual==null)visual=GetComponentInChildren<NemesisCharacterVisual>();
   if(target==null||Time.time>provokedUntil)return;if(targetHealth==null)targetHealth=target.GetComponent<Health>();if(targetHealth==null||!targetHealth.IsAlive)return;
   Vector3 offset=target.position-transform.position;offset.y=0;float distance=offset.magnitude;
   if(distance>aggroRadius*2.5f){provokedUntil=0;return;}
   if(distance>attackRange){Vector3 dir=offset.normalized;if(dir.sqrMagnitude>0.001f)transform.rotation=Quaternion.LookRotation(dir);Move(dir);if(visual!=null)visual.SetMotion(moveSpeed,false);return;}
   Move(Vector3.zero);
   if(visual!=null)visual.SetMotion(0f,false);
   if(Time.time>=nextAttackTime){nextAttackTime=Time.time+attackCooldown;if(visual!=null)visual.PlayAction("Attack",.35f);float dealt=CombatRules.CalculateDamage(damage,1f,targetHealth.Defense);targetHealth.ApplyDamage(dealt);}
  }

  private void Move(Vector3 direction)
  {
   if(motor==null){transform.position+=direction*(moveSpeed*Time.deltaTime);return;}
   if(motor.isGrounded&&verticalVelocity<0f)verticalVelocity=-2f;
   else verticalVelocity+=-22f*Time.deltaTime;
   Vector3 velocity=direction*moveSpeed+Vector3.up*verticalVelocity;
   motor.Move(velocity*Time.deltaTime);
  }
 }
}