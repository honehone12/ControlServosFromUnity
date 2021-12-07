using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ServoMotorSimulator.ROS
{
    public class ROSBall : ML.Ball
    {
        [SerializeField]
        private Transform origin;
        Transform ballTF;

        public Vector3 SetPosition
        {
            set
            {
                ballTF.position = origin.TransformPoint(value);
            }
        }

        public override void SetRandomPosition()
        { /*do nothing now*/ }

        protected override void Awake()
        {
            base.Awake();
            if(!origin)
            {
                origin = Camera.main.transform;
                if(!origin)
                {
                    Debug.LogError("serialize origin before play.");
                }
            }
            ballTF = transform;
            if (!ballRB.isKinematic)
            {
                ballRB.isKinematic = true;
            }
        }

        protected override void FixedUpdate()
        { /*do nothing now*/ }
    }
}
