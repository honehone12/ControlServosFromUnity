using UnityEngine;

namespace ServoMotorSimulator.ML
{
    [RequireComponent(typeof(BoxCollider))]
    public class Stage : MonoBehaviour
    {
        [SerializeField]
        private Color gizmoColor = Color.cyan;
        [SerializeField]
        private bool showGizmos;

        private BoxCollider bounds;

        public void GenerateRandomPosition(out Vector3 pos)
        {
            pos = new Vector3(
                Random.Range(bounds.bounds.min.x, bounds.bounds.max.x),
                Random.Range(bounds.bounds.min.y, bounds.bounds.max.y),
                Random.Range(bounds.bounds.min.z, bounds.bounds.max.z)
            );
        }

        private void Awake()
        {
            bounds = GetComponent<BoxCollider>();
        }

        private void OnDrawGizmos()
        {
            if(bounds && showGizmos)
            {
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireCube(
                    bounds.center,
                    bounds.size
                );
            }
        }
    }
}