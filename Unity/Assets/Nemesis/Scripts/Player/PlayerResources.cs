using UnityEngine;
namespace Nemesis.Player {
 public sealed class PlayerResources : MonoBehaviour {
  [SerializeField] private float maxStamina=100f, maxEssence=100f, staminaRegen=16f, sprintDrain=24f;
  public float Stamina {get;private set;} public float Essence {get;private set;}
  public float MaxStamina=>maxStamina; public float MaxEssence=>maxEssence; public bool IsSprinting{get;private set;}
  private void Awake(){Stamina=maxStamina; Essence=maxEssence;}
  private void Update(){IsSprinting=Input.GetKey(KeyCode.LeftShift)&&Stamina>0.1f; if(IsSprinting) Stamina=Mathf.Max(0,Stamina-sprintDrain*Time.deltaTime); else Stamina=Mathf.Min(maxStamina,Stamina+staminaRegen*Time.deltaTime);}
  public bool SpendStamina(float amount){if(amount<0||Stamina<amount)return false; Stamina-=amount; return true;}
  public bool SpendEssence(float amount){if(amount<0||Essence<amount)return false; Essence-=amount; return true;}
  public void RestoreEssence(float amount){Essence=Mathf.Clamp(Essence+amount,0,maxEssence);}
 }
}