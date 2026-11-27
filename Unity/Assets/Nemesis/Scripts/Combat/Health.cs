using System;
using UnityEngine;
namespace Nemesis.Combat {
 public sealed class Health:MonoBehaviour {
  [SerializeField,Min(1f)] private float maximum=100f; [SerializeField] private bool destroyOnDeath=true;
  private float shield,shieldUntil,vulnerableUntil,vulnerabilityBonus;
  public float Current{get;private set;} public float Maximum=>maximum; public float Defense{get;private set;} public bool IsAlive=>Current>0f;
  public float VulnerabilityMultiplier=>Time.time<vulnerableUntil?1f+vulnerabilityBonus:1f;
  public event Action<float,float> Changed; public event Action<float> Damaged; public event Action Died;
  private void Awake(){if(Current<=0f)Current=maximum;}
  public void Initialize(float maxHealth,float defense=0f){maximum=Mathf.Max(1f,maxHealth);Defense=Mathf.Max(0f,defense);Current=maximum;shield=0f;}
  public bool ApplyDamage(float amount){
   if(!IsAlive||amount<=0f)return false;
   if(Time.time<shieldUntil&&shield>0f){float absorbed=Mathf.Min(shield,amount);shield-=absorbed;amount-=absorbed;}else if(Time.time>=shieldUntil)shield=0f;
   if(amount>0f){Current=Mathf.Max(0f,Current-amount);Damaged?.Invoke(amount);Changed?.Invoke(Current,maximum);}
   if(Current<=0f){Died?.Invoke();if(destroyOnDeath)Destroy(gameObject);}return true;
  }
  public void GrantShield(float amount,float duration){shield=Mathf.Max(shield,amount);shieldUntil=Time.time+Mathf.Max(0f,duration);}
  public void ApplyVulnerability(float bonus,float duration){vulnerabilityBonus=Mathf.Max(vulnerabilityBonus,bonus);vulnerableUntil=Mathf.Max(vulnerableUntil,Time.time+duration);}
  public void Restore(float amount){if(!IsAlive||amount<=0f)return;Current=Mathf.Min(maximum,Current+amount);Changed?.Invoke(Current,maximum);}
 }
}