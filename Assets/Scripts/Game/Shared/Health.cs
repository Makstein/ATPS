using UnityEngine;
using UnityEngine.Events;

namespace Game.Shared
{
    public class Health : MonoBehaviour
    {
        [Tooltip("Maximum amount of health")] public float MaxHealth = 10f;

        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float CriticalHealthRatio = 0.3f;

        private bool m_IsDead;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction OnDie;
        public UnityAction<float> OnHealed;

        public float CurrentHealth { get; set; }
        public bool Invincible { get; set; }

        private void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public bool CanPickUp()
        {
            return CurrentHealth < MaxHealth;
        }

        public float GetRatio()
        {
            return CurrentHealth / MaxHealth;
        }

        public bool IsCritical()
        {
            return GetRatio() <= CriticalHealthRatio;
        }

        public void Heal(float healAmount)
        {
            var healthBefore = CurrentHealth;
            CurrentHealth += healAmount;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            var trueHealAmount = CurrentHealth - healthBefore;
            if (trueHealAmount > 0f)
                //呼叫所有订阅OnHealed时间的函数
                OnHealed?.Invoke(trueHealAmount);
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible) return;

            var healthBefore = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            var trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0) OnDamaged?.Invoke(trueDamageAmount, damageSource);

            HandleDeath();
        }

        public void Kill()
        {
            CurrentHealth = 0f;

            OnDamaged?.Invoke(MaxHealth, null);

            HandleDeath();
        }

        private void HandleDeath()
        {
            if (m_IsDead) return;

            if (CurrentHealth <= 0f)
            {
                m_IsDead = true;
                OnDie?.Invoke();
            }
        }
    }
}