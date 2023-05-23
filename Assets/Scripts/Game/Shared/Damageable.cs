using UnityEngine;

namespace Game.Shared
{
    public class Damageable : MonoBehaviour
    {
        [Tooltip("Multiplier to apply to the received damage")]
        public float DamageMultiplier = 1f;

        [Range(0, 1)] [Tooltip("Multiplier to apply self damage")]
        public float SensibilityToSelfDamage = 0.5f;

        public Health Health { get; private set; }

        private void Awake()
        {
            // find the health component either at the same level, or higher in the hierarchy
            Health = GetComponent<Health>();
            if (!Health) Health = GetComponentInParent<Health>();
        }

        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if (!Health) return;

            var totalDamage = damage;

            // Skip the critical multiplier if it's from an explosion
            if (!isExplosionDamage) totalDamage *= DamageMultiplier;

            // potentially reduce damages if inflicted by self
            if (Health.gameObject == damageSource) totalDamage *= SensibilityToSelfDamage;

            Health.TakeDamage(totalDamage, damageSource);
        }
    }
}