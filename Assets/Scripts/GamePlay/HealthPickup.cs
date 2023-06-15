using Game.Shared;
using UnityEngine;

namespace GamePlay
{
    public class HealthPickup : Pickup
    {
        [Tooltip("Amount of health to heal on pickup")]
        public float HealAmount;

        protected override void OnPicked(ThirdController controller)
        {
            var playerHealth = controller.GetComponent<Health>();
            
            if (!playerHealth || !playerHealth.CanPickUp()) return;
            
            playerHealth.Heal(HealAmount);
            PlayPickupFeedback();
            Destroy(gameObject);
        }
    }
}