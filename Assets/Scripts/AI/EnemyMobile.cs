using System;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI
{
    [RequireComponent(typeof(EnemyController))]
    public class EnemyMobile : MonoBehaviour
    {
        public enum AIState
        {
            Patrol,
            Follow,
            Attack
        }

        public Animator Animator;

        [Tooltip("Fraction of the enemy's attack range at which it will stop moving towards target while attacking")]
        [Range(0f, 1f)]
        public float AttackStopDistanceRatio = 0.5f;

        [Tooltip("The random hit damage effects")]
        public ParticleSystem[] RandomHitSparks;

        public ParticleSystem[] OnDetectVfx;
        
        public AIState AiState { get; private set; }
        private EnemyController m_EnemyController;

        private const string k_AnimMoveSpeedParameter = "MoveSpeed";
        private const string k_AnimAttackParameter = "Attack";
        private const string k_AnimAlertedParameter = "Alerted";
        private const string k_AnimOnDamagedParameter = "OnDamaged";

        private void Start()
        {
            m_EnemyController = GetComponent<EnemyController>();
            DebugUtility.HandleErrorIfNullGetComponent<EnemyController, EnemyMobile>(m_EnemyController, this, gameObject);

            m_EnemyController.onAttack += OnAttack;
            m_EnemyController.onDetectedTarget += OnDetectedTarget;
            m_EnemyController.onLostTarget += OnLostTarget;
            m_EnemyController.onDamaged += OnDamaged;
            m_EnemyController.SetPathDestinationToClosestNode();
            
            // Start patrolling
            AiState = AIState.Patrol;
            
            // todo: Audio
        }

        private void Update()
        {
            UpdateAiStateTransitions();
            UpdateCurrentAiState();

            var moveSpeed = m_EnemyController.NavMeshAgent.velocity.magnitude;
            
            // todo: Animator
            
            // todo: sound
        }

        private void UpdateAiStateTransitions()
        {
            switch (AiState)
            {
                case AIState.Follow:
                    if (m_EnemyController.IsSeeingTarget && m_EnemyController.IsTargetInAttackRange)
                    {
                        Debug.Log("In attack range");
                        AiState = AIState.Attack;
                        m_EnemyController.SetNavDestination(transform.position);
                    }
                    break;
                case AIState.Attack:
                    if (!m_EnemyController.IsTargetInAttackRange)
                    {
                        AiState = AIState.Follow;
                    }
                    break;
            }
        }

        private void UpdateCurrentAiState()
        {
            switch (AiState)
            {
                case AIState.Patrol:
                    m_EnemyController.UpdatePathDestination();
                    m_EnemyController.SetNavDestination(m_EnemyController.GetDestinationOnPath());
                    break;
                case AIState.Follow:
                    var tPosition = m_EnemyController.KnownDetectedTarget.transform.position;
                    m_EnemyController.SetNavDestination(tPosition);
                    m_EnemyController.OrientTowards(tPosition);
                    // todo: Rotate Weapon
                    break;
                case AIState.Attack:
                    if (Vector3.Distance(m_EnemyController.KnownDetectedTarget.transform.position,
                            m_EnemyController.DetectionModule.DetectionSourcePoint.position)
                        >= (AttackStopDistanceRatio * m_EnemyController.DetectionModule.AttackRange))
                    {
                        m_EnemyController.SetNavDestination(m_EnemyController.KnownDetectedTarget.transform.position);
                    }
                    else
                    {
                        m_EnemyController.SetNavDestination(transform.position);
                    }
                    
                    m_EnemyController.OrientTowards(m_EnemyController.KnownDetectedTarget.transform.position);
                    m_EnemyController.TryAttack(m_EnemyController.KnownDetectedTarget.transform.position);
                    break;
            }
        }

        private void OnAttack()
        {
            // todo: Animator
        }

        private void OnDetectedTarget()
        {
            if (AiState == AIState.Patrol)
            {
                AiState = AIState.Follow;
            }

            foreach (var t in OnDetectVfx)
            {
                t.Play();
            }
            
            // todo: Animator
        }

        private void OnLostTarget()
        {
            if (AiState == AIState.Follow || AiState == AIState.Attack)
            {
                AiState = AIState.Patrol;
            }

            foreach (var v in OnDetectVfx)
            {
                v.Stop();
            }
            
            // todo: Animator
        }

        private void OnDamaged()
        {
            if (RandomHitSparks.Length > 0)
            {
                int n = Random.Range(0, RandomHitSparks.Length - 1);
                RandomHitSparks[n].Play();
            }
            
            // todo: Animator
        }
    }
}