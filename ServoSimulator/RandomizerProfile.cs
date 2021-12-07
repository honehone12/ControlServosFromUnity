using UnityEngine;

namespace ServoMotorSimulator
{
    [CreateAssetMenu(fileName = "NewRandomizerProfile", menuName = "ServoSim2/RandomizerProfile")]
    public class RandomizerProfile : ScriptableObject
    {
        [Header("Scholar")]
        [SerializeField]
        private float genMin;
        [SerializeField]
        private float genMax;
        [Header("Period")]
        [SerializeField]
        private int periodMin;
        [SerializeField]
        private int periodMax;
        
        public float GenerateRandomScholar => Random.Range(genMin, genMax);

        public int GetRandomPeriod => Random.Range(periodMin, periodMax + 1);
    }
}