using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class PatrolPath : MonoBehaviour
    {
        //Enemies that will be assigned to this path
        public List<EnemyController> EnemiesToAssign = new();

        //The nodes making up the path
        public List<Transform> PathNodes = new();

        private void Start()
        {
            foreach (var enemy in EnemiesToAssign) enemy.PatrolPath = this;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            for (var i = 0; i < PathNodes.Count; i++)
            {
                var nextIndex = i + 1;
                if (nextIndex >= PathNodes.Count) nextIndex -= PathNodes.Count;

                Gizmos.DrawLine(PathNodes[i].position, PathNodes[nextIndex].position);
                Gizmos.DrawSphere(PathNodes[i].position, 0.1f);
            }
        }

        public float GetDistanceToNode(Vector3 origin, int destinationNodeIndex)
        {
            if (destinationNodeIndex < 0 || destinationNodeIndex > PathNodes.Count || PathNodes == null) return -1;

            return (PathNodes[destinationNodeIndex].position - origin).magnitude;
        }

        public Vector3 GetPositionOfPathNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex > PathNodes.Count || PathNodes == null) return Vector3.zero;

            return PathNodes[nodeIndex].position;
        }
    }
}