using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnitySerialPort;

namespace ServoMotorSimulator.ML
{
    [RequireComponent(typeof(SerialServoDriver))]
    public class InferenceOnlyAgent : Agent
    {
        [SerializeField]
        private List<ServoSimulator> servoSimsList = new List<ServoSimulator>();
        [SerializeField]
        private Ball ball;
        [SerializeField]
        private Raycaster raycaster;
        [SerializeField]
        private SerialServoDriver serialServoDriver;
        [SerializeField]
        private float writingHZ = 30.0f;

        public override void Initialize()
        {
            if (servoSimsList.Count == 0)
            {
                servoSimsList.AddRange(GetComponentsInChildren<ServoSimulator>());
                if (servoSimsList.Count != 2)
                {
                    Debug.LogError("could not find servo sims. or found more than 2 sims.");
                }
            }

            servoSimsList.ForEach(
                (sim) => sim.OnDumperWorked += () => AddReward(-0.005f)
            );

            if (!ball)
            {
                ball = transform.root.GetComponentInChildren<Ball>();
                if (!ball)
                {
                    Debug.LogError("could not find ball.");
                }
            }

            if (!raycaster)
            {
                raycaster = GetComponentInChildren<Raycaster>();
                if (!raycaster)
                {
                    Debug.LogError("could not find raycaster.");
                }
            }

            if(!serialServoDriver)
            {
                serialServoDriver = GetComponent<SerialServoDriver>();
                if(!serialServoDriver)
                {
                    Debug.LogError("could not find serial servo driver.");
                }
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            float angle0 = servoSimsList[0].VirtualFeedBack;
            float angle1 = servoSimsList[1].VirtualFeedBack;
            //angle0 += Random.Range(-0.1f, 0.1f);
            //angle1 += Random.Range(-0.1f, 0.1f);
            sensor.AddObservation(angle0);
            sensor.AddObservation(angle1);

            //this means camera's transform.
            Transform lookingEdge = raycaster.Muzzle;
            Vector3 pos = ball.GetPosition;
            //pos.x += Random.Range(-0.1f, 0.1f);
            //pos.y += Random.Range(-0.1f, 0.1f);
            //pos.z += Random.Range(-0.1f, 0.1f);
            sensor.AddObservation(
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
                if (acts[i] > baseMax)
                {
                    baseMax = acts[i];
                    baseActIdx = i;
                }
            }

            for (int i = 0; i <= 18; i++)
            {
                if (acts[i + 19] > middleMax)
                {
                    middleMax = acts[i + 19];
                    middleActIdx = i;
                }
            }

            if (baseActIdx >= 0)
            {
                servoSimsList[0].Value = (byte)(baseActIdx * 10);
                //Debug.LogFormat("{0} => {1}", baseActIdx, servoSimsList[0].Value);
            }
            if (middleActIdx >= 0)
            {
                servoSimsList[1].Value = (byte)(middleActIdx * 10);
                //Debug.LogFormat("{0} => {1}", middleActIdx, servoSimsList[1].Value);
            }

            //Write();

            if (raycaster.Raycast(out _))
            {
                Debug.Log("hit." + transform.root.name);
            }
        }

        // for hiding warning messages.
        public override void Heuristic(in ActionBuffers actionsOut)
        { }

        private IEnumerator Start()
        {
            yield return new WaitUntil(
                () => serialServoDriver.IsInitialized
            );

            // wait one more frame for getting initial feedback.
            yield return null;

            float[] feedback = serialServoDriver.RequestFeedbackData;
            for (int i = 0; i < feedback.Length; i++)
            {
                if (float.IsFinite(feedback[i]))
                {
                    servoSimsList[i].SetAngle(feedback[i]);
                }
            }

            StartCoroutine(WritingRoutine());
        }

        protected virtual void FixedUpdate()
        {
            float[] feedback = serialServoDriver.RequestFeedbackData;
            for (int i = 0; i < feedback.Length; i++)
            {
                if (float.IsFinite(feedback[i]))
                {
                    servoSimsList[i].SetAngle(feedback[i]);
                }
                servoSimsList[i].LimitationCheck(out _);
                //Debug.LogFormat("read {0} {1}", i, feedback[i]);
            }
        }

        private IEnumerator WritingRoutine()
        {
            while (isActiveAndEnabled)
            {
                Write();

                yield return new WaitForSeconds(1.0f / writingHZ);
            }
        }

        private void Write()
        {
            byte[] buff = serialServoDriver.RequestWritingBuffer;
            for (int i = 0; i < buff.Length; i++)
            {
                if (servoSimsList[i].WithinLimitation)
                {
                    buff[i] = servoSimsList[i].Value;
                }
                else
                {
                    buff[i] = ServoConstants.VALUE_STOPPING;
                }
                //Debug.LogFormat("write {0} {1}", i, buff[i]);
            }
            serialServoDriver.Write();
        }
    }
}
