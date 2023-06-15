using Game;
using Game.Managers;
using UnityEngine;

namespace GamePlay
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        [Tooltip("物品浮动频率/Frequency at which the item will move up and down")]
        public float VerticalBobFrequency = 1f;

        [Tooltip("物品浮动幅度/Distance the item will move up and down")]
        public float BobbingAmount = 1f;

        [Tooltip("物品转动速度")] public float RotatingSpeed = 360f;

        [Tooltip("物品拾取音效")] public AudioClip PickupSfx;
        [Tooltip("物品拾取特效")] public GameObject PickupVFXPrefab;
    
        public Rigidbody PickupRigidbody { get; private set; }

        private Collider m_Collider;
        private Vector3 m_StartPosition;
        private bool m_HasPlayedFeedback;
    
        protected virtual void Start()
        {
            PickupRigidbody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();

            PickupRigidbody.isKinematic = true;
            m_Collider.isTrigger = true;

            m_StartPosition = transform.position;
        }

        private void Update()
        {
            // Handle bobbing
            var bobbingAnimationPhase = (Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f + 0.3f) * BobbingAmount;
            transform.position = m_StartPosition + Vector3.up * bobbingAnimationPhase;
        
            transform.Rotate(Vector3.up, RotatingSpeed * Time.deltaTime, Space.Self);
        }

        private void OnTriggerEnter(Collider other)
        {
            var pickingPlayer = other.GetComponent<ThirdController>();
        
            if (pickingPlayer == null) return;
        
            OnPicked(pickingPlayer);
            var evt = Events.PickUpEvent;
            evt.Pickup = gameObject;
            EventManager.Broadcast(evt);
        }

        protected virtual void OnPicked(ThirdController controller)
        {
            PlayPickupFeedback();
        }

        public void PlayPickupFeedback()
        {
            if (m_HasPlayedFeedback) return;

            if (PickupSfx)
            {
                // todo: sfx
            }

            if (PickupVFXPrefab)
            {
                Instantiate(PickupVFXPrefab, transform.position, Quaternion.identity);
            }

            m_HasPlayedFeedback = true;
        }
    }
}
