using UnityEngine;

namespace ServoMotorSimulator.ML
{
    [RequireComponent(typeof(Rigidbody))]
    public class Ball : MonoBehaviour
    {
        [SerializeField]
        private ForceMode forceMode;
        [SerializeField]
        [Range(0f, 100f)]
        private float forceMin;
        [SerializeField]
        [Range(0f, 100f)]
        private float forceMax;

        protected Rigidbody ballRB;
        protected Rigidbody fakeCenter;
        private Stage stage;

        public Vector3 GetPosition => ballRB.position;
        public Vector3 GetFakePosition => fakeCenter.position;

        public virtual void SetRandomPosition()
        {
            stage.GenerateRandomPosition(out Vector3 pos);
            ballRB.Sleep();
            ballRB.transform.position = pos;
            ballRB.WakeUp();
        }

        protected virtual void Awake()
        {
            Transform tf = transform;
            ballRB = GetComponent<Rigidbody>();
            if(tf.childCount > 0)
            {
                fakeCenter = tf.GetChild(0).GetComponent<Rigidbody>();
            }
            stage = tf.root.GetComponentInChildren<Stage>();
            if(!stage)
            {
                Debug.LogError("stage component was not found.");
            }
        }

        protected virtual void FixedUpdate()
        {
            Vector3 rand3dir = new Vector3(
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f)
            );
        
            ballRB.AddForce(rand3dir * Random.Range(forceMin, forceMax), forceMode);
        }
    }
}


