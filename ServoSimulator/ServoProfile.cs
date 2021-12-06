using UnityEngine;

namespace ServoMotorSimulator
{
    [CreateAssetMenu(fileName = "NewServoProfile", menuName = "ServoSim2/ServoProfile")]
    public class ServoProfile : ScriptableObject
    {
        [Header("Speed")]
        [SerializeField]
        private AnimationCurve iterationSpeedCurve = AnimationCurve.Constant(-1.0f, 1.0f, 0.0f);
        [SerializeField]
        [Range(1.0f, 100.0f)]
        private float speedScholar = 1.0f;
        [Header("Dumper")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float dumperSec = 0.1f; 

        public float DumperSec => dumperSec;

        public float GetIterationSpeed(byte value)
        {
            value = value > ServoConstants.VALUE_MAX ? ServoConstants.VALUE_MAX : value;

            float normalized = value / 180.0f * 2.0f - 1.0f;
            return speedScholar * iterationSpeedCurve.Evaluate(normalized);
        }
    }
}
