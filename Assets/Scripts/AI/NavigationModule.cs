using UnityEngine;

namespace AI
{
    public class NavigationModule : MonoBehaviour
    {
        [Header("Parameters")] [Tooltip("The maximum speed at which the enemy is moving (in world units per second)")]
        public float MoveSpeed;

        [Tooltip("The maximum speed at which the enemy is rotating (degrees per second")]
        public float AngularSpeed;

        [Tooltip("The acceleration to reach the maximum speed (in world units per second squared")]
        public float Acceleration;
    }
}