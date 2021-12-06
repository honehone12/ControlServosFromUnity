using UnityEngine;

namespace ServoMotorSimulator.ML
{
    public class Raycaster : MonoBehaviour
    {
        [SerializeField]
        private Transform muzzle;
        [SerializeField]
        private float maxRange;
        [SerializeField]
        private float rayRadius;
        [SerializeField]
        private LayerMask layerMask;
        [Space]
        [SerializeField]
        private Color gizmoColor = Color.magenta;
        [SerializeField]
        private bool showGizmo;

        public Transform Muzzle => muzzle;

        public bool Raycast(out RaycastHit info)
        {
            return Physics.SphereCast(
                muzzle.position,
                rayRadius,
                muzzle.forward,
                out info,
                maxRange,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
        }

        private void OnDrawGizmos()
        {
            if (showGizmo)
            {
                if(!muzzle)
                {
                    muzzle = transform;
                    Debug.LogWarning("muzzle was assigned as this transform.");
                }

                Gizmos.color = gizmoColor;
                Vector3 ori = muzzle.position;
                Vector3 to = ori + muzzle.forward * maxRange;
                Gizmos.DrawLine(ori, to);
                Gizmos.DrawWireSphere(to, rayRadius);
            }
        }
    }
}