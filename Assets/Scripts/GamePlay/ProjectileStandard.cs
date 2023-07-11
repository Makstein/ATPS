using System;
using System.Collections.Generic;
using Game;
using Game.Shared;
using GamePlay.Managers;
using UnityEngine;

namespace GamePlay
{
    public class ProjectileStandard : ProjectileBase
    {
        private const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

        [Header("General")] [Tooltip("Radius of this projectile's collision detection")]
        public float Radius = 0.01f;

        [Tooltip("Transform representing the root of the projectile (used for accurate collision detection")]
        public Transform Root;

        [Tooltip("Transform representing the tip of the projectile (used for accurate collision detection")]
        public Transform Tip;

        [Tooltip("LifeTime of the projectile")]
        public float MaxLifeTime = 5f;

        [Tooltip("VFX prefab to spawn upon impact")]
        public GameObject ImpactVfx;

        [Tooltip("LifeTime of the VFX before being destroyed")]
        public float ImpactVfxLifeTime = 5f;

        [Tooltip("Offset along the hit normal where the VFX will be spawned")]
        public float ImpactVfxSpawnOffset = 0.1f;

        [Tooltip("Clip to play on impact")] public AudioClip ImpactSfxClip;

        [Tooltip("Layers the projectile can collide with")]
        public LayerMask HittableLayers = -1;

        [Header("Movement")] [Tooltip("Speed of the projectile")]
        public float Speed = 40f;

        [Tooltip("Downward acceleration from gravity")]
        public float GravityDownAcceleration;

        [Tooltip(
            "Distance over which the projectile will correct its course to fit the intended trajectory (used to drift projectiles towards center of screen in First Person view). At values under 0, there is no correction")]
        public float TrajectoryCorrectionDistance = -1;

        [Tooltip("Determines if the projectile inherits the velocity that the weapon's muzzle had when firing")]
        public bool InheritWeaponVelocity;

        [Header("Damage")] [Tooltip("Damage of the projectile")]
        public float Damage = 40f;

        [Header("Debug")] [Tooltip("Color of the projectile radius debug view")]
        public Color RadiusColor = Color.cyan * 0.2f;

        private Vector3 m_ConsumeTrajectoryCorrectionVector;
        private bool m_HasTrajectoryOverride;
        private List<Collider> m_IgnoreColliders;
        private Vector3 m_LastRootPosition;

        private ProjectileBase m_ProjectileBase;
        private float m_ShootTime;
        private Vector3 m_TrajectoryCorrectionVector;
        private Vector3 m_Velocity;

        private readonly RaycastHit[] hits = new RaycastHit[100]; // 用于在Update中的球体检测结果储存

        private void Update()
        {
            // Move
            transform.position += m_Velocity * Time.deltaTime;

            // Drift towards trajectory override (this is so that projectile can be centered
            // with the camera center even though the actual weapon is offset )
            if (m_HasTrajectoryOverride && m_ConsumeTrajectoryCorrectionVector.sqrMagnitude <
                m_TrajectoryCorrectionVector.sqrMagnitude)
            {
                var correctionLeft = m_TrajectoryCorrectionVector - m_ConsumeTrajectoryCorrectionVector;
                var distanceThisFrame = (Root.position - m_LastRootPosition).magnitude;
                var correctionThisFrame =
                    distanceThisFrame / TrajectoryCorrectionDistance * m_TrajectoryCorrectionVector;
                correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
                m_ConsumeTrajectoryCorrectionVector += correctionThisFrame;

                // Detect end of correction
                if (Math.Abs(m_ConsumeTrajectoryCorrectionVector.sqrMagnitude -
                             m_TrajectoryCorrectionVector.sqrMagnitude) < 0.001)
                    m_HasTrajectoryOverride = false;

                transform.position += correctionThisFrame;
            }

            // Orient towards velocity
            transform.forward = m_Velocity.normalized;

            // Gravity
            if (GravityDownAcceleration > 0)
                // add gravity to the projectile for ballistic effect
                m_Velocity += Vector3.down * (GravityDownAcceleration * Time.deltaTime);

            // hit detection
            {
                var closestHit = new RaycastHit
                {
                    distance = Mathf.Infinity
                };
                var foundHit = false;

                // sphere cast
                var displacementSinceLastFrame = Tip.position - m_LastRootPosition;
                var size = Physics.SphereCastNonAlloc(m_LastRootPosition, Radius, displacementSinceLastFrame.normalized,
                    hits, displacementSinceLastFrame.magnitude, HittableLayers, k_TriggerInteraction);
                for (var i = 0; i < size; ++i)
                    if (IsHitValid(hits[i]) && hits[i].distance < closestHit.distance)
                    {
                        foundHit = true;
                        closestHit = hits[i];
                    }

                if (foundHit)
                {
                    // handle case of casting while already inside a collider
                    if (closestHit.distance <= 0f)
                    {
                        closestHit.point = Root.position;
                        closestHit.normal = -transform.forward;
                    }

                    OnHit(closestHit.point, closestHit.normal, closestHit.collider);
                }
            }

            m_LastRootPosition = Root.position;
        }

