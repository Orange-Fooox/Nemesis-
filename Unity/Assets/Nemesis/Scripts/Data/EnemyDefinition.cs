using UnityEngine;

namespace Nemesis.Data
{
    [CreateAssetMenu(menuName = "Nemesis/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string enemyId = "ash_raider";
        [SerializeField] private string displayName = "Ash Raider";
        [SerializeField, Min(1f)] private float maximumHealth = 55f;
        [SerializeField, Min(0f)] private float damage = 7f;
        [SerializeField, Min(0f)] private float moveSpeed = 2.4f;
        [SerializeField, Min(0f)] private float aggroRadius = 4f;
        [SerializeField, Min(0f)] private float attackRange = 1.4f;

        public string EnemyId => enemyId;
        public string DisplayName => displayName;
        public float MaximumHealth => maximumHealth;
        public float Damage => damage;
        public float MoveSpeed => moveSpeed;
        public float AggroRadius => aggroRadius;
        public float AttackRange => attackRange;
    }
}
