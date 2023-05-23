using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AI
{
    public class DetectionModule : MonoBehaviour
    {
        private const string k_AnimAttackParameter = "Attack";

        private const string k_AnimOnDamageParaMeter = "OnDamage";

        //The point representing the source of target-detection raycasts for the enemy AI
        public Transform DetectionSourcePoint;

        //The max distance at which the enemy can see targets
        public float DetectionRange = 20f;

        //The max distance at which the enemy can attack its targets
        public float AttackRange = 10f;

        //Time before an enemy abandons a known target that it can't see anymore
        public float KnownTargetTimeout = 4f;

        //Optional animator for OnShoot Animations
        public Animator Animator;

        private ActorsManager m_ActorsManager;

        public UnityAction onDetectedTarget;
        public UnityAction onLostTarget;

        protected float TimeLastSeenTarget = Mathf.NegativeInfinity;

        public DetectionModule(UnityAction onDetectedTarget)
        {
            this.onDetectedTarget = onDetectedTarget;
        }

        public GameObject KnownDetectedTarget { get; private set; }
        public bool IsTargetInAttackRange { get; private set; }
        public bool IsSeeingTarget { get; private set; }
        public bool HadKnownTarget { get; private set; }

        protected virtual void Start()
        {
            m_ActorsManager = FindObjectOfType<ActorsManager>();
        }

        public virtual void HandleTargetDetection(Actor actor, Collider[] selfColliders)
        {
            if (KnownDetectedTarget && !IsSeeingTarget && Time.time - TimeLastSeenTarget > KnownTargetTimeout)
                KnownDetectedTarget = null;

            //遍历寻找最近的敌对目标（待优化，如果没有enemy内部冲突设定，那么只用判断player就可以）
            var sqrDetectionRange = DetectionRange * DetectionRange;
            IsSeeingTarget = false;
            var closestSqrDistance = Mathf.Infinity;
            foreach (var otherActor in m_ActorsManager.Actors)
                if (otherActor.Affiliation != actor.Affiliation)
                {
                    var sqrDistance = (otherActor.transform.position - DetectionSourcePoint.transform.position)
                        .sqrMagnitude;
                    if (sqrDistance < sqrDetectionRange && sqrDistance < closestSqrDistance)
                    {
                        var results = new NativeArray<RaycastHit>(1, Allocator.TempJob);
                        var commands = new NativeArray<RaycastCommand>(1, Allocator.TempJob);
                        var closestValidHit = new RaycastHit { distance = Mathf.Infinity };
                        var position = DetectionSourcePoint.position;
                        commands[0] = new RaycastCommand(position,
                            (otherActor.AimPoint.position - position).normalized,
                            new QueryParameters(-1), DetectionRange);
                        var handle = RaycastCommand.ScheduleBatch(commands, results, 1, 1);
                        handle.Complete();
                        var foundValidHit = false;
                        if (results[0].collider != null)
                            if (!selfColliders.Contains(results[0].collider) &&
                                results[0].distance < closestValidHit.distance)
                            {
                                closestValidHit = results[0];
                                foundValidHit = true;
                            }

                        if (foundValidHit)
                        {
                            var hitActor = closestValidHit.collider.GetComponentInParent<Actor>();
                            if (hitActor != otherActor) return;

                            IsSeeingTarget = true;
                            closestSqrDistance = sqrDistance;
                            TimeLastSeenTarget = Time.time;
                            KnownDetectedTarget = otherActor.AimPoint.gameObject;
                        }
                    }
                }

            IsTargetInAttackRange = KnownDetectedTarget != null &&
                                    Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <=
                                    AttackRange;

            if (!HadKnownTarget && KnownDetectedTarget != null) OnDetect();

            if (HadKnownTarget && KnownDetectedTarget == null) OnLostTarget();

            HadKnownTarget = KnownDetectedTarget != null;
        }

        public virtual void OnLostTarget()
        {
            onLostTarget?.Invoke();
        }

        public virtual void OnDetect()
        {
            onDetectedTarget?.Invoke();
        }

        public virtual void OnDamaged(GameObject damageSource)
        {
            TimeLastSeenTarget = Time.time;
            KnownDetectedTarget = damageSource;

            if (Animator) Animator.SetTrigger(k_AnimOnDamageParaMeter);
        }

        public virtual void OnAttack()
        {
            if (Animator) Animator.SetTrigger(k_AnimAttackParameter);
        }
    }
}