        private void OnEnable()
        {
            m_ProjectileBase = GetComponent<ProjectileBase>();
            DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileStandard>(m_ProjectileBase, this,
                gameObject);

            m_ProjectileBase.OnShoot += OnShoot;

            Destroy(gameObject, MaxLifeTime);
        }

        private new void OnShoot()
        {
            var nowTransform = transform;
            if (InheritWeaponVelocity)
            {
                nowTransform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;
            }
            
            m_ShootTime = Time.time;
            m_LastRootPosition = Root.position;
            m_Velocity = nowTransform.forward.normalized * Speed;
            m_IgnoreColliders = new List<Collider>();
            

            // 忽略自身碰撞体
            var ownerColliders = m_ProjectileBase.Owner.GetComponentsInChildren<Collider>();
            m_IgnoreColliders.AddRange(ownerColliders);

            // ---发射子弹，判断碰撞是否合法，避免穿墙---
            var playerWeaponsManager = m_ProjectileBase.Owner.GetComponent<PlayerWeaponsManager>();
            if (playerWeaponsManager)
            {
                // m_HasTrajectoryOverride = true;
                //
                // var cameraToMuzzle = m_ProjectileBase.InitialPosition -
                //                      playerWeaponsManager.WeaponCamera.transform.position;
                //
                // m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle,
                //     playerWeaponsManager.WeaponCamera.transform.forward);
                // if (TrajectoryCorrectionDistance == 0)
                // {
                //     transform.position += m_TrajectoryCorrectionVector;
                //     m_ConsumeTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
                // }
                // else if (TrajectoryCorrectionDistance < 0f)
                // {
                //     m_HasTrajectoryOverride = false;
                // }

                if (Physics.Raycast(playerWeaponsManager.WeaponCamera.transform.position, 
                        playerWeaponsManager.WeaponCamera.transform.forward.normalized,
                        out var hit, 100f, HittableLayers, k_TriggerInteraction))
                    if (IsHitValid(hit))
                    {
                        transform.LookAt(hit.point);
                        m_Velocity = transform.forward.normalized * Speed;
                    }
            }
        }

        private void OnHit(Vector3 point, Vector3 normal, Collider hitCollider)
        {
            // damage
            // todo: AreaDamage

            var damageable = hitCollider.GetComponent<Damageable>();
            if (damageable) damageable.InflictDamage(Damage, false, m_ProjectileBase.Owner);

            // impact vfx
            if (ImpactVfx)
            {
                var impactVfxInstance = Instantiate(ImpactVfx, point + normal * ImpactVfxSpawnOffset,
                    Quaternion.LookRotation(normal));
                if (ImpactVfxLifeTime > 0) Destroy(impactVfxInstance.gameObject, ImpactVfxLifeTime);
            }

            // todo: audio

            // self destruct
            Destroy(gameObject);
        }

        private bool IsHitValid(RaycastHit hit)
        {
            // ignore hit with an ignore component
            if (hit.collider.GetComponent<IgnoreHitDetection>()) return false;

            // ignore hit with triggers that don't have damageable component
            if (hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null) return false;

            // ignore hits with specific ignored colliders (self colliders, by default)
            if (m_IgnoreColliders != null && m_IgnoreColliders.Contains(hit.collider))
                return false;

            return true;
        }
    }
}