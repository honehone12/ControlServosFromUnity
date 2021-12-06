namespace ServoMotorSimulator
{
    public struct LimitationCheckInfo
    {
        public bool result;
        public float theta;
        public float delta;
    }

    public delegate void LimitationCheckEvent(ref LimitationCheckInfo info);
}