using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace ServoMotorSimulator.ROS
{
    public class ROSConnector : MonoBehaviour
    {
        [SerializeField]
        private float scaleModifier;
        
        private ROSBall ball;
        
        ROSConnection connection;

        private void Awake()
        {
            if(!ball)
            {
                if (!(ball = transform.root.GetComponentInChildren<ROSBall>()))
                {
                    Debug.LogError("could not find ros ball.");
                }
            }
        }

        private void Start()
        {
            connection = ROSConnection.GetOrCreateInstance();
            connection.Subscribe<RosMessageTypes.Geometry.PoseStampedMsg>(
                ROSTopicNames.NearestHumanPoseName,
                OnMessageRecieved
            );
        }

        private void OnMessageRecieved(RosMessageTypes.Geometry.PoseStampedMsg msg)
        {
            Vector3 uniPos = new Vector3(
                (float)msg.pose.position.x * scaleModifier,
                (float)msg.pose.position.y * scaleModifier,
                (float)msg.pose.position.z * scaleModifier
            );
            Debug.LogFormat(
                "position recieved {0}",
                uniPos
            );

            float dist = Vector3.Distance(Vector3.zero, uniPos);

            if(dist > 4.0f)
            {
                ball.SetPosition = uniPos;
            }
        }
    }
}