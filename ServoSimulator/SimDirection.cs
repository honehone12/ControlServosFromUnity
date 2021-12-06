namespace ServoMotorSimulator
{
    public enum SimDirection : int
    {
        None = 0,
        Forward = 1,
        Reversal = -1
    }

    namespace Extensions
    {
        public static class SimDirectionsExtensions
        {
            public static float ToFloat(this SimDirection dir)
            {
                return (float)dir;
            }

            public static SimDirection ToSimDirection(this byte value)
            {
                return value switch
                {
                    > 90 => SimDirection.Forward,
                    < 90 => SimDirection.Reversal,
                    90 => SimDirection.None
                };
            }

            public static SimDirection Blend(this SimDirection origin, SimDirection blend)
            {
                if(origin == SimDirection.None || blend == SimDirection.None)
                {
                    return SimDirection.None;
                }
                else if(origin == blend)
                {
                    return SimDirection.Forward;
                }
                else
                {
                    return SimDirection.Reversal;
                }
            }
        }
    }
}
