using System;
using Game.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        [Tooltip("Image component that display player's current health")]
        public Image HealthFillImage;

        private Health m_PlayerHealth;

        private void Start()
        {
            var thirdController = FindObjectOfType<ThirdController>();

            m_PlayerHealth = thirdController.GetComponent<Health>();
            
        }

        private void Update()
        {
            HealthFillImage.fillAmount = m_PlayerHealth.CurrentHealth / m_PlayerHealth.MaxHealth;
        }
    }
}