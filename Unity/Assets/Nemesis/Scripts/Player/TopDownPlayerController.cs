using UnityEngine;
using Nemesis.Presentation;
using Nemesis.AI;
namespace Nemesis.Player {
 [RequireComponent(typeof(CharacterController))]
 public sealed class TopDownPlayerController:MonoBehaviour {
  [SerializeField] private float moveSpeed=5f,sprintMultiplier=1.45f,rotationSharpness=14f,gravity=-22f;
  private CharacterController controller; private PlayerResources resources; private NemesisCharacterVisual visual; private float verticalVelocity; private Vector3 dashVelocity; private float dashUntil;
  private void Awake(){controller=GetComponent<CharacterController>();resources=GetComponent<PlayerResources>(); visual=GetComponentInChildren<NemesisCharacterVisual>();}
  private void Update(){
   if(visual==null)visual=GetComponentInChildren<NemesisCharacterVisual>();
   Vector2 input=Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")),1f);
   Transform view=Camera.main!=null?Camera.main.transform:transform;
   Vector3 forward=Vector3.ProjectOnPlane(view.forward,Vector3.up).normalized,right=Vector3.ProjectOnPlane(view.right,Vector3.up).normalized;
   Vector3 move=(forward*input.y+right*input.x).normalized;
   if(move.sqrMagnitude>0.001f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),1f-Mathf.Exp(-rotationSharpness*Time.deltaTime));
   if(controller.isGrounded&&verticalVelocity<0)verticalVelocity=-2f; verticalVelocity+=gravity*Time.deltaTime;
   float speed=moveSpeed*(resources!=null&&resources.IsSprinting?sprintMultiplier:1f);
   Vector3 velocity=move*speed+Vector3.up*verticalVelocity+(Time.time<dashUntil?dashVelocity:Vector3.zero); if(visual!=null)visual.SetMotion(move.magnitude*(resources!=null&&resources.IsSprinting?1.5f:1f),resources!=null&&resources.IsSprinting);
   controller.Move(velocity*Time.deltaTime);
  }
  public void Dash(float distance,float duration,bool countAsDodge=true){dashVelocity=transform.forward*(distance/Mathf.Max(0.05f,duration));dashUntil=Time.time+duration; if(visual!=null)visual.PlayAction("Dodge",duration); if(countAsDodge)NemesisPlayerMemory.RecordDodge();}
 }
}