using UnityEngine;

namespace ServoMotorSimulator
{
    public enum SimAxis
    {
        X,
        Y,
        Z
    }

    namespace Extensions
    {
        public static class SimAxisExtensions
        {
            public static float GetLocalTheta(this Transform tf, SimAxis axis)
            {
                return axis switch
                {
                    SimAxis.X => tf.localEulerAngles.x,
                    SimAxis.Y => tf.localEulerAngles.y,
                    SimAxis.Z => tf.localEulerAngles.z,
                    _ => 0.0f
                };
            }

            public static void SetLocalTheta(this Transform tf, SimAxis axis, float angle)
            {
                Vector3 euler = axis switch
                {
                    SimAxis.X => new Vector3(angle, 1.0f, 1.0f),
                    SimAxis.Y => new Vector3(0.0f, angle, 0.0f),
                    SimAxis.Z => new Vector3(0.0f, 0.0f, angle),
                    _ => new Vector3()
                };
                tf.localEulerAngles = euler;
            }
        }
    }
}
