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

        private Rigidbody ballRB;
        private Stage stage;
        private VirtualServoAgent agent;

        public Vector3 Position => ballRB.position;

        public void SetRandomPosition()
        {
            stage.GenerateRandomPosition(out Vector3 pos);
            ballRB.Sleep();
            ballRB.transform.position = pos;
            ballRB.WakeUp();
        }

        private void Awake()
        {
            ballRB = GetComponent<Rigidbody>();
            Transform root = transform.root;
            stage = root.GetComponentInChildren<Stage>();
            if(!stage)
            {
                Debug.LogError("stage component was not found.");
            }

            agent = root.GetComponentInChildren<VirtualServoAgent>();
            if (!agent)
            {
                Debug.LogError("could not fined agent.");
            }
        }

        private void FixedUpdate()
        {
            Vector3 rand3dir = new Vector3(
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f),
                Random.Range(-1.0f, 1.0f)
            );
        
            ballRB.AddForce(rand3dir * Random.Range(forceMin, forceMax), forceMode);
        }

        //private void OnCollisionEnter(Collision collision)
        //{
        //    agent.EndEpisode();
        //    //Debug.Log("ball collision with " + collision.gameObject.name);
        //}
    }
}


