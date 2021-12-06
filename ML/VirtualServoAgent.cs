using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

namespace ServoMotorSimulator.ML
{
    public class VirtualServoAgent : Agent
    {
        [SerializeField]
        private List<ServoSimulator> servoSimsList = new List<ServoSimulator>();
        [SerializeField]
        private Ball ball;
        [SerializeField]
        private Raycaster raycaster;
        [SerializeField]
        private Transform debugIdealBox;

        private uint falseCount;
        private uint rewardCount;
        private const uint MAX_FALSE_COUNT = 1000;
        private const uint MAX_REWARD_COUNT = 1000;

        public override void Initialize()
        {
            if(servoSimsList.Count == 0)
            {
                servoSimsList.AddRange(GetComponentsInChildren<ServoSimulator>());
                if(servoSimsList.Count != 2)
                {
                    Debug.LogError("could not find servo sims. or found more than 2 sims.");
                }
            }

            servoSimsList.ForEach(
                (sim) => sim.OnDumperWorked += () => AddReward(-0.005f)
            );

            if(!ball)
            {
                ball = transform.root.GetComponentInChildren<Ball>();
                if(!ball)
                {
                    Debug.LogError("could not find ball.");
                }
            }

            if (!raycaster)
            {
                raycaster = GetComponentInChildren<Raycaster>();
                if(!raycaster)
                {
                    Debug.LogError("could not find raycaster.");
                }
            }
        }

        public override void OnEpisodeBegin()
        {
            ball.SetRandomPosition();
            falseCount = 0;
            rewardCount = 0;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            float angle0 = servoSimsList[0].FeedBack;
            float angle1 = servoSimsList[1].FeedBack;
            //angle0 += Random.Range(-0.1f, 0.1f);
            //angle1 += Random.Range(-0.1f, 0.1f);
            sensor.AddObservation(angle0);
            sensor.AddObservation(angle1);
            //Debug.LogFormat(
            //    "feedback[0] {0} [1] {1}", 
            //    servoSimsList[0].FeedBack, 
            //    servoSimsList[1].FeedBack
            //);
            //this means camera's transform.
            Transform lookingEdge = raycaster.Muzzle;
            //camera has imu. so obserb it now.
            //sensor.AddObservation(
            //    lookingEdge.rotation
            //);
            //now add error
            Vector3 pos = ball.Position;
            //pos.x += Random.Range(-0.1f, 0.1f);
            //pos.y += Random.Range(-0.1f, 0.1f);
            //pos.z += Random.Range(-0.1f, 0.1f);
            sensor.AddObservation(
                //////////////////////////////////////////////////////////
                // values do not respond with actions are bad observations
                // => origin.InverseTransformPoint(ball.Position)
                /////////////////////////////////////////////////////////
                lookingEdge.InverseTransformPoint(pos)
            );
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            ActionSegment<float> acts = actions.ContinuousActions;
            float baseMax = float.MinValue;
            int baseActIdx = -1;
            float middleMax = float.MinValue;
            int middleActIdx = -1;

            for (int i = 0; i <= 18; i++)
            {
                if(acts[i] > baseMax)
                {
                    baseMax = acts[i];
                    baseActIdx = i;
                }
            }

            for (int i = 0; i <= 18; i++)
            {
                if(acts[i + 19] > middleMax)
                {
                    middleMax = acts[i + 19];
                    middleActIdx = i;
                }
            }

            if(baseActIdx >= 0)
            {
                servoSimsList[0].Value = (byte)(baseActIdx * 10);
                //Debug.LogFormat("{0} => {1}", baseActIdx, servoSimsList[0].Value);
            }
            if(baseActIdx != 9)
            {
                AddReward(-0.005f);
            }
            if(middleActIdx >= 0)
            {
                servoSimsList[1].Value = (byte)(middleActIdx * 10);
                //Debug.LogFormat("{0} => {1}", middleActIdx, servoSimsList[1].Value);
            }
            if(middleActIdx != 9)
            {
                AddReward(-0.005f);
            }

            Transform muzzle = raycaster.Muzzle;
            Vector3 dir = (ball.Position - muzzle.position).normalized;
            Quaternion ideal = Quaternion.LookRotation(dir);
            if(debugIdealBox)
            {
                debugIdealBox.rotation = ideal;
            }
            float angle = Quaternion.Angle(muzzle.rotation, ideal);
            //Debug.Log(angle);
            AddReward(angle * -0.001f);

            if(raycaster.Raycast(out _))
            {
                //Debug.Log("hit." + transform.root.name);
                AddReward(0.01f);
                if(++rewardCount >= MAX_REWARD_COUNT)
                {
                    AddReward(10.0f);
                    EndEpisode();
                }
            }
            else
            {
                if(++falseCount >= MAX_FALSE_COUNT)
                {
                    EndEpisode();
                }
            }
        }

        // for hiding warning messages.
        public override void Heuristic(in ActionBuffers actionsOut)
        { }
    }
}
