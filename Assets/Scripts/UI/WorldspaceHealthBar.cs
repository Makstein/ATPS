using Game.Shared;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class WorldspaceHealthBar : MonoBehaviour
    {
        [FormerlySerializedAs("health")] [Tooltip("Health component to track")]
        public Health Health;

        [Tooltip("Image component display health left")]
        public Image HealthBarImage;

        [FormerlySerializedAs("healthBarPivot")] [Tooltip("The floating healthbar pivot transform")]
        public Transform HealthBarPivot;

        [Tooltip("Whether the healthbar is visible when at full health or not")]
        public bool HideFullHealthBar = true;

        private Transform _mainCamera;


        private void Start()
        {
            _mainCamera = Camera.main.transform;
        }

        // Update is called once per frame
        private void Update()
        {
            HealthBarImage.fillAmount = Health.GetRatio();

            HealthBarPivot.LookAt(_mainCamera.transform.position);

            if (HideFullHealthBar) HealthBarPivot.gameObject.SetActive(HealthBarImage.fillAmount != 1);
        }
    }
}