using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ServoMotorSimulator;

namespace UnitySerialPort
{
    public abstract class SerialServoControllerBase : MonoBehaviour
    {
        [Header("SerialDriver")]
        [SerializeField]
        protected SerialServoDriver servoDriver;
        [SerializeField]
        protected float writingHZ = 30;
        [Header("VirtualServos")]
        [SerializeField]
        protected List<ServoSimulator> simulatorsList = new List<ServoSimulator>();

        protected float writingTime;

        protected virtual void Awake()
        {
            if (simulatorsList.Count == 0)
            {
                // base->middle(->edge)
                simulatorsList.AddRange(
                    GetComponentsInChildren<ServoSimulator>()
                );
                if(simulatorsList.Count == 0)
                {
                    Debug.LogError("need at learst one VirtualServo. please add VirtualServos in children. : " + name);
                }
            }

            if (!servoDriver)
            {
                // try get once
                if(!TryGetComponent<SerialServoDriver>(out servoDriver))
                {
                    // if none this does not use serial port
                    simulatorsList.ForEach(
                        (sim) => sim.Mode = SimMode.Virtual
                    );
                    Debug.LogWarning("could not find SerialDriver. if not in use, please ignore this message. : " + name);
                    return;
                }
            }

            // set servo modes
            simulatorsList.ForEach(
                (sim) => sim.Mode = SimMode.SerialViz
            );
        }

        protected virtual IEnumerator Start()
        {
            if(servoDriver)
            {
                // wait for serial port.
                yield return new WaitUntil(
                    () => servoDriver.IsInitialized
                );

                writingTime = 1.0f / writingHZ;
            }
        }

        protected virtual void Update()
        {
            if(servoDriver)
            {
                float[] feedback = servoDriver.RequestFeedbackData;
                for (int i = 0; i < feedback.Length; i++)
                {
                    if(float.IsFinite(feedback[i]))
                    {
                        simulatorsList[i].SetAngle(feedback[i]);
                    }
                    simulatorsList[i].LimitationCheck(out _);
                    Debug.LogFormat("read {0} {1}", i, feedback[i]);
                }
            }
        }

        protected void Write()
        {
            if (servoDriver)
            {
                byte[] buff = servoDriver.RequestWritingBuffer;
                for (int i = 0; i < buff.Length; i++)
                {
                    buff[i] = simulatorsList[i].Value;
                    //Debug.LogFormat("write {0} {1}", i, buff[i]);
                }
                servoDriver.Write();
                //Debug.LogFormat("write {0} {1}", buff[0], buff[1]);
            }
        }
    }
}