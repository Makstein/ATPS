using System;
using System.Collections.Generic;
using Game;
using Game.Managers;
using Game.Shared;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace AI
{
    [RequireComponent(typeof(Health), typeof(Actor), typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Parameters")]
        [Tooltip("The Y height at which the enemy will be automatically killed(if it fall out of levels)")]
        public float SelfDestructYHeight = -20f;

        [Tooltip("The distance at which the enemy considers that it has reached its current path destination point")]
        public float PathReachingRadius = 2f;

        [Tooltip("The speed at which the enemy rotates")]
        public float OrientationSpeed = 10f;

        [Tooltip("Delay after death where the GameObject is destroyed (to allow animation)")]
        public float DeathDuration;

        [Header("Weapon Parameters")] [Tooltip("Allow weapon swapping for this enemy")]
        public bool SwapToNextWeapon;

        [Tooltip("The delay between a weapon swap and the next attack")]
        public float DelayAfterWeaponSwap;

        [Header("Eye color")] [Tooltip("Material for the eye color")]
        public Material EyeColorMaterial;

        [Tooltip("The default color of the bot's eye")] [ColorUsage(true, true)]
        public Color DefaultEyeColor;

        [Tooltip("The attack eye color of the bot's eye")] [ColorUsage(true, true)]
        public Color AttackEyeColor;

        [Header("Flash on hit")] [Tooltip("The material used for the body of hover bot")]
        public Material BodyMaterial;

        [Tooltip("The gradient representing the color of the flash on hit")] [GradientUsage(true)]
        public Gradient OnHitBodyGradient;

        [Tooltip("The duration of the flash on hit")]
        public float FlashOnHitDuration;

        [Header("Sounds")] [Tooltip("Sound played when receiving damage")]
        public AudioClip DamageTick;

        [Header("VFX")] [Tooltip("The VFX prefab spawned when the enemy dies")]
        public GameObject DeathVfx;

        [Tooltip("The point at which the death VFX is spawned")]
        public Transform DeathVfxSpawnPoint;

        [Header("Loot")] [Tooltip("The object this enemy can drop then dying")]
        public GameObject LootPrefab;

        [Tooltip("The chance the object has to drop")] [Range(0, 1)]
        public float DropRate = 1f;

        [Header("Debug display")] [Tooltip("Color of the sphere gizmo representing the path reaching range")]
        public Color PathReachingRangeColor = Color.yellow;

        [Tooltip("Color of the sphere gizmo representing the attack range")]
        public Color AttackRangeColor = Color.red;

        [Tooltip("Color of the sphere gizmo representing the detecting range")]
        public Color DetectionRangeColor = Color.blue;

        private Actor m_Actor;
        private ActorsManager m_ActorsManager;
        private MaterialPropertyBlock m_BodyFlashMaterialPropertyBlock;

        private readonly List<RendererIndexData> m_BodyRenderers = new();
        private int m_CurrentWeaponIndex;
        private EnemyManager m_EnemyManager;
        private MaterialPropertyBlock m_EyeColorPropertyBlock;

        private RendererIndexData m_EyeRendererData;
        private GameFlowManager m_GameFLowManager;
        private Health m_Health;
        private float m_LastTimeDamaged = float.NegativeInfinity;
        private readonly float m_LastTimeWeaponSwapped = Mathf.NegativeInfinity;

        // todo: weapon manage

        private NavigationModule m_NavigationModule;

        private int m_PathDestinationNodeIndex;
        private Collider[] m_SelfColliders;
        private bool m_WasDamagedThisFrame;

        public UnityAction onAttack;
        public UnityAction onDamaged;
        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;

        public PatrolPath PatrolPath { get; set; }
        public DetectionModule DetectionModule { get; private set; }
        public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;
        public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;
        public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
        public bool HadKnownTarget => DetectionModule.HadKnownTarget;
        public NavMeshAgent NavMeshAgent { get; private set; }

        private void Start()
        {
            m_EnemyManager = FindObjectOfType<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(m_EnemyManager, this);

            m_ActorsManager = FindObjectOfType<ActorsManager>();
            DebugUtility.HandleErrorIfNullFindObject<ActorsManager, EnemyManager>(m_ActorsManager, this);

            m_EnemyManager.RegisterEnemy(this);

            m_Health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, EnemyController>(m_Health, this, gameObject);

            m_Actor = GetComponent<Actor>();
            DebugUtility.HandleErrorIfNullGetComponent<Actor, EnemyController>(m_Actor, this, gameObject);

            NavMeshAgent = GetComponent<NavMeshAgent>();
            m_SelfColliders = GetComponentsInChildren<Collider>();

            m_GameFLowManager = FindObjectOfType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, EnemyController>(m_GameFLowManager, this);

            m_Health.OnDie += OnDie;
            m_Health.OnDamaged += OnDamaged;

            // todo: Weapon initialize

            var detectionModules = GetComponentsInChildren<DetectionModule>();
            DebugUtility.HandleErrorIfNoComponentFound<DetectionModule, EnemyController>(detectionModules.Length, this,
                gameObject);
            DebugUtility.HandleWarningIfDuplicateObjects<DetectionModule, EnemyController>(detectionModules.Length,
                this, gameObject);

            DetectionModule = detectionModules[0];
            DetectionModule.onDetectedTarget += OnDetectedTarget;
            DetectionModule.onLostTarget += OnLostTarget;
            onAttack += DetectionModule.OnAttack;

            var navigationModules = GetComponentsInChildren<NavigationModule>();
            DebugUtility.HandleWarningIfDuplicateObjects<NavigationModule, EnemyController>(navigationModules.Length,
                this, gameObject);

            if (navigationModules.Length > 0)
            {
                m_NavigationModule = navigationModules[0];
                NavMeshAgent.speed = m_NavigationModule.MoveSpeed;
                NavMeshAgent.angularSpeed = m_NavigationModule.AngularSpeed;
                NavMeshAgent.acceleration = m_NavigationModule.Acceleration;
            }

            foreach (var rd in GetComponentsInChildren<Renderer>(true))
                for (var i = 0; i < rd.sharedMaterials.Length; i++)
                {
                    if (rd.sharedMaterials[i] == EyeColorMaterial) m_EyeRendererData = new RendererIndexData(rd, i);

                    if (rd.sharedMaterials[i] == BodyMaterial) m_BodyRenderers.Add(new RendererIndexData(rd, i));
                }

            m_BodyFlashMaterialPropertyBlock = new MaterialPropertyBlock();

            if (m_EyeRendererData.Renderer == null) return;

            m_EyeColorPropertyBlock = new MaterialPropertyBlock();
            m_EyeColorPropertyBlock.SetColor("_EmissionColor", DefaultEyeColor);
            m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorPropertyBlock,
                m_EyeRendererData.MaterialIndex);
        }

        private void Update()
        {
            EnsureWithinLevelBounds();

            DetectionModule.HandleTargetDetection(m_Actor, m_SelfColliders);

            var currentColor = OnHitBodyGradient.Evaluate((Time.time - m_LastTimeDamaged) / FlashOnHitDuration);
            m_BodyFlashMaterialPropertyBlock.SetColor("_EmissionColor", currentColor);
            foreach (var data in m_BodyRenderers)
                data.Renderer.SetPropertyBlock(m_BodyFlashMaterialPropertyBlock, data.MaterialIndex);

            m_WasDamagedThisFrame = false;
        }

        private void EnsureWithinLevelBounds()
        {
            if (transform.position.y >= SelfDestructYHeight) return;

            Destroy(gameObject);
        }

        private void OnLostTarget()
        {
            onLostTarget?.Invoke();

            // Set the eye attack color and property block if eye renderer is set
            if (m_EyeRendererData.Renderer == null) return;

            m_EyeColorPropertyBlock.SetColor("_EmissionColor", AttackEyeColor);
            m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorPropertyBlock, m_EyeRendererData.MaterialIndex);
        }

        private void OnDetectedTarget()
        {
            onDetectedTarget?.Invoke();

            if (m_EyeRendererData.Renderer == null) return;

            m_EyeColorPropertyBlock.SetColor("_EmissionColor", AttackEyeColor);
            m_EyeRendererData.Renderer.SetPropertyBlock(m_EyeColorPropertyBlock, m_EyeRendererData.MaterialIndex);
        }

        public void OrientTowards(Vector3 lookPosition)
        {
            var lookDirection = Vector3.ProjectOnPlane(lookPosition - transform.position, Vector3.up).normalized;
            if (lookDirection.sqrMagnitude != 0.0f)
            {
                var targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation =
                    Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * OrientationSpeed);
            }
        }

        private bool IsPathValid()
        {
            return PatrolPath && PatrolPath.PathNodes.Count > 0;
        }

        public void SetPathDestinationToClosestNode()
        {
            if (IsPathValid())
            {
                var closestPathNodeIndex = 0;
                for (var i = 0; i < PatrolPath.PathNodes.Count; i++)
                {
                    var distanceToPathNode = PatrolPath.GetDistanceToNode(transform.position, i);
                    if (distanceToPathNode < PatrolPath.GetDistanceToNode(transform.position, closestPathNodeIndex))
                        closestPathNodeIndex = i;
                }

                m_PathDestinationNodeIndex = closestPathNodeIndex;
            }
            else
            {
                m_PathDestinationNodeIndex = 0;
            }
        }

        public Vector3 GetDestinationOnPath()
        {
            return IsPathValid() ? PatrolPath.GetPositionOfPathNode(m_PathDestinationNodeIndex) : transform.position;
        }

        public void UpdatePathDestination(bool inverseOrder = false)
        {
            if (!IsPathValid()) return;
            if ((transform.position - GetDestinationOnPath()).magnitude > PathReachingRadius) return;

            m_PathDestinationNodeIndex =
                inverseOrder ? m_PathDestinationNodeIndex - 1 : m_PathDestinationNodeIndex + 1;
            if (m_PathDestinationNodeIndex < 0) m_PathDestinationNodeIndex += PatrolPath.PathNodes.Count;

            if (m_PathDestinationNodeIndex >= PatrolPath.PathNodes.Count)
                m_PathDestinationNodeIndex -= PatrolPath.PathNodes.Count;
        }

        public void SetNavDestination(Vector3 destination)
        {
            if (NavMeshAgent) NavMeshAgent.SetDestination(destination);
        }

        public bool TryAttack(Vector3 enemyPosition)
        {
            if (m_GameFLowManager.GameIsEnding) return false;

            // todo: Rotate weapon

            if (m_LastTimeWeaponSwapped + DelayAfterWeaponSwap >= Time.time) return false;

            // todo: Shoot the weapon

            return true;
        }

        private bool TryDropItem()
        {
            if (DropRate == 0 || LootPrefab == null) return false;
            if (DropRate == 1) return true;
            return Random.value <= DropRate;
        }

        private void OnDamaged(float damage, GameObject damageSource)
        {
            if (damageSource == null || damageSource.GetComponent<EnemyController>()) return;

            DetectionModule.OnDamaged(damageSource);

            onDamaged?.Invoke();
            m_LastTimeDamaged = Time.time;

            // todo: play the damage tick sound

            m_WasDamagedThisFrame = true;
        }

        private void OnDie()
        {
            // todo: learn VFX and spawn a particle system when dying

            m_EnemyManager.UnRegisterEnemy(this);

            if (TryDropItem())
            {
                // todo: drop loot
            }

            Destroy(gameObject, DeathDuration);
        }

        [Serializable]
        public struct RendererIndexData
        {
            [FormerlySerializedAs("renderer")] public Renderer Renderer;

            public int MaterialIndex;

            public RendererIndexData(Renderer renderer, int index)
            {
                Renderer = renderer;
                MaterialIndex = index;
            }
        }
    }
}