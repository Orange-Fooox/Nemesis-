using UnityEngine;
using Nemesis.Controls;

namespace Nemesis.Combat
{
    public sealed class MeleeCombat : MonoBehaviour
    {
        [SerializeField] private float damage = 24f;
        [SerializeField] private float range = 1.7f;
        [SerializeField] private float radius = 0.85f;
        [SerializeField] private float cooldown = 0.55f;
        [SerializeField] private LayerMask hitMask = ~0;
        private float nextAttackTime;

        private void Update()
        {
            if (NemesisInput.WasPressed(NemesisInputAction.Attack) &&
                Time.time >= nextAttackTime)
                Attack();
        }

        private void Attack()
        {
            nextAttackTime = Time.time + cooldown;
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            Vector3 direction = transform.forward;
            Vector3 center = origin + direction * range;
            Collider[] hits = Physics.OverlapSphere(center, radius, hitMask, QueryTriggerInteraction.Ignore);

            foreach (Collider hit in hits)
            {
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
                Health health = hit.GetComponentInParent<Health>();
                if (health != null && health.IsAlive)
                {
                    health.ApplyDamage(damage);
                    break;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Vector3 center = transform.position + Vector3.up * 0.8f + transform.forward * range;
            Gizmos.DrawWireSphere(center, radius);
        }
    }
}
