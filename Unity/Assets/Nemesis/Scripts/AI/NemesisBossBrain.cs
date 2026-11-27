using UnityEngine;
using Nemesis.Combat;
using Nemesis.Presentation;
using Nemesis.AI;

namespace Nemesis.Enemies
{
    public sealed class NemesisBossBrain : MonoBehaviour
    {
        [SerializeField] private float detectionRange=24f, moveSpeed=2.1f, meleeRange=3.1f;
        [SerializeField] private float attackCooldown=4.2f, damage=18f, slamRadius=5f;
        private Transform target;
        private Health targetHealth, ownHealth;
        private CharacterController motor;
        private float verticalVelocity;
        private NemesisCharacterVisual visual;
        private float nextAttack;
        private bool windingUp; private bool adapted;

        public void Initialize(Transform player)
        {
            target=player; ownHealth=GetComponent<Health>(); motor=GetComponent<CharacterController>();
            visual=GetComponentInChildren<NemesisCharacterVisual>();
            if(ownHealth!=null) ownHealth.Damaged+=OnDamaged;
        }

        private void OnDestroy(){if(ownHealth!=null)ownHealth.Damaged-=OnDamaged;}
        private void OnDamaged(float amount)
        {
            if (visual != null) visual.PlayAction("BossSlam", .45f);
            if (adapted) return;
            NemesisPlayerStyle style = NemesisPlayerMemory.DominantStyle;
            if (style == NemesisPlayerStyle.Balanced) return;
            adapted = true;
            switch (style)
            {
                case NemesisPlayerStyle.Melee:
                    if (ownHealth != null) ownHealth.SetDamageResistance(.12f);
                    break;
                case NemesisPlayerStyle.Abilities:
                    moveSpeed *= 1.12f;
                    attackCooldown *= .94f;
                    break;
                case NemesisPlayerStyle.Dodge:
                    slamRadius += .65f;
                    break;
            }
        }

        private void Update()
        {
            if(target==null||ownHealth==null||!ownHealth.IsAlive)return;
            if(visual==null)visual=GetComponentInChildren<NemesisCharacterVisual>();
            if(targetHealth==null)targetHealth=target.GetComponent<Health>();
            if(targetHealth==null||!targetHealth.IsAlive)return;
            Vector3 delta=target.position-transform.position; delta.y=0;
            float distance=delta.magnitude;
            if(distance>detectionRange)return;
            if(distance>meleeRange&&!windingUp)
            {
                Vector3 direction=delta.normalized;
                if(direction.sqrMagnitude>.001f)transform.rotation=Quaternion.LookRotation(direction);
                Move(direction);
                if(visual!=null)visual.SetMotion(moveSpeed,false);
                return;
            }
            Move(Vector3.zero);
            if(Time.time>=nextAttack&&!windingUp)StartCoroutine(Slam());
        }

        private void Move(Vector3 direction)
        {
            if(motor==null){transform.position+=direction*(moveSpeed*Time.deltaTime);return;}
            if(motor.isGrounded&&verticalVelocity<0f)verticalVelocity=-2f;
            else verticalVelocity+=-22f*Time.deltaTime;
            motor.Move((direction*moveSpeed+Vector3.up*verticalVelocity)*Time.deltaTime);
        }

        private System.Collections.IEnumerator Slam()
        {
            windingUp=true; nextAttack=Time.time+attackCooldown;
            if(visual!=null)visual.PlayAction("BossSlam",1.1f);
            yield return new WaitForSeconds(1.0f);
            if(target!=null&&Vector3.Distance(new Vector3(target.position.x,0,target.position.z),
                new Vector3(transform.position.x,0,transform.position.z))<=slamRadius)
            {
                float amount=CombatRules.CalculateDamage(damage,1f,targetHealth!=null?targetHealth.Defense:0f);
                if(targetHealth!=null)targetHealth.ApplyDamage(amount);
            }
            windingUp=false;
        }
    }
}